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
using NETCONLib;

namespace HostedNetwork.Wlan
{
    /// <summary>
    /// Wrapper class representing a network adapter which can be used to
    /// configure ICS.
    /// </summary>
    class SharingNetworkAdapter
    {
        /// <summary>
        /// Instance of adapter connection properties.
        /// </summary>
        INetConnectionProps properties;

        /// <summary>
        /// Instance of adapter sharing configuration.
        /// </summary>
        INetSharingConfiguration sharingConfiguration;

        /// <summary>
        /// Gets whether or not sharing is enabled.
        /// </summary>
        public bool IsSharingEnabled
        {
            get
            {
                return sharingConfiguration.SharingEnabled;
            }
        }

        /// <summary>
        /// Gets adapter GUID
        /// </summary>
        public string Guid
        {
            get
            {
                return properties.Guid;
            }
        }

        /// <summary>
        /// Gets adapter name.
        /// </summary>
        public string Name
        {
            get
            {
                return properties.Name;
            }
        }

        /// <summary>
        /// Gets adapter device name.
        /// </summary>
        public string DeviceName
        {
            get
            {
                return properties.DeviceName;
            }
        }


        /// <summary>
        /// Gets adapter media type.
        /// </summary>
        public tagNETCON_MEDIATYPE MediaType
        {
            get
            {
                return properties.MediaType;
            }
        }

        /// <summary>
        /// Gets adapter status.
        /// </summary>
        public tagNETCON_STATUS Status
        {
            get
            {
                return properties.Status;
            }
        }

        /// <summary>
        /// Disable sharing on this adapter.
        /// </summary>
        public void DisableSharing()
        {
            sharingConfiguration.DisableSharing();
        }

        /// <summary>
        /// Enable internet sharing on this adapter and set sharing type from one of the 
        /// available types in private (to) and public (from).
        /// </summary>
        /// <param name="shareType"></param>
        public void EnableSharing(tagSHARINGCONNECTIONTYPE shareType)
        {
            if (this.IsSharingEnabled && shareType == sharingConfiguration.SharingConnectionType)
            {
                return;
            }

            sharingConfiguration.EnableSharing(shareType);
        }

        /// <summary>
        /// Initializes new instance of sharing network adapter.
        /// </summary>
        /// <param name="sharingManager">Instance of network sharing manager used for ICS.</param>
        /// <param name="connection">Instance of network connection from which data will be retrieved.</param>
        public SharingNetworkAdapter(NetSharingManager sharingManager, INetConnection connection)
        {
            this.properties = sharingManager.NetConnectionProps[connection];
            this.sharingConfiguration = sharingManager.INetSharingConfigurationForINetConnection[connection];
        }

        public override string ToString()
        {
            return properties.Name;
        }

        public override bool Equals(object otherAdapter)
        {
            if (otherAdapter is SharingNetworkAdapter)
            {
                return this.Guid.Equals(((SharingNetworkAdapter)otherAdapter).Guid);
            }

            return base.Equals(otherAdapter);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
