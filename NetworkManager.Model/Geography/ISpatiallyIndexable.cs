using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkManager.Model.Geography
{
    public interface ISpatiallyIndexable
    {
        GeoAPI.Geometries.Envelope Envelope { get; }
    }
}
