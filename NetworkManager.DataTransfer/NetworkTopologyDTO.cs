using System;

namespace NetworkManager.DataTransfer
{
    /// <summary>
    /// Serializable data transfer object for network topology
    /// </summary>
    public struct NetworkTopologyDTO
    {
        public DeviceTypeDTO[] DeviceTypes;
        public DeviceDTO[] Devices;
        public Tuple<ulong, ulong>[] Edges;
    }
}
