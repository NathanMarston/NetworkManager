using System.Collections.Generic;

namespace NetworkManager.Model.Topology
{
    /// <summary>
    /// Represents a single device in a network model
    /// </summary>
    public class Device
    {
        /// <summary>
        /// The network topology object that this device belongs to
        /// </summary>
        public NetworkTopology Parent { get; internal set; }

        /// <summary>
        /// Unique ID for a device
        /// </summary>
        public ulong Id { get; internal set; }

        /// <summary>
        /// The device's type
        /// </summary>
        public DeviceType Type { get; internal set; }

        /// <summary>
        /// The devices which are physically connected to this one
        /// </summary>
        public HashSet<Device> AdjacentDevices { get; internal set; }

        /// <summary>
        /// Whether this device is currently capable of conducting electricity
        /// </summary>
        public bool CanConduct { get; internal set; }

        /// <summary>
        /// Whether this device is currently powered on
        /// </summary>
        public bool IsEnergized { get; internal set; }
    }
}
