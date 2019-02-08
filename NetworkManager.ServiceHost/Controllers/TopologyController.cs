using Microsoft.AspNetCore.Mvc;
using NetworkManager.DTO;
using NetworkManager.Model.Topology;
using System.Linq;

namespace NetworkManager.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopologyController : ControllerBase
    {
        // Static so it can be shared across controllers within the same worker process
        public static NetworkTopology Topology { get; set; }

        /// <summary>
        /// Return the set of device types in the network model
        /// </summary>
        /// <returns></returns>
        [HttpGet("DeviceTypes")]
        public DeviceTypeDTO[] GetDeviceTypes()
        {
            return Topology.Devices.Values.Select(d => d.Type).Distinct().Select(dt => new DeviceTypeDTO
            {
                Id = dt.Id,
                Name = dt.Name,
                IsSwitchable = dt.IsSwitchable,
                IsGenerator = dt.IsGenerator,
                IsServicePoint = dt.IsServicePoint
            }).ToArray();
        }

        /// <summary>
        /// Return a device's details given its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult<DeviceDTO> GetDeviceById(ulong id)
        {
            if (Topology.Devices.ContainsKey(id))
            {
                var device = Topology.Devices[id];
                return new DeviceDTO
                {
                    Id = device.Id,
                    DeviceTypeId = device.Type.Id,
                    CanConduct = device.CanConduct,
                    IsEnergized = device.IsEnergized,
                    Latitude = device.Position.Latitude,
                    Longitude = device.Position.Longitude
                };
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Test opening one or more devices (i.e. don't actually open them, just see what effect it would have)
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>The devices that would be de-energized if those devices were opened</returns>
        [HttpGet("TestOpeningDevices")]
        public ActionResult<DeviceDTO[]> TestOpeningDevices([FromQuery] ulong[] ids)
        {
            var result = Topology.TestOpeningDevices(ids);
            return result.Select(d => new DeviceDTO
            {
                Id = d.Id,
                DeviceTypeId = d.Type.Id,
                CanConduct = d.CanConduct,
                IsEnergized = d.IsEnergized,
                Latitude = d.Position.Latitude,
                Longitude = d.Position.Longitude
            }).ToArray();
        }

        /// <summary>
        /// Test closing one or more devices (i.e.  don't open them, just see what effect it would have)
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>The devices that would be energized if those devices were closed</returns>
        [HttpGet("TestClosingDevices")]
        public ActionResult<DeviceDTO[]> TestClosingDevices([FromQuery] ulong[] ids)
        {
            var result = Topology.TestClosingDevices(ids);
            return result.Select(d => new DeviceDTO
            {
                Id = d.Id,
                DeviceTypeId = d.Type.Id,
                CanConduct = d.CanConduct,
                IsEnergized = d.IsEnergized,
                Latitude = d.Position.Latitude,
                Longitude = d.Position.Longitude
            }).ToArray();
        }

        /// <summary>
        /// Open one or more devices
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>The devices de-energized as a result of those devices being opened</returns>
        [HttpPut("OpenDevices")]
        public ActionResult<DeviceDTO[]> OpenDevices([FromQuery] ulong[] ids)
        {
            var result = Topology.OpenDevices(ids);
            return result.Select(d => new DeviceDTO
            {
                Id = d.Id,
                DeviceTypeId = d.Type.Id,
                CanConduct = d.CanConduct,
                IsEnergized = d.IsEnergized,
                Latitude = d.Position.Latitude,
                Longitude = d.Position.Longitude
            }).ToArray();
        }

        /// <summary>
        /// Close one or more devices
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>The devices energied as a result of those devices being closed</returns>
        [HttpPut("CloseDevices")]
        public ActionResult<DeviceDTO[]> CloseDevices([FromQuery] ulong[] ids)
        {
            var result = Topology.CloseDevices(ids);
            return result.Select(d => new DeviceDTO { Id = d.Id, DeviceTypeId = d.Type.Id, CanConduct = d.CanConduct, IsEnergized = d.IsEnergized }).ToArray();
        }
    }
}
