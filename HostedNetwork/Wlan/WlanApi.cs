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
using System.Runtime.InteropServices;

namespace HostedNetwork.Wlan
{
    /// <summary>
    /// Contains internal functions which use Windows API to control Wlan.
    /// </summary>
    internal static class WlanApi
    {
        /// <summary>
        /// Enumeration of available WLAN authorization algorithms.
        /// </summary>
        internal enum DOT11_AUTH_ALGORITHM
        {
            ALGO_80211_OPEN = 1,
            ALGO_80211_SHARED_KEY = 2,
            WPA = 3,
            WPA_PSK = 4,
            WPA_NONE = 5,
            RSNA = 6,
            RSNA_PSK = 7
        }

        /// <summary>
        /// Enumeration of available WLAN encryption algoritms.
        /// </summary>
        internal enum DOT11_CIPHER_ALGORITHM
        {
            NONE = 0x00,
            WEP40 = 0x01,
            TKIP = 0x02,
            CCMP = 0x04,
            WEP104 = 0x05,
            WPA_USE_GROUP = 0x100,
            RSN_USE_GROUP = 0x100,
            WEP = 0x101
        }

        /// <summary>
        /// Structure which represents device's MAC address.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct DOT11_MAC_ADDRESS
        {
            public byte byte1;
            public byte byte2;
            public byte byte3;
            public byte byte4;
            public byte byte5;
            public byte byte6;
        }

        /// <summary>
        /// The DOT11_PHY_TYPE enumeration defines an 802.11 PHY and media type.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/ms706034%28v=vs.85%29.aspx
        /// </summary>
        internal enum DOT11_PHY_TYPE
        {
            Unknown = 0,
            Any = 0,
            FHSS = 1,
            DSSS = 2,
            IRBaseBand = 3,
            OFDM = 4,
            HRDSSS = 5,
            ERP = 6,
            HT = 7,
            VHT = 8,
            IHVStart = 0x80000000,
            IHVEnd = 0xffffffff
        }

        /// <summary>
        /// A DOT11_SSID structure contains the SSID of an interface.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/ms706034%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct DOT11_SSID
        {
            public int uSSIDLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string ucSSID;
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS structure contains information about the connection 
        /// settings on the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439499%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS
        {
            public DOT11_SSID hostedNetworkSSID;
            public UInt32 dwMaxNumberOfPeers;
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_OPCODE enumerated type specifies the possible values of the 
        /// operation code for the properties to query or set on the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439502%28v=vs.85%29.aspx
        /// </summary>
        internal enum WLAN_HOSTED_NETWORK_OPCODE
        {
            ConnectionSettings,
            SecuritySettings,
            StationProfile,
            Enable
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_PEER_AUTH_STATE enumerated type specifies the possible 
        /// values for the authentication state of a peer on the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439503%28v=vs.85%29.aspx
        /// </summary>
        internal enum WLAN_HOSTED_NETWORK_PEER_AUTH_STATE
        {
            Invalid,
            Authenticated
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_PEER_STATE structure contains information about the 
        /// peer state for a peer on the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439504%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct WLAN_HOSTED_NETWORK_PEER_STATE
        {
            public DOT11_MAC_ADDRESS PeerMacAddress;
            public WLAN_HOSTED_NETWORK_PEER_AUTH_STATE PeerAuthState;
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_REASON enumerated type specifies the possible values for the 
        /// result of a wireless Hosted Network function call.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439506%28v=vs.85%29.aspx
        /// </summary>
        internal enum WLAN_HOSTED_NETWORK_REASON
        {
            Success = 0,
            UnspecifiedReason,
            BadParameters,
            ServiceShuttingDown,
            InsufficientResources,
            ElevationRequired,
            ReadOnly,
            PersistenceFailed,
            CryptError,
            Impersonation,
            StoppedBeforeStart,
            InterfaceAvailable,
            InterfaceUnavailable,
            MiniportStopped,
            MiniportStarted,
            IncompatibleConnectionStarted,
            IncompatibleConnectionStopped,
            UserAction,
            ClientAborted,
            APStartFailed,
            PeerArrived,
            PeerDeparted,
            PeerTimeout,
            GPDenied,
            ServiceUnavailable,
            DeviceChange,
            PropertiesChange,
            VirtualStationBlockingUse,
            ServiceAvailableOnVirtualStation
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_SECURITY_SETTINGS structure contains information 
        /// about the security settings on the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439507%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct WLAN_HOSTED_NETWORK_SECURITY_SETTINGS
        {
            public DOT11_AUTH_ALGORITHM dot11AuthAlgo;
            public DOT11_CIPHER_ALGORITHM dot11CipherAlgo;
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_STATE enumerated type specifies the possible 
        /// values for the network state of the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439508%28v=vs.85%29.aspx
        /// </summary>
        internal enum WLAN_HOSTED_NETWORK_STATE
        {
            Unavailable,
            Idle,
            Active
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_STATUS structure contains information about the status of the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439510%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WLAN_HOSTED_NETWORK_STATUS
        {
            public WLAN_HOSTED_NETWORK_STATE HostedNetworkState;
            public Guid IPDeviceID;
            public DOT11_MAC_ADDRESS wlanHostedNetworkBSSID;
            public DOT11_PHY_TYPE dot11PhyType;
            public UInt32 ulChannelFrequency;
            public UInt32 dwNumberOfPeers;
            public IntPtr PeerList;
        }

        /// <summary>
        /// Parsed version of WLAN_HOSTED_NETWORK_STATUS, more friendly to .NET.
        /// </summary>
        internal struct WLAN_HOSTED_NETWORK_STATUS_PARSED
        {
            public WLAN_HOSTED_NETWORK_STATE HostedNetworkState;
            public Guid IPDeviceID;
            public DOT11_MAC_ADDRESS wlanHostedNetworkBSSID;
            public DOT11_PHY_TYPE dot11PhyType;
            public UInt32 ulChannelFrequency;
            public UInt32 dwNumberOfPeers;
            public WLAN_HOSTED_NETWORK_PEER_STATE[] PeerList;
        }

        /// <summary>
        /// The WLAN_OPCODE_VALUE_TYPE enumeration specifies the origin of automatic configuration (auto config) settings. 
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/ms706910%28v=vs.85%29.aspx
        /// </summary>
        internal enum WLAN_OPCODE_VALUE_TYPE
        {
            QueryOnly = 0,
            SetByGroupPolicy = 1,
            SetByUser = 2,
            Invalid = 3
        }

        /// <summary>
        /// Delegate function which is called when there is a WLAN notification.
        /// </summary>
        /// <param name="notificationData">Pointer to WLAN notification data sent by notification.</param>
        /// <param name="context">Pointer to contextual data sent by user.</param>
        internal delegate void WlanNotificationDelegate(IntPtr notificationData, IntPtr context);

        /// <summary>
        /// Specifies where the notification comes from.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/ms706902%28v=vs.85%29.aspx
        /// </summary>
        [Flags]
        internal enum WLAN_NOTIFICATION_SOURCE : uint
        {
            None = 0,
            All = 0X0000FFFF,
            ACM = 0X00000008,
            MSM = 0X00000010,
            Security = 0X00000020,
            IHV = 0X00000040,
            HNWK = 0X00000080
        }

        /// <summary>
        /// The WLAN_NOTIFICATION_CODE_MSM enumerated type specifies the possible values of 
        /// the NotificationCode member of the WLAN_NOTIFICATION_DATA structure for Media Specific Module (MSM) notifications.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd815255%28v=vs.85%29.aspx
        /// </summary>
        internal enum WLAN_NOTIFICATION_CODE_MSM
        {
            Associating = 1,
            Associated,
            Authenticating,
            Connected,
            RoamingStart,
            RoamingEnd,
            RadioStateChange,
            SignalQualityChange,
            Disassociating,
            Disconnected,
            PeerJoin,
            PeerLeave,
            AdapterRemoval,
            AdapterOperationModeChange
        }

        /// <summary>
        /// The WLAN_NOTIFICATION_CODE_ACM enumerated type specifies the possible values of the 
        /// NotificationCode member of the WLAN_NOTIFICATION_DATA structure for Auto Configuration Module (ACM) notifications.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd815254%28v=vs.85%29.aspx
        /// </summary>
        internal enum WLAN_NOTIFICATION_CODE_ACM
        {
            AutoconfEnabled = 0,
            AutoconfDisabled,
            BackgroundScanEnabled,
            BackgroundScanDisabled,
            BssTypeChange,
            PowerSettingChange,
            ScanComplete,
            ScanFail,
            ConnectionStart,
            ConnectionComplete,
            ConnectionAttemptFail,
            FilterListChange,
            InterfaceArrival,
            InterfaceRemoval,
            ProfileChange,
            ProfileNameChange,
            ProfilesExhausted,
            NetworkNotAvailable,
            NetworkAvailable,
            Disconnecting,
            Disconnected,
            AdhocNetworkStateChange
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_NOTIFICATION_CODE enumerated type specifies the possible 
        /// values of the NotificationCode parameter for received notifications on the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439501%28v=vs.85%29.aspx
        /// </summary>
        internal enum WLAN_HOSTED_NETWORK_NOTIFICATION_CODE
        {
            StateChange = 0x00001000,
            PeerStateChange,
            RadioStateChange
        }

        /// <summary>
        /// The WLAN_NOTIFICATION_DATA structure contains information provided when receiving notifications.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/ms706902%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct WLAN_NOTIFICATION_DATA
        {
            public WLAN_NOTIFICATION_SOURCE notificationSource;
            public int notificationCode;
            public Guid interfaceGuid;
            public int dataSize;
            public IntPtr dataPtr;
        }

        /// <summary>
        /// The WLAN_PHY_RADIO_STATE structure specifies the radio state on a specific physical layer (PHY) type.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/ms706918%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct WLAN_PHY_RADIO_STATE
        {
            public uint dwPhyIndex;
            public DOT11_RADIO_STATE dot11SoftwareRadioState;
            public DOT11_RADIO_STATE dot11HardwareRadioState;
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_STATE_CHANGE structure contains information about a 
        /// network state change on the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439509%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WLAN_HOSTED_NETWORK_STATE_CHANGE
        {
            public WLAN_HOSTED_NETWORK_STATE OldState;
            public WLAN_HOSTED_NETWORK_STATE NewState;
            public WLAN_HOSTED_NETWORK_REASON Reason;
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_DATA_PEER_STATE_CHANGE structure contains information about a network 
        /// state change for a data peer on the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439500%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WLAN_HOSTED_NETWORK_DATA_PEER_STATE_CHANGE
        {
            public WLAN_HOSTED_NETWORK_PEER_STATE OldState;
            public WLAN_HOSTED_NETWORK_PEER_STATE NewState;
            public WLAN_HOSTED_NETWORK_REASON Reason;
        }

        /// <summary>
        /// The WLAN_HOSTED_NETWORK_RADIO_STATE structure contains 
        /// information about the radio state on the wireless Hosted Network.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/dd439505%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WLAN_HOSTED_NETWORK_RADIO_STATE
        {
            public DOT11_RADIO_STATE dot11SoftwareRadioState;
            public DOT11_RADIO_STATE dot11HardwareRadioState;
        }

        /// <summary>
        /// The DOT11_RADIO_STATE enumeration specifies an 802.11 radio state.
        /// 
        /// More info: https://msdn.microsoft.com/en-us/library/windows/desktop/ms706027%28v=vs.85%29.aspx
        /// </summary>
        internal enum DOT11_RADIO_STATE
        {
            UnknownState,
            OnState,
            OffState
        }

        /// <summary>
        /// The WlanHostedNetworkStartUsing function starts the wireless Hosted Network.
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439497%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pFailReason">An optional pointer to a value that receives the failure reason, if the call to the W
        /// lanHostedNetworkStartUsing function fails.</param>
        /// <param name="pReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanHostedNetworkStartUsing", SetLastError = true)]
        internal static extern int WlanHostedNetworkStartUsing(
            [In] IntPtr hClientHandle,
            [Out, Optional] out WLAN_HOSTED_NETWORK_REASON pFailReason,
            [In, Out] IntPtr pReserved
            );

        /// <summary>
        /// The WlanHostedNetworkStopUsing function stops the wireless Hosted Network.
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439498%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pFailReason">An optional pointer to a value that receives the failure reason if the call to the 
        /// WlanHostedNetworkStopUsing function fails.</param>
        /// <param name="pReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanHostedNetworkStopUsing", SetLastError = true)]
        internal static extern int WlanHostedNetworkStopUsing(
            [In] IntPtr hClientHandle,
            [Out, Optional] out WLAN_HOSTED_NETWORK_REASON pFailReason,
            [In, Out] IntPtr pReserved
            );

        /// <summary>
        /// The WlanHostedNetworkForceStart function transitions the wireless Hosted Network to the wlan_hosted_network_active 
        /// state without associating the request with the application's calling handle. 
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439488%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pFailReason">An optional pointer to a value that receives the failure reason if the call to the 
        /// WlanHostedNetworkForceStart function fails.</param>
        /// <param name="pReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanHostedNetworkForceStart", SetLastError = true)]
        internal static extern int WlanHostedNetworkForceStart(
            [In] IntPtr hClientHandle,
            [Out, Optional] out WLAN_HOSTED_NETWORK_REASON pFailReason,
            [In, Out] IntPtr pReserved
            );

        /// <summary>
        /// The WlanHostedNetworkForceStop function transitions the wireless Hosted Network to the wlan_hosted_network_idle without 
        /// associating the request with the application's calling handle. 
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439489%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pFailReason">An optional pointer to a value that receives the failure reason, if the call to the 
        /// WlanHostedNetworkForceStop function fails.</param>
        /// <param name="pReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanHostedNetworkForceStop", SetLastError = true)]
        internal static extern int WlanHostedNetworkForceStop(
            [In] IntPtr hClientHandle,
            [Out, Optional] out WLAN_HOSTED_NETWORK_REASON pFailReason,
            [In, Out] IntPtr pReserved
            );


        /// <summary>
        /// The WlanHostedNetworkInitSettings function configures and persists to storage the network connection settings 
        /// (SSID and maximum number of peers, for example) on the wireless Hosted Network if these settings are not already configured. 
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439490%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pFailReason">An optional pointer to a value that receives the failure reason if the call to the 
        /// WlanHostedNetworkInitSettings function fails.</param>
        /// <param name="pReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanHostedNetworkInitSettings", SetLastError = true)]
        internal static extern int WlanHostedNetworkInitSettings(
            [In] IntPtr hClientHandle,
            [Out, Optional] out WLAN_HOSTED_NETWORK_REASON pFailReason,
            [In, Out] IntPtr pReserved
            );

        /// <summary>
        /// The WlanHostedNetworkQuerySecondaryKey function queries the secondary security key that is configured to be 
        /// used by the wireless Hosted Network. 
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439492%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pKeyLength">
        /// A pointer to a value that specifies number of valid data bytes in the key data array pointed to by the 
        /// ppucKeyData parameter, if the call to the WlanHostedNetworkQuerySecondaryKey function succeeds.
        /// This key length includes the terminating ‘\0’ if the key is a passphrase.
        /// </param>
        /// <param name="ppucKeyData">
        /// A pointer to a value that receives a pointer to the buffer returned with the secondary security key data, 
        /// if the call to the WlanHostedNetworkQuerySecondaryKey function succeeds. 
        /// </param>
        /// <param name="pbIsPassPhrase">A pointer to a Boolean value that indicates if the key data array pointed to by the ppucKeyData 
        /// parameter is in passphrase format. </param>
        /// <param name="pbPersistent">A pointer to a Boolean value that indicates if the key data array pointed to by the 
        /// ppucKeyData parameter is to be stored and reused later or is for one-time use only. </param>
        /// <param name="pFailReason">An optional pointer to a value that receives the failure reason, 
        /// if the call to the WlanHostedNetworkSetSecondaryKey function fails.</param>
        /// <param name="pReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanHostedNetworkQuerySecondaryKey", SetLastError = true)]
        internal static extern int WlanHostedNetworkQuerySecondaryKey(
            [In]  IntPtr hClientHandle,
            [Out] out uint pKeyLength,
            [Out, MarshalAs(UnmanagedType.LPStr)] out string ppucKeyData,
            [Out] out bool pbIsPassPhrase,
            [Out] out bool pbPersistent,
            [Out, Optional] out WLAN_HOSTED_NETWORK_REASON pFailReason,
            [In, Out] IntPtr pReserved
            );


        /// <summary>
        /// The WlanHostedNetworkSetSecondaryKey function configures the secondary security key 
        /// that will be used by the wireless Hosted Network. 
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439496%28v=vs.85%29.aspx for more information.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="dwKeyLength">The number of valid data bytes in the key data array pointed to by the pucKeyData parameter.</param>
        /// <param name="pucKeyData">A pointer to a buffer that contains the key data. </param>
        /// <param name="bIsPassPhrase">A Boolean value that indicates if the key data array pointed to by the pucKeyData parameter 
        /// is in passphrase format. </param>
        /// <param name="bPersistent">A Boolean value that indicates if the key data array pointed to by the pucKeyData parameter 
        /// is to be stored and reused later or is for one-time use only. </param>
        /// <param name="pFailReason">An optional pointer to a value that receives the failure reason, if the call to the 
        /// WlanHostedNetworkSetSecondaryKey function fails.</param>
        /// <param name="pvReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi", EntryPoint = "WlanHostedNetworkSetSecondaryKey", SetLastError = true)]
        internal static extern uint WlanHostedNetworkSetSecondaryKey(
            IntPtr hClientHandle, 
            uint dwKeyLength,
            [In, MarshalAs(UnmanagedType.LPStr)] string pucKeyData,
            bool bIsPassPhrase, 
            bool bPersistent, 
            [Out] out WLAN_HOSTED_NETWORK_REASON pFailReason, 
            IntPtr pReserved
            );

        /// <summary>
        /// The WlanHostedNetworkRefreshSecuritySettings function refreshes the configurable and auto-generated 
        /// parts of the wireless Hosted Network security settings. 
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439494%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pFailReason">An optional pointer to a value that receives the failure reason, 
        /// if the call to the WlanHostedNetworkRefreshSecuritySettings function fails.</param>
        /// <param name="pReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanHostedNetworkRefreshSecuritySettings", SetLastError = true)]
        internal static extern int WlanHostedNetworkRefreshSecuritySettings(
            [In] IntPtr hClientHandle,
            [Out, Optional] out WLAN_HOSTED_NETWORK_REASON pFailReason,
            [In, Out] IntPtr pReserved
            );

        /// <summary>
        /// The WlanHostedNetworkSetProperty function sets static properties of the wireless Hosted Network. 
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439495%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="OpCode">The identifier for the property to be set.</param>
        /// <param name="dwDataSize">A value that specifies the size, in bytes, of the buffer pointed to by the pvData parameter.</param>
        /// <param name="pvData">A pointer to a buffer with the static property to set.</param>
        /// <param name="pFailReason">An optional pointer to a value that receives the failure reason, if the call to the WlanHostedNetworkSetProperty function fails.</param>
        /// <param name="pReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanHostedNetworkSetProperty", SetLastError = true)]
        internal static extern int WlanHostedNetworkSetProperty(
            [In] IntPtr hClientHandle,
            [In] WLAN_HOSTED_NETWORK_OPCODE OpCode,
            [In] uint dwDataSize,
            [In] IntPtr pvData,
            [Out, Optional] out WLAN_HOSTED_NETWORK_REASON pFailReason,
            [In, Out] IntPtr pReserved
            );


        /// <summary>
        /// The WlanHostedNetworkQueryProperty function queries the current static properties of the wireless Hosted Network.
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439491%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="OpCode">The identifier for property to be queried.</param>
        /// <param name="pdwDataSize">A pointer to a value that specifies the size, in bytes, of the buffer returned in the ppvData parameter, 
        /// if the call to the WlanHostedNetworkQueryProperty function succeeds.</param>
        /// <param name="ppvData">
        /// On input, this parameter must be NULL.
        /// On output, this parameter receives a pointer to a buffer returned with the static property requested, 
        /// if the call to the WlanHostedNetworkQueryProperty function succeeds.
        /// </param>
        /// <param name="pWlanOpcodeValueType">A pointer to a value that receives the value 
        /// type of the wireless Hosted Network property, if the call to the WlanHostedNetworkQueryProperty function succeeds.</param>
        /// <param name="pReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanHostedNetworkQueryProperty", SetLastError = true)]
        internal static extern int WlanHostedNetworkQueryProperty(
            [In] IntPtr hClientHandle,
            [In] WLAN_HOSTED_NETWORK_OPCODE OpCode,
            [Out] out uint pdwDataSize,
            [Out] out IntPtr ppvData,
            [Out, Optional] out WLAN_OPCODE_VALUE_TYPE pWlanOpcodeValueType,
            [In, Out] IntPtr pReserved
            );

        /// <summary>
        /// The WlanHostedNetworkQueryStatus function queries the current status of the wireless Hosted Network. 
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd439493%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, returned by a previous call to the WlanOpenHandle function.</param>
        /// <param name="ppWlanHostedNetworkStatus">
        /// On input, this parameter must be NULL.
        /// 
        /// On output, this parameter receives a pointer to the current status of the wireless Hosted Network, 
        /// if the call to the WlanHostedNetworkQueryStatus function succeeds.
        /// </param>
        /// <param name="pvReserved">Reserved for future use. This parameter must be NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanHostedNetworkQueryStatus", SetLastError = true)]
        internal static extern int WlanHostedNetworkQueryStatus(
            [In] IntPtr hClientHandle,
            [Out] out IntPtr ppWlanHostedNetworkStatus,
            [In, Out] IntPtr pvReserved
            );


        /// <summary>
        /// The WlanOpenHandle function opens a connection to the server.
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/ms706759%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="dwClientVersion">
        /// The highest version of the WLAN API that the client supports. 
        /// 1 - Client version for Windows XP with SP3 and Wireless LAN API for Windows XP with SP2.
        /// 2 - Client version for Windows Vista and Windows Server 2008.
        /// </param>
        /// <param name="pReserved">
        /// Reserved for future use. Must be set to NULL.
        /// </param>
        /// <param name="pdwNegotiatedVersion">
        /// The version of the WLAN API that will be used in this session. 
        /// This value is usually the highest version supported by both the client and server.
        /// </param>
        /// <param name="phClientHandle">
        /// A handle for the client to use in this session. This handle is used by other functions throughout the session.
        /// </param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanOpenHandle" , SetLastError = true)]
        internal static extern int WlanOpenHandle(
          [In] int dwClientVersion,
          [In, Out] IntPtr pReserved,
          [Out] out int pdwNegotiatedVersion,
          [Out] out IntPtr phClientHandle
          );

        /// <summary>
        /// The WlanCloseHandle function closes a connection to the server.
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/ms706610%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, which identifies the connection to be closed. 
        /// This handle was obtained by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pReserved">Reserved for future use. Set this parameter to NULL.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanCloseHandle", SetLastError = true)]
        internal static extern int WlanCloseHandle(
            [In] IntPtr hClientHandle,
            [In, Out] IntPtr pReserved
            );

        /// <summary>
        /// The WlanFreeMemory function frees memory. Any memory returned from Native Wifi functions must be freed.
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/ms706722%28v=vs.85%29.aspx for more details.
        /// </summary>
        /// <param name="pMemory">Pointer to the memory to be freed.</param>
        [DllImport("Wlanapi", EntryPoint = "WlanFreeMemory", SetLastError = true)]
        internal static extern void WlanFreeMemory(
            [In] IntPtr pMemory
            );


        /// <summary>
        /// The WlanRegisterNotification function is used to register and unregister notifications on all wireless interfaces. 
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/ms706771%28v=vs.85%29.aspx for more info.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, obtained by a previous call to the WlanOpenHandle function.</param>
        /// <param name="dwNotifSource">The notification sources to be registered. These flags may be combined.</param>
        /// <param name="bIgnoreDuplicate">Specifies wether duplicate notifications will be ignored.</param>
        /// <param name="funcCallback">A WlanNotificationDelegate type that defines the type of notification callback function.</param>
        /// <param name="pCallbackContext">A pointer to the client context that will be passed to the callback function with the notification.</param>
        /// <param name="pReserved">Reserved for future use. Must be set to NULL.</param>
        /// <param name="pdwPrevNotifSource">A pointer to the previously registered notification sources.</param>
        /// <returns>Zero on success, or value other than zero on error.</returns>
        [DllImport("Wlanapi.dll", EntryPoint = "WlanRegisterNotification")]
        internal static extern uint WlanRegisterNotification(
            [In] IntPtr hClientHandle,
            [In] WLAN_NOTIFICATION_SOURCE dwNotifSource,
            [In] bool bIgnoreDuplicate,
            [In] WlanNotificationDelegate funcCallback,
            [In] IntPtr pCallbackContext,
            [In, Out] IntPtr pReserved,
            [Out] out WLAN_NOTIFICATION_SOURCE pdwPrevNotifSource
            );

        /// <summary>
        /// Performs data marshalling to IntPtr.
        /// </summary>
        /// <param name="data">Data to be marshalled.</param>
        /// <returns>IntPtr representing marshalled data.</returns>
        internal static IntPtr MarshalDataToIntPtr(object data)
        {
            IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf(data));
            Marshal.StructureToPtr(data, buffer, false);
            return buffer;
        }

        /// <summary>
        /// Performs data marshalling from IntPtr to structure.
        /// </summary>
        /// <typeparam name="T">Type of structure to be marshalled.</typeparam>
        /// <param name="dataPointer">IntPtr representing data to be marshalled.</param>
        /// <returns>Structure representing marshalled data.</returns>
        internal static T MarshalDataToStructure<T>(IntPtr dataPointer)
        {
            return (T)Marshal.PtrToStructure(dataPointer, typeof(T));
        }

        /// <summary>
        /// Transforms WLAN_HOSTED_NETWORK_STATUS and gets all peers into a list and returns
        /// WLAN_HOSTED_NETWORK_STATUS_PARSED structure with all data filled.
        /// </summary>
        /// <param name="statusPointer">Pointer to status given by WlanHostedNetworkQueryStatus.</param>
        /// <returns>WLAN_HOSTED_NETWORK_STATUS_PARSED with all data filled.</returns>
        internal static WLAN_HOSTED_NETWORK_STATUS_PARSED GetParsedHostedNetworkStatus(IntPtr statusPointer)
        {
            WLAN_HOSTED_NETWORK_STATUS status = MarshalDataToStructure<WLAN_HOSTED_NETWORK_STATUS>(statusPointer);
            WLAN_HOSTED_NETWORK_STATUS_PARSED parsedStructure = new WLAN_HOSTED_NETWORK_STATUS_PARSED();

            parsedStructure.HostedNetworkState = status.HostedNetworkState;
            parsedStructure.IPDeviceID = status.IPDeviceID;
            parsedStructure.wlanHostedNetworkBSSID = status.wlanHostedNetworkBSSID;
            parsedStructure.dot11PhyType = status.dot11PhyType;
            parsedStructure.ulChannelFrequency = status.ulChannelFrequency;
            parsedStructure.dwNumberOfPeers = status.dwNumberOfPeers;

            parsedStructure.PeerList = new WLAN_HOSTED_NETWORK_PEER_STATE[status.dwNumberOfPeers];

            IntPtr offset = Marshal.OffsetOf(typeof(WLAN_HOSTED_NETWORK_STATUS), "PeerList");

            for (int i = 0; i < status.dwNumberOfPeers; i++)
            {
                parsedStructure.PeerList[i] = (WLAN_HOSTED_NETWORK_PEER_STATE)Marshal.PtrToStructure(
                new IntPtr(statusPointer.ToInt64() + offset.ToInt64()),
                typeof(WLAN_HOSTED_NETWORK_PEER_STATE));

                offset += Marshal.SizeOf(parsedStructure.PeerList[i]);
            }

            return parsedStructure;
        }

        /// <summary>
        /// Transforms a DOT11_MAC_ADDRESS structure into a readable string.
        /// </summary>
        /// <param name="mac">DOT11_MAC_ADDRESS structure to be transformed.</param>
        /// <returns>String representation of DOT11_MAC_ADDRESS structure</returns>
        internal static string GetMACAddressAsString(DOT11_MAC_ADDRESS mac)
        {
            StringBuilder macBuilder = new StringBuilder();

            macBuilder.Append(mac.byte1.ToString("X2"));
            macBuilder.Append(":");
            macBuilder.Append(mac.byte2.ToString("X2"));
            macBuilder.Append(":");
            macBuilder.Append(mac.byte3.ToString("X2"));
            macBuilder.Append(":");
            macBuilder.Append(mac.byte4.ToString("X2"));
            macBuilder.Append(":");
            macBuilder.Append(mac.byte5.ToString("X2"));
            macBuilder.Append(":");
            macBuilder.Append(mac.byte6.ToString("X2"));

            return macBuilder.ToString();
        }
    }
}
