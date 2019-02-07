using System.Collections.Generic;

namespace NetworkManager.DTO
{
    public class NetworkEdgeDTO
    {
        public double lhsLatitude;
        public double lhsLongitude;
        public double rhsLatitude;
        public double rhsLongitude;
    }

    /// <summary>
    /// Represents a collection of visual items
    /// </summary>
    public class NetworkElementsDTO
    {
        public List<DeviceDTO> Devices { get; set; }
        public List<NetworkEdgeDTO> Edges { get; set; }
    }
}
