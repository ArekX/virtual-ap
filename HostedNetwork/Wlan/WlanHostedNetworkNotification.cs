/**
 *  Virtual AP
 *  Copyright (C) Panic Aleksandar
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HostedNetwork.Wlan
{
    /// <summary>
    /// Class which handles hosted network notifications.
    /// </summary>
    class WlanHostedNetworkNotification : IDisposable
    {
        /// <summary>
        /// Notification source
        /// </summary>
        public static WlanApi.WLAN_NOTIFICATION_SOURCE notifySource;

        /// <summary>
        /// Callback which will be called when wlan notification happens.
        /// </summary>
        WlanApi.WlanNotificationDelegate notificationCallback;

        /// <summary>
        /// Client handle which is used to open the Wlan API
        /// </summary>
        IntPtr clientHandle;

        /// <summary>
        /// Wlan state changed delegate.
        /// </summary>
        /// <param name="oldState">State that was previously set.</param>
        /// <param name="newState">New state which is now set.</param>
        /// <param name="reason">Reson for change.</param>
        public delegate void WlanStateChangeEvent(WlanApi.WLAN_HOSTED_NETWORK_STATE oldState, WlanApi.WLAN_HOSTED_NETWORK_STATE newState, WlanApi.WLAN_HOSTED_NETWORK_REASON reason);

        /// <summary>
        /// Wlan peer state changed delegate.
        /// </summary>
        /// <param name="oldState">State that was previously set.</param>
        /// <param name="newState">New state which is now set.</param>
        /// <param name="reason">Reson for change.</param>
        public delegate void WlanPeerStateChangeEvent(WlanApi.WLAN_HOSTED_NETWORK_PEER_STATE oldState, WlanApi.WLAN_HOSTED_NETWORK_PEER_STATE newState, WlanApi.WLAN_HOSTED_NETWORK_REASON reason);
        
        /// <summary>
        /// Wlan radio state changed delegate.
        /// </summary>
        /// <param name="hardwareState">New state of wlan on hardware level.</param>
        /// <param name="softwareState">New state of wlan on software level.</param>
        public delegate void WlanRadioStateChangeEvent(WlanApi.DOT11_RADIO_STATE hardwareState, WlanApi.DOT11_RADIO_STATE softwareState);

        /// <summary>
        /// Event which fires when wlan state is changed.
        /// </summary>
        public event WlanStateChangeEvent StateChanged;

        /// <summary>
        /// Event which fires when wlan peer state is changed.
        /// </summary>
        public event WlanPeerStateChangeEvent PeerStateChanged;

        /// <summary>
        /// Event which fires when wlan hardware/software radio state is changed.
        /// </summary>
        public event WlanRadioStateChangeEvent RadioStateChanged;

        /// <summary>
        /// Initializes new wlan hosted network notification.
        /// </summary>
        /// <param name="clientHandle">Already opened handle using WlanOpenHandle</param>
        public WlanHostedNetworkNotification(IntPtr clientHandle)
        {
            this.notificationCallback = new WlanApi.WlanNotificationDelegate(WlanNotificationHandler);

            if (WlanApi.WlanRegisterNotification(
                clientHandle,
                WlanApi.WLAN_NOTIFICATION_SOURCE.HNWK | WlanApi.WLAN_NOTIFICATION_SOURCE.MSM,
                true,
                this.notificationCallback,
                IntPtr.Zero,
                IntPtr.Zero,
                out notifySource
                ) != 0)
            {
                throw new Exception("Unable to register wlan notification.");
            }

            this.clientHandle = clientHandle;
        }


        /// <summary>
        /// Unregisters all notifcation from handle and performs cleanup.
        /// </summary>
        public void Dispose()
        {
            if (WlanApi.WlanRegisterNotification(
                clientHandle,
                WlanApi.WLAN_NOTIFICATION_SOURCE.None,
                true,
                null,
                IntPtr.Zero,
                IntPtr.Zero,
                out notifySource
            ) != 0)
            {
                throw new Exception("Unable to unregister wlan notification");
            }
        }

        /// <summary>
        /// Notifcation handler function which is called when WlanAPI notification is fired.
        /// </summary>
        /// <param name="data">Pointer to notifcation data.</param>
        /// <param name="context">Pointer to context which is sent along with this data.</param>
        private void WlanNotificationHandler(IntPtr data, IntPtr context)
        {
            WlanApi.WLAN_NOTIFICATION_DATA notificationData;

            notificationData = WlanApi.MarshalDataToStructure<WlanApi.WLAN_NOTIFICATION_DATA>(data);

            bool isHostedNetworkSource = notificationData.notificationSource == WlanApi.WLAN_NOTIFICATION_SOURCE.HNWK;
            bool isMSMNetworkSource = notificationData.notificationSource == WlanApi.WLAN_NOTIFICATION_SOURCE.MSM;

            switch (notificationData.notificationCode)
            {
                case (int)WlanApi.WLAN_HOSTED_NETWORK_NOTIFICATION_CODE.StateChange:

                    if (notificationData.dataSize > 0 && notificationData.dataPtr != IntPtr.Zero && isHostedNetworkSource)
                    {
                        WlanApi.WLAN_HOSTED_NETWORK_STATE_CHANGE pStateChange =
                            WlanApi.MarshalDataToStructure<WlanApi.WLAN_HOSTED_NETWORK_STATE_CHANGE>(notificationData.dataPtr);

                        this.StateChanged(pStateChange.OldState, pStateChange.NewState, pStateChange.Reason);
                    }

                    break;

                case (int)WlanApi.WLAN_HOSTED_NETWORK_NOTIFICATION_CODE.PeerStateChange:

                    if (notificationData.dataSize > 0 && notificationData.dataPtr != IntPtr.Zero && isHostedNetworkSource)
                    {
                        WlanApi.WLAN_HOSTED_NETWORK_DATA_PEER_STATE_CHANGE pPeerStateChange =
                            WlanApi.MarshalDataToStructure<WlanApi.WLAN_HOSTED_NETWORK_DATA_PEER_STATE_CHANGE>(notificationData.dataPtr);

                        this.PeerStateChanged(pPeerStateChange.OldState, pPeerStateChange.NewState, pPeerStateChange.Reason);
                    }

                    break;

                case (int)WlanApi.WLAN_HOSTED_NETWORK_NOTIFICATION_CODE.RadioStateChange:
                    if (notificationData.dataSize > 0 && notificationData.dataPtr != IntPtr.Zero && isHostedNetworkSource)
                    {
                        WlanApi.WLAN_HOSTED_NETWORK_RADIO_STATE radioState =
                            WlanApi.MarshalDataToStructure<WlanApi.WLAN_HOSTED_NETWORK_RADIO_STATE>(notificationData.dataPtr);

                        this.RadioStateChanged(radioState.dot11SoftwareRadioState, radioState.dot11SoftwareRadioState);

                    }
                    break;
                case (int)WlanApi.WLAN_NOTIFICATION_CODE_MSM.RadioStateChange:
                    if (notificationData.dataSize > 0 && notificationData.dataPtr != IntPtr.Zero && isMSMNetworkSource)
                    {
                        WlanApi.WLAN_PHY_RADIO_STATE radioState =
                            WlanApi.MarshalDataToStructure<WlanApi.WLAN_PHY_RADIO_STATE>(notificationData.dataPtr);

                        this.RadioStateChanged(radioState.dot11SoftwareRadioState, radioState.dot11SoftwareRadioState);

                    }
                    break;

            }
        }

    }
}
