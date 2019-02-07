using GeoAPI.Geometries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NetworkManager.DTO;
using NetworkManager.Model.Geography;
using NetworkManager.Model.Topology;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetworkManager.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeographyController : ControllerBase
    {
        // Static so it can be shared across controllers within the same worker process
        public static NetworkGeography Geography { get; private set; }

        public GeographyController(IConfiguration configuration)
        {
            // Load the topology first, if it hasn't been loaded yet
            if (TopologyController.Topology == null)
            {
                var path = configuration["NetworkTopology:Path"];
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    TopologyController.Topology = new NetworkTopology(fs);
                }
            }

            // Load the geography from the topology, if required
            if (Geography == null)
            {
                Geography = new NetworkGeography(TopologyController.Topology);
            }
        }

        /// <summary>
        /// Return the set of spatial elements within a given envelope
        /// </summary>
        /// <returns></returns>
        [HttpGet("{minLat}/{minLng}/{maxLat}/{maxLng}")]
        public NetworkElementsDTO Query(double minLat, double minLng, double maxLat, double maxLng)
        {
            var envelope = new Envelope(
                new Coordinate(minLat, minLng),
                new Coordinate(maxLat, maxLng));
            var elements = Geography.Query(envelope);
            return new NetworkElementsDTO
            {
                Devices = elements.OfType<Device>().Select(
                    d => new DeviceDTO
                    {
                        Id = d.Id,
                        DeviceTypeId = d.Type.Id,
                        CanConduct = d.CanConduct,
                        IsEnergized = d.IsEnergized,
                        Latitude = d.Position.Latitude,
                        Longitude = d.Position.Longitude
                    }).ToList(),
                Edges = elements.OfType<Edge>().Select(
                    e => new NetworkEdgeDTO
                    {
                        lhsLatitude = e.LHS.Position.Latitude,
                        lhsLongitude = e.LHS.Position.Longitude,
                        rhsLatitude = e.RHS.Position.Latitude,
                        rhsLongitude = e.RHS.Position.Longitude
                    }).ToList()
            };
        }
    }
}