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
using System.Text.RegularExpressions;
using System.Threading;
using System.ComponentModel;
using System.Net;

namespace HostedNetwork.Wlan
{

    /// <summary>
    /// Class which downloads and performs mac address translation
    /// to their respective company names by using first 3 bytes of the mac.
    /// </summary>
    class MacDiscovery
    {
        /// <summary>
        /// URL containing a list of MAC addresses and company names.
        /// </summary>
        const string MacListURL = "http://standards-oui.ieee.org/oui.txt";

        /// <summary>
        /// Regular expression used to get a company name from mac address.
        /// </summary>
        const string MacRegex = @"  {0}\s+\(base 16\)\t+(.*)\n";

        /// <summary>
        /// String builder which holds list of mac addresses and company names.
        /// </summary>
        StringBuilder macList;

        /// <summary>
        /// Web client used for downloading a mac list.
        /// </summary>
        WebClient webClient;

        /// <summary>
        /// Event which fires when download progress has changed.
        /// </summary>
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        /// <summary>
        /// Event which fires when download is complete.
        /// </summary>
        public event DownloadStringCompletedEventHandler DownloadCompleted;

        /// <summary>
        /// Holds value wether or bit event is set.
        /// </summary>
        bool isEventSet = false;

        /// <summary>
        /// Holds value wether or not download is complete.
        /// </summary>
        bool isDownloadComplete = false;

        /// <summary>
        /// Holds value wether or not download is currently in progress.
        /// </summary>
        bool isDownloadInProgress = false;

        /// <summary>
        /// Gets value wether or not download is complete.
        /// </summary>
        public bool IsDownloadComplete
        {
            get
            {
                return this.isDownloadComplete;
            }
        }

        /// <summary>
        /// Gets value wether download is currently in progress.
        /// </summary>
        public bool IsDownloadInProgress
        {
            get
            {
                return this.isDownloadInProgress;
            }
        }

        /// <summary>
        /// Creates new instance of MacDiscovery class.
        /// </summary>
        public MacDiscovery()
        {
            macList = new StringBuilder();
            webClient = new WebClient();
            webClient.DownloadStringCompleted += webClient_DownloadStringCompleted;
        }

        /// <summary>
        /// Performs downloading of mac list if not already downloaded.
        /// If list is already downloaded this function does nothing.
        /// </summary>
        public void DownloadList()
        {
            if (IsDownloadComplete)
            {
                return;
            }

            if (!isEventSet)
            {
                webClient.DownloadProgressChanged += DownloadProgressChanged;
                webClient.DownloadStringCompleted += DownloadCompleted;
                
                isEventSet = true;
            }
            
            webClient.DownloadStringAsync(new Uri(MacDiscovery.MacListURL));
            isDownloadInProgress = true;
        }

        /// <summary>
        /// Event which fires when mac discovery data is downloaded.
        /// </summary>
        /// <param name="sender">Object which sent the event.</param>
        /// <param name="e">Event arguments.</param>
        void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            isDownloadComplete = true;
            isDownloadInProgress = false;

            try
            {
                macList.Append(e.Result);
            }
            catch
            {
                isDownloadComplete = false;
                isDownloadInProgress = false;
            }
        }

        /// <summary>
        /// Returns a companyName from mac address.
        /// </summary>
        /// <param name="macAddress">DOT11_MAC_ADDRESS from which company name will be returned.</param>
        /// <param name="unknownReturn">String which will be returned is mac address is not found in the downloaded list.</param>
        /// <returns>Company name</returns>
        public string GetCompanyNameFromMacAddess(WlanApi.DOT11_MAC_ADDRESS macAddress, string unknownReturn = "Unknown")
        {
            string base16Mac = 
                macAddress.byte1.ToString("X2") + 
                macAddress.byte2.ToString("X2") + 
                macAddress.byte3.ToString("X2");

            Regex regex = new Regex(String.Format(MacRegex, base16Mac));

            Match match = regex.Match(macList.ToString());

            if (match.Length > 0)
            {
                return match.Groups[1].Value;
            }

            return unknownReturn;
        }
    }
}
