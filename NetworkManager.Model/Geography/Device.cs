using GeoAPI.Geometries;
using NetworkManager.Model.Geography;

namespace NetworkManager.Model.Topology
{
    /// <summary>
    /// Extensions to the "Device" model class to support geospatial indexing of devices
    /// </summary>
    public partial class Device : ISpatiallyIndexable
    {
        private LatLng _position;

        public LatLng Position
        {
            get { return _position; }
            set { _position = value; Envelope = new Envelope(new Coordinate(_position.Latitude, _position.Longitude, 0)); }
        }

        public Envelope Envelope { get; private set; }
    }
}
