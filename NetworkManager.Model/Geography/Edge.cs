using GeoAPI.Geometries;
using NetworkManager.Model.Topology;
using System;

namespace NetworkManager.Model.Geography
{
    /// <summary>
    /// An edge represents a straight line that connects a pair of devices
    /// </summary>
    public class Edge : ISpatiallyIndexable
    {
        public Device LHS { get; private set; }
        public Device RHS { get; private set; }
        
        public Envelope Envelope { get; private set; }

        public Edge(Device lhs, Device rhs)
        {
            LHS = lhs;
            RHS = rhs;

            // Calculate the envelope for the edge
            Envelope = new Envelope(
                new Coordinate(
                    Math.Min(lhs.Envelope.MinX, rhs.Envelope.MinX),
                    Math.Max(lhs.Envelope.MinY, rhs.Envelope.MinY)),
                new Coordinate(
                    Math.Max(lhs.Envelope.MaxX, rhs.Envelope.MaxX),
                    Math.Max(lhs.Envelope.MaxY, rhs.Envelope.MaxY))
            );
        }
    }
}
