using GeoAPI.Geometries;
using NetTopologySuite.Index.Strtree;
using NetworkManager.Model.Topology;
using System.Collections.Generic;
using System.Linq;

namespace NetworkManager.Model.Geography
{
    public class NetworkGeography
    {
        /// <summary>
        /// An STR tree of devices and edges
        /// </summary>
        STRtree<ISpatiallyIndexable> _geospatialElements = new STRtree<ISpatiallyIndexable>();

        /// <summary>
        /// Create a geographical view of the network
        /// </summary>
        /// <param name="topology"></param>
        public NetworkGeography(NetworkTopology topology)
        {
            foreach (var device in topology.Devices.Values)
            {
                // Add the device to the spatial index
                _geospatialElements.Insert(device.Envelope, device);

                // Add the edges to the tree (check on Id is due to edges being bi-directional, only want to add once)
                foreach (var adjacentDevice in device.AdjacentDevices.Where(ad => ad.Id > device.Id))
                {
                    var edge = new Edge(device, adjacentDevice);
                    _geospatialElements.Insert(edge.Envelope, edge);
                }
            }

            _geospatialElements.Build();
        }

        /// <summary>
        /// Query the geospatial model for all entities that fall within a given viewport
        /// </summary>
        /// <param name="viewPort"></param>
        /// <returns></returns>
        public IList<ISpatiallyIndexable> Query(Envelope viewPort)
        {
            return _geospatialElements.Query(viewPort);
        }
    }
}
