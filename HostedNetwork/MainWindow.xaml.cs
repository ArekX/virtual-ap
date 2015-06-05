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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Principal;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HostedNetwork.Wlan;
using NETCONLib;
using System.Reflection;

namespace HostedNetwork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Contains instance of WlanHostedNetwork class.
        /// 
        /// <see cref="HostedNetwork.WlanHostedNetwork"/>
        /// </summary>
        WlanHostedNetwork hostedNetwork;

        /// <summary>
        /// Contains instance of WlanHostedNetworkNotification class.
        /// 
        /// <see cref="HostedNetwork.WlanHostedNetworkNotification"/>
        /// </summary>
        WlanHostedNetworkNotification networkNotification;
        MacDiscovery macDiscovery;

        /// <summary>
        /// Delegate which is called when network status is updated.
        /// </summary>
        /// <param name="newState">New network state.</param>
        private delegate void UpdateNetworkStatusDelegate(WlanApi.WLAN_HOSTED_NETWORK_STATE newState);

        /// <summary>
        /// Delegate which is called when network state needs to be set.
        /// </summary>
        /// <param name="isEnabled">Boolen value which shows whether network state should be enabled or disabled.</param>
        private delegate void SetNetworkStateDelegate(bool isEnabled);

        /// <summary>
        /// Delegate which is called when network peer is updated.
        /// </summary>
        private delegate void UpdateNetworkPeersDelegate();
        
        /// <summary>
        /// Flag which is set when same update network delegate should not be called again.
        /// </summary>
        bool skipCheckUpdate = false;

        /// <summary>
        /// Initializes new main window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            string versionString = String.Format("{0}.{1}.{2} Build {3}",
                appVersion.Major,
                appVersion.Minor,
                appVersion.Revision,
                appVersion.Build);


            this.Title += " [" + versionString + "]";
            lblVersion.Text = "Version: " + versionString;

            hostedNetwork = new WlanHostedNetwork();
            networkNotification = new WlanHostedNetworkNotification(hostedNetwork.clientHandle);

            networkNotification.StateChanged += networkNotification_StateChanged;
            networkNotification.PeerStateChanged += networkNotification_PeerStateChanged;
            networkNotification.RadioStateChanged += networkNotification_RadioStateChanged;

            hostedNetwork.IsNetworkAllowed = true;

            hostedNetwork.RefreshNetworkData();

            chkAPAllowed.IsChecked = hostedNetwork.NetworkStatus.HostedNetworkState != WlanApi.WLAN_HOSTED_NETWORK_STATE.Unavailable;
            txtSSID.Text = hostedNetwork.NetworkSSID;
            txtKey.Text = hostedNetwork.NetworkKey;

            macDiscovery = new MacDiscovery();
            macDiscovery.DownloadCompleted += macDiscovery_DownloadCompleted;
            macDiscovery.DownloadProgressChanged += macDiscovery_DownloadProgressChanged;

            UpdateNetworkState();
            UpdateNetworkPeers();

            btnRefreshNetworkDevices_Click(null, null);
        }

        /// <summary>
        /// Event which is fired when mac discovery progress changes.
        /// </summary>
        /// <param name="sender">Sender from which event originated.</param>
        /// <param name="e">Event arguments.</param>
        void macDiscovery_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                return;
            }

            pbProgressArc.EndAngle = e.ProgressPercentage / 100.0F * 360;
            tbMacDownload.Text = "Downloading: " + e.ProgressPercentage.ToString() + "%";
        }

        /// <summary>
        /// Event which is fured when mac discovery list download completes.
        /// </summary>
        /// <param name="sender">Sender from which event originated.</param>
        /// <param name="e">Event arguments.</param>
        void macDiscovery_DownloadCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
            tbMacDownload.Visibility = System.Windows.Visibility.Hidden;
            pbProgressArc.Visibility = System.Windows.Visibility.Hidden;

            UpdateNetworkPeers();
        }

        /// <summary>
        /// Function which is called when wlan state changes.
        /// </summary>
        /// <param name="newState">New state.</param>
        private void UpdateNetworkState(WlanApi.WLAN_HOSTED_NETWORK_STATE newState = WlanApi.WLAN_HOSTED_NETWORK_STATE.Unavailable)
        {
            if (!grConfig.IsEnabled)
            {
                tbNetworkState.Text = "Status: Wireless radio disabled.";
                return;
            }

            WlanApi.WLAN_HOSTED_NETWORK_STATE state = newState;

            if (state == WlanApi.WLAN_HOSTED_NETWORK_STATE.Unavailable)
            {
                hostedNetwork.RefreshNetworkData();
                state = hostedNetwork.NetworkStatus.HostedNetworkState;
            }
           
            tbNetworkState.Text = "Status: ";
            switch (state)
            {
                case WlanApi.WLAN_HOSTED_NETWORK_STATE.Active:
                    tbNetworkState.Text += "AP is active.";
                    btnStart.Background = new SolidColorBrush(Color.FromRgb(0xBE, 0x5E, 0x5E));
                    btnStart.Content = "Stop AP";
                    txtKey.IsEnabled = false;
                    txtSSID.IsEnabled = false;
                    skipCheckUpdate = true;
                    chkAPAllowed.IsChecked = true;
                    skipCheckUpdate = false;
                    cmbDevice.IsEnabled = false;
                    btnRefreshNetworkDevices.IsEnabled = false;
                    break;
                case WlanApi.WLAN_HOSTED_NETWORK_STATE.Idle:
                    tbNetworkState.Text += "AP is ready.";
                    btnStart.IsEnabled = true;
                    btnStart.Background = new SolidColorBrush(Color.FromRgb(0x79, 0xBE, 0x5E));
                    btnStart.Content = "Start AP";
                    skipCheckUpdate = true;
                    chkAPAllowed.IsChecked = true;
                    skipCheckUpdate = false;
                    txtKey.IsEnabled = true;
                    txtSSID.IsEnabled = true;
                    cmbDevice.IsEnabled = true;
                    btnRefreshNetworkDevices.IsEnabled = true;
                    break;
                case WlanApi.WLAN_HOSTED_NETWORK_STATE.Unavailable:
                    tbNetworkState.Text += "AP is disabled or unavailable.";
                    btnStart.IsEnabled = false;
                    skipCheckUpdate = true;
                    chkAPAllowed.IsChecked = false;
                    skipCheckUpdate = false;
                    cmbDevice.IsEnabled = false;
                    btnRefreshNetworkDevices.IsEnabled = false;
                    txtKey.IsEnabled = false;
                    txtSSID.IsEnabled = false;
                    break;
            }
        }

        /// <summary>
        /// Sets wlan network enable state.
        /// </summary>
        /// <param name="isEnabled">True if enabled, false if not.</param>
        private void SetNetworkEnabled(bool isEnabled)
        {
            grConfig.IsEnabled = isEnabled;
            UpdateNetworkState();
        }

        /// <summary>
        /// Function which is called when peer state changes like
        /// peer connecting to network or disconnecting.
        /// </summary>
        private void UpdateNetworkPeers()
        {
            hostedNetwork.RefreshNetworkData();
            
            peerPanel.Children.Clear();
            peerPanel.CanVerticallyScroll = true;

            foreach(var p in hostedNetwork.NetworkStatus.PeerList)
            {
                PeerListItem pli = new PeerListItem();
                pli.peerName.Text = "User: " + p.PeerAuthState.ToString();
                pli.peerMac.Text = WlanApi.GetMACAddressAsString(p.PeerMacAddress);

                if (macDiscovery.IsDownloadComplete)
                {
                    pli.peerInfo.Text = macDiscovery.GetCompanyNameFromMacAddess(p.PeerMacAddress);
                }
                else
                {
                    pli.peerInfo.Text = "";
                }

                peerPanel.Children.Add(pli);
            }


        }

        /// <summary>
        /// Function which is called when wlan radio state changes.
        /// </summary>
        /// <param name="hardwareState">Current hardware state of wireless device.</param>
        /// <param name="softwareState">Current software state of wireless device.</param>
        private void networkNotification_RadioStateChanged(WlanApi.DOT11_RADIO_STATE hardwareState, WlanApi.DOT11_RADIO_STATE softwareState)
        {
            if (hardwareState == WlanApi.DOT11_RADIO_STATE.OffState || softwareState == WlanApi.DOT11_RADIO_STATE.OffState)
            {
                this.Dispatcher.Invoke(new SetNetworkStateDelegate(SetNetworkEnabled), new object[] { false });
            }
            else
            {
                this.Dispatcher.Invoke(new SetNetworkStateDelegate(SetNetworkEnabled), new object[] { true });
            }
        }

        /// <summary>
        /// Function which is called when wlan peer state changes.
        /// </summary>
        /// <param name="oldState">State of peer prior to change.</param>
        /// <param name="newState">New state of peer.</param>
        /// <param name="reason">Reson for change.</param>
        private void networkNotification_PeerStateChanged(WlanApi.WLAN_HOSTED_NETWORK_PEER_STATE oldState, WlanApi.WLAN_HOSTED_NETWORK_PEER_STATE newState, WlanApi.WLAN_HOSTED_NETWORK_REASON reason)
        {
            this.Dispatcher.Invoke(new UpdateNetworkPeersDelegate(UpdateNetworkPeers));
        }


        /// <summary>
        /// Function which is called when hosted network wlan state changes.
        /// </summary>
        /// <param name="oldState">State that was prior to change.</param>
        /// <param name="newState">New state of hosted network.</param>
        /// <param name="reason">Reson for change.</param>
        private void networkNotification_StateChanged(WlanApi.WLAN_HOSTED_NETWORK_STATE oldState, WlanApi.WLAN_HOSTED_NETWORK_STATE newState, WlanApi.WLAN_HOSTED_NETWORK_REASON reason)
        {
            this.Dispatcher.Invoke(new UpdateNetworkStatusDelegate(UpdateNetworkState), new object[] { newState });
        }

        /// <summary>
        /// Function which is called when window is loaded.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);



            if (!isAdmin)
            {
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                proc.Verb = "runas";

                try
                {
                    MessageBox.Show("T" + isAdmin.ToString());
                    Process.Start(proc);
                }
                catch
                {
                    return;
                }

                Application.Current.Shutdown();
            }
            else
            {

            }
        }

        /// <summary>
        /// Function which is called when SSID name text is changed.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void txtSSID_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isKeyFieldValid = false;

            if (txtKey != null)
            {
                isKeyFieldValid = !string.IsNullOrWhiteSpace(txtKey.Text) && txtKey.Text.Length >= 8;
            }

            bool isSSIDFieldValid = !string.IsNullOrWhiteSpace(txtSSID.Text) && txtSSID.Text.Length <= 32;
            btnStart.IsEnabled = isKeyFieldValid && isSSIDFieldValid;

            if (!isSSIDFieldValid)
            {
                txtSSID.BorderBrush = new SolidColorBrush(Color.FromRgb(160, 10, 10));
            }
            else
            {
                txtSSID.BorderBrush = new SolidColorBrush(Color.FromRgb(0x79, 0xBE, 0x5E));
            }
        }

        /// <summary>
        /// Function which is called when wlan key changes.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void txtKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isKeyFieldValid = !string.IsNullOrWhiteSpace(txtKey.Text) && txtKey.Text.Length >= 8;
            bool isSSIDFieldValid = !string.IsNullOrWhiteSpace(txtSSID.Text) && txtSSID.Text.Length <= 32;
            btnStart.IsEnabled = isKeyFieldValid && isSSIDFieldValid;

            if (!isKeyFieldValid)
            {
                txtKey.BorderBrush = new SolidColorBrush(Color.FromRgb(160, 10, 10));
            }
            else
            {
                txtKey.BorderBrush = new SolidColorBrush(Color.FromRgb(0x79, 0xBE, 0x5E));
            }
        }

        /// <summary>
        /// Function which is called when refresh devices button is clicked.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void btnRefreshNetworkDevices_Click(object sender, RoutedEventArgs e)
        {
            cmbDevice.Items.Clear();

            List<SharingNetworkAdapter> deviceList = hostedNetwork.GetSharingDevices();
            
            SharingNetworkAdapter activeAdapter = null;

            foreach (SharingNetworkAdapter device in deviceList)
            {
                cmbDevice.Items.Add(device);

                if (device.Status == tagNETCON_STATUS.NCS_CONNECTED)
                {
                    activeAdapter = device;
                }
            }

            if (activeAdapter != null)
            {
                cmbDevice.SelectedItem = activeAdapter;

                if (!macDiscovery.IsDownloadInProgress && !macDiscovery.IsDownloadComplete)
                {
                    macDiscovery.DownloadList();
                }
            }
        }


        /// <summary>
        /// Function which is called when AP allowed checkbox is checked.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void chkAPAllowed_Checked(object sender, RoutedEventArgs e)
        {
            if (skipCheckUpdate)
            {
                return;
            }
            skipCheckUpdate = true;
            hostedNetwork.IsNetworkAllowed = true;
            hostedNetwork.RefreshNetworkData();
            if (hostedNetwork.NetworkStatus.HostedNetworkState == WlanApi.WLAN_HOSTED_NETWORK_STATE.Unavailable)
            {
                chkAPAllowed.IsChecked = false;
            }
            skipCheckUpdate = false;
        }

        /// <summary>
        /// Function which is called when AP allowed checkbox is unchecked.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void chkAPAllowed_Unchecked(object sender, RoutedEventArgs e)
        {
            if (skipCheckUpdate)
            {
                return;
            }
            skipCheckUpdate = true;
            hostedNetwork.IsNetworkAllowed = false;
            skipCheckUpdate = false;
        }

        /// <summary>
        /// Function which is called when start/stop ap button is clicked.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            hostedNetwork.RefreshNetworkData();
            if (hostedNetwork.NetworkStatus.HostedNetworkState == WlanApi.WLAN_HOSTED_NETWORK_STATE.Active)
            {
                hostedNetwork.Stop();
                hostedNetwork.DisableSharing();
                return;
            }
            hostedNetwork.Start();
            hostedNetwork.GetSharingDevices();
            hostedNetwork.ShareConnectionFrom((SharingNetworkAdapter)cmbDevice.SelectedItem);
        }

        /// <summary>
        /// Function which is called when switching out focus from network SSID textbox.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void txtSSID_LostFocus(object sender, RoutedEventArgs e)
        {
            bool isSSIDFieldValid = !string.IsNullOrWhiteSpace(txtSSID.Text) && txtSSID.Text.Length <= 32;

            if (isSSIDFieldValid)
            {
                hostedNetwork.SetSSIDAndMaxConnections(txtSSID.Text, 100);
            }
        }

        /// <summary>
        /// Function which is called when switching out focus from network Key textbox.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void txtKey_LostFocus(object sender, RoutedEventArgs e)
        {
            bool isKeyFieldValid = !string.IsNullOrWhiteSpace(txtKey.Text) && txtKey.Text.Length >= 8;

            if (isKeyFieldValid)
            {
                hostedNetwork.NetworkKey = txtKey.Text;
            }
        }
    }
}
