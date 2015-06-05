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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NETCONLib;

namespace HostedNetwork.Wlan
{
    /// <summary>
    /// Wrapper class for handling WLAN hosted network operations.
    /// </summary>
    class WlanHostedNetwork : IDisposable
    {
        /// <summary>
        /// Desired client API version.
        /// </summary>
        const int DESIRED_CLIENT_VERSION = 2;

        /// <summary>
        /// Actual negotiated version.
        /// </summary>
        public readonly int negotiatedClientVersion;

        /// <summary>
        /// Returned client wlan api handle used for processing.
        /// </summary>
        public readonly IntPtr clientHandle;

        /// <summary>
        /// Variable which holds a reason why wlan api function failed.
        /// </summary>
        WlanApi.WLAN_HOSTED_NETWORK_REASON networkFailReason;

        /// <summary>
        /// Parsed network status easily readable by .NET
        /// </summary>
        WlanApi.WLAN_HOSTED_NETWORK_STATUS_PARSED networkStatus;

        /// <summary>
        /// Hosted network SSID.
        /// </summary>
        string networkSSID;

        /// <summary>
        /// Hosted network key.
        /// </summary>
        string networkKey;

        /// <summary>
        /// Maximum number of peer connections.
        /// </summary>
        UInt32 maxConnections;

        /// <summary>
        /// Peer authorization algorithm.
        /// </summary>
        WlanApi.DOT11_AUTH_ALGORITHM authAlgorithm;

        /// <summary>
        /// Hosted network wlan encryption algorithm.
        /// </summary>
        WlanApi.DOT11_CIPHER_ALGORITHM cipherAlgorith;

        /// <summary>
        /// Holds value wether or not Wlan API handle is open.
        /// </summary>
        bool isHandleOpen = false;

        /// <summary>
        /// Holds value wether or not hosted network is enabled.
        /// </summary>
        bool isHostedNetworkEnabled = false;

        /// <summary>
        /// Holds value wether or not hosted network is started.
        /// </summary>
        bool isNetworkStarted = false;

        /// <summary>
        /// Instance of network sharing (ICS) COM object.
        /// </summary>
        NetSharingManager sharingManager;

        /// <summary>
        /// Holds current adapter which is used for hosted network.
        /// </summary>
        SharingNetworkAdapter hostedNetworkSharingAdapter;

        /// <summary>
        /// Holds adapter from which internet connection is shared.
        /// </summary>
        SharingNetworkAdapter fromSharingAdapter = null;
      
        /// <summary>
        /// Gets wether or not network is started.
        /// </summary>
        public bool IsNetworkStarted {
            get
            {
                return this.isNetworkStarted;
            }
        }

        /// <summary>
        /// Gets wether or not current connection is shared.
        /// </summary>
        public bool IsSharingConnection
        {
            get
            {
                return fromSharingAdapter != null;
            }
        }

        /// <summary>
        /// Gets currently assigned network key.
        /// </summary>
        public string NetworkKey
        {
            get
            {
                return this.networkKey;
            }

            set
            {
                SetNetworkKey(value);
            }
        }

        /// <summary>
        /// Gets currently assigned network name.
        /// </summary>
        public string NetworkSSID
        {
            get
            {
                return networkSSID;
            }
        }

        /// <summary>
        /// Gets maximum number of peer connection allowed by hosted network.
        /// </summary>
        public int MaxConnections
        {
            get
            {
                return (int)maxConnections;
            }
        }

        /// <summary>
        /// Gets current network status.
        /// </summary>
        public WlanApi.WLAN_HOSTED_NETWORK_STATUS_PARSED NetworkStatus
        {
            get
            {
                return this.networkStatus;
            }
        }

        /// <summary>
        /// Gets or sets wether or not hosted network is enabled.
        /// </summary>
        public bool IsNetworkAllowed
        {
            get
            {
                return this.isHostedNetworkEnabled;
            }

            set
            {
                SetAllowMode(value);
            }
        }

        /// <summary>
        /// Gets GUID of a device assigned to hosted network.
        /// </summary>
        public string HostedNetworkGuid
        {
            get
            {
                this.RefreshNetworkData();

                if (this.networkStatus.HostedNetworkState == WlanApi.WLAN_HOSTED_NETWORK_STATE.Active)
                {
                    return "{" + this.networkStatus.IPDeviceID.ToString().ToUpper() + "}";
                }

                return null;
            }
        }

        /// <summary>
        /// Initializes wlan hosted network.
        /// </summary>
        public WlanHostedNetwork()
        {
            if (WlanApi.WlanOpenHandle(DESIRED_CLIENT_VERSION, IntPtr.Zero, out negotiatedClientVersion, out clientHandle) != 0)
            {
                throw new Exception("Unable to open wlan handle." + Marshal.GetLastWin32Error());
            }

            if (WlanApi.WlanHostedNetworkInitSettings(clientHandle, out networkFailReason, IntPtr.Zero) != 0)
            {
                throw new Exception("Failed to initialize settings.");
            }

            UInt32 dataSize;
            IntPtr data = IntPtr.Zero;
            WlanApi.WLAN_OPCODE_VALUE_TYPE valueType;

            if (WlanApi.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanApi.WLAN_HOSTED_NETWORK_OPCODE.SecuritySettings, 
                    out dataSize,
                    out data,
                    out valueType,
                    IntPtr.Zero) != 0)
            {
                throw new Exception("Unable to get wlan security settings.");
            }

            WlanApi.WLAN_HOSTED_NETWORK_SECURITY_SETTINGS security = 
                WlanApi.MarshalDataToStructure<WlanApi.WLAN_HOSTED_NETWORK_SECURITY_SETTINGS>(data);

            this.authAlgorithm = security.dot11AuthAlgo;
            this.cipherAlgorith = security.dot11CipherAlgo;

            WlanApi.WlanFreeMemory(data);

            data = IntPtr.Zero;
            if (WlanApi.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanApi.WLAN_HOSTED_NETWORK_OPCODE.ConnectionSettings,
                    out dataSize,
                    out data,
                    out valueType,
                    IntPtr.Zero) != 0)
            {
                throw new Exception("Unable to get wlan connection settings.");
            }

            WlanApi.WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS connection =
                WlanApi.MarshalDataToStructure<WlanApi.WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS>(data);

            this.maxConnections = connection.dwMaxNumberOfPeers;
            this.networkSSID = connection.hostedNetworkSSID.ucSSID;

            WlanApi.WlanFreeMemory(data);

            bool isPassPhrase;
            bool isPersistent;

            if (WlanApi.WlanHostedNetworkQuerySecondaryKey(
                clientHandle, 
                out dataSize,
                out this.networkKey, 
                out isPassPhrase,
                out isPersistent, 
                out networkFailReason,
                IntPtr.Zero) != 0)
            {
                throw new Exception("Unable to get wlan security key.");
            }

            data = IntPtr.Zero;
            if (WlanApi.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanApi.WLAN_HOSTED_NETWORK_OPCODE.Enable,
                    out dataSize,
                    out data,
                    out valueType,
                    IntPtr.Zero) != 0)
            {
                throw new Exception("Unable to get wlan connection settings.");
            }

            isHostedNetworkEnabled = Marshal.ReadByte(data) != 0;

            RefreshNetworkData();

            this.isNetworkStarted = this.networkStatus.HostedNetworkState == WlanApi.WLAN_HOSTED_NETWORK_STATE.Active;

            sharingManager = new NetSharingManager();

            isHandleOpen = true;
        }

        /// <summary>
        /// Refresh network data and pool for new status values.
        /// </summary>
        public void RefreshNetworkData()
        {
            IntPtr networkStatusPtr;

            if (WlanApi.WlanHostedNetworkQueryStatus(clientHandle, out networkStatusPtr, IntPtr.Zero) != 0)
            {
                throw new Exception("Unable to get wlan status.");
            }

            this.networkStatus = WlanApi.GetParsedHostedNetworkStatus(networkStatusPtr);

            WlanApi.WlanFreeMemory(networkStatusPtr);
        }

        /// <summary>
        /// Set network key.
        /// </summary>
        /// <param name="newKey">New key which will be set.</param>
        private void SetNetworkKey(string newKey)
        {
            if (WlanApi.WlanHostedNetworkSetSecondaryKey(
                clientHandle,
                (uint)newKey.Length + 1,
                newKey, 
                true, 
                true, 
                out networkFailReason,
                IntPtr.Zero) != 0) 
            {
                throw new Exception("Unable to set new security key. Reason: " + networkFailReason.ToString());
            }

            networkKey = newKey;
        }

        /// <summary>
        /// Closes Wlan API handle if open.
        /// </summary>
        public void Dispose()
        {
            if (isHandleOpen)
            {
                if (WlanApi.WlanCloseHandle(clientHandle, IntPtr.Zero) != 0)
                {
                    throw new Exception("Unable to close wlan handle.");
                }
            }
        }

        /// <summary>
        /// Generates a random string from specified length.
        /// </summary>
        /// <param name="length">Length which will be used to generate a random string.</param>
        /// <returns>Random generated string.</returns>
        private string GetRandomString(int length)
        {
            string available = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            Random rng = new Random(DateTime.Now.Second);

            StringBuilder randomString = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                randomString.Append(available[rng.Next(0, available.Length)]);
            }

            return randomString.ToString();
        }

        /// <summary>
        /// Sets SSID and maximum number of peer connections.
        /// </summary>
        /// <param name="ssid">New network SSID.</param>
        /// <param name="maxConnections">New maximum number of peer connections.</param>
        public void SetSSIDAndMaxConnections(string ssid, uint maxConnections)
        {
            WlanApi.WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS connectionSettings = new WlanApi.WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS();

            string resetSSIDString = GetRandomString(16);

            connectionSettings.dwMaxNumberOfPeers = (uint)maxConnections;
            connectionSettings.hostedNetworkSSID = new WlanApi.DOT11_SSID()
            {
                ucSSID = resetSSIDString,
                uSSIDLength = resetSSIDString.Length
            };

            IntPtr settingsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(connectionSettings));
            Marshal.StructureToPtr(connectionSettings, settingsPtr, false);

            if (WlanApi.WlanHostedNetworkSetProperty(
                    this.clientHandle,
                    WlanApi.WLAN_HOSTED_NETWORK_OPCODE.ConnectionSettings,
                    (uint)Marshal.SizeOf(connectionSettings), settingsPtr, out networkFailReason, IntPtr.Zero
                ) != 0)
            {
                throw new Exception("Unable to set connection settings.");
            }


            connectionSettings.dwMaxNumberOfPeers = (uint)maxConnections;
            connectionSettings.hostedNetworkSSID = new WlanApi.DOT11_SSID()
            {
                ucSSID = ssid,
                uSSIDLength = ssid.Length
            };

            settingsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(connectionSettings));
            Marshal.StructureToPtr(connectionSettings, settingsPtr, false);

            if (WlanApi.WlanHostedNetworkSetProperty(
                    this.clientHandle,
                    WlanApi.WLAN_HOSTED_NETWORK_OPCODE.ConnectionSettings,
                    (uint)Marshal.SizeOf(connectionSettings), settingsPtr, out networkFailReason, IntPtr.Zero
                ) != 0)
            {
                throw new Exception("Unable to set connection settings.");
            }


            this.networkSSID = ssid;
            this.maxConnections = (uint)maxConnections;
        }

        /// <summary>
        /// Start Hosted Network.
        /// </summary>
        public void Start()
        {
            if (WlanApi.WlanHostedNetworkStartUsing(clientHandle, out networkFailReason, IntPtr.Zero) != 0)
            {
                throw new Exception("Failed to start hosted network. Reason: " + networkFailReason.ToString());
            }

            this.isNetworkStarted = true;
        }

        /// <summary>
        /// Stop Hosted Network.
        /// </summary>
        public void Stop()
        {
            if (WlanApi.WlanHostedNetworkForceStop(clientHandle, out networkFailReason, IntPtr.Zero) != 0)
            {
                throw new Exception("Failed to start hosted network. Reason: " + networkFailReason.ToString());
            }

            this.isNetworkStarted = false;
        }

        /// <summary>
        /// Force hosted network into a start state.
        /// </summary>
        public void ForceStart()
        {
            if (WlanApi.WlanHostedNetworkForceStart(clientHandle, out networkFailReason, IntPtr.Zero) != 0)
            {
                throw new Exception("Failed to force start hosted network. Reason: " + networkFailReason.ToString());
            }

            this.isNetworkStarted = true;
        }

        /// <summary>
        /// Force hosted network into a stop state.
        /// </summary>
        public void ForceStop()
        {
            if (WlanApi.WlanHostedNetworkForceStop(clientHandle, out networkFailReason, IntPtr.Zero) != 0)
            {
                throw new Exception("Failed to force start hosted network. Reason: " + networkFailReason.ToString());
            }

            this.isNetworkStarted = false;
        }

        /// <summary>
        /// Sets wether or not hosted network is enabled.
        /// </summary>
        /// <param name="isAllowed">New value which will be set.</param>
        private void SetAllowMode(bool isAllowed)
        {
            if (WlanApi.WlanHostedNetworkSetProperty(
                clientHandle, 
                WlanApi.WLAN_HOSTED_NETWORK_OPCODE.Enable, 
                (uint)Marshal.SizeOf(isAllowed), 
                WlanApi.MarshalDataToIntPtr(isAllowed),
                out networkFailReason,
                IntPtr.Zero

                ) != 0)
            {
                throw new Exception("Failed to set enable property. Reason: " + networkFailReason.ToString());
            }

            this.isHostedNetworkEnabled = isAllowed;
        }

        /// <summary>
        /// Returns list of devices available to be used by ICS.
        /// </summary>
        /// <returns>List containing network devices.</returns>
        public List<SharingNetworkAdapter> GetSharingDevices()
        {
            List<SharingNetworkAdapter> sharingList = new List<SharingNetworkAdapter>();

            string hostedNetworkGuid = this.HostedNetworkGuid;

            foreach(INetConnection connection in sharingManager.EnumEveryConnection)
            {
                INetConnectionProps connectionProperties = sharingManager.NetConnectionProps[connection];

                if (connectionProperties.Guid.Equals(hostedNetworkGuid))
                {
                    this.hostedNetworkSharingAdapter = new SharingNetworkAdapter(sharingManager, connection);
                }
                else
                {
                    sharingList.Add(new SharingNetworkAdapter(sharingManager, connection));
                }
            }

            return sharingList;
        }

        /// <summary>
        /// Shares connection from specified adapter to hosted network.
        /// </summary>
        /// <param name="fromAdapter">Adapter from which connection will be shared.</param>
        public void ShareConnectionFrom(SharingNetworkAdapter fromAdapter)
        {
            if (fromAdapter.Equals(hostedNetworkSharingAdapter))
            {
                return;
            }

            foreach (INetConnection connection in sharingManager.EnumEveryConnection)
            {
                INetSharingConfiguration sharingConfig = sharingManager.INetSharingConfigurationForINetConnection[connection];

                if (sharingConfig.SharingEnabled)
                {
                    sharingConfig.DisableSharing();
                }
            }

            fromSharingAdapter = fromAdapter;

            fromAdapter.EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC);
            hostedNetworkSharingAdapter.EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE);
        }

        /// <summary>
        /// Disables sharing internet connection from hosted network.
        /// </summary>
        public void DisableSharing()
        {
            if (fromSharingAdapter == null)
            {
                return;
            }

            fromSharingAdapter.DisableSharing();
            hostedNetworkSharingAdapter.DisableSharing();

            fromSharingAdapter = null;
        }
    }
}
