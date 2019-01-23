using System.Collections.Generic;

namespace NetworkManager.Model.Topology
{
    /// <summary>
    /// Represents a single device in a network model
    /// </summary>
    public partial class Device
    {
        /// <summary>
        /// Unique ID for a device
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The device's type
        /// </summary>
        public DeviceType Type { get; set; }

        /// <summary>
        /// The devices which are physically connected to this one
        /// </summary>
        public HashSet<Device> AdjacentDevices { get; set; } = new HashSet<Device>();

        /// <summary>
        /// Whether this device is currently capable of conducting electricity
        /// </summary>
        public bool CanConduct { get; set; }

        /// <summary>
        /// Whether this device is currently powered on
        /// </summary>
        public bool IsEnergized { get; set; }
    }
}
