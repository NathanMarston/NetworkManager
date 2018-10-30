using System.Collections.Generic;

namespace NetworkManager.Model.Topology
{
    public class DeviceType
    {
        /// <summary>
        /// A unique ID for the device, within a network model.
        /// </summary>
        /// <value>The identifier.</value>
        public ulong Id { get; internal set; }

        /// <summary>
        /// The device type's descriptive name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Whether this type of device is switchable (i.e. can change state from closed to open and back again)
        /// </summary>
        public bool IsSwitchable { get; internal set; }

        /// <summary>
        /// Whether this device type represents a generator of electricity
        /// </summary>
        public bool IsGenerator { get; internal set; }

        /// <summary>
        /// Whether this device type represents a boundary between the network and one or more customers
        /// </summary>
        public bool IsServicePoint { get; internal set; }
    }
}
