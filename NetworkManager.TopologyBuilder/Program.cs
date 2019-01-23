using log4net;
using NetworkManager.Model.Topology;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace NetworkManager.TopologyBuilder
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Dictionary<ulong, DeviceType> LoadDeviceTypes(OracleConnection connection)
        {
            log.Info("Loading device types");

            var result = new Dictionary<ulong, DeviceType>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"select id, name, switchable from oms_metafeatures where id > 0";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(
                            ulong.Parse(reader["id"].ToString()),
                            new DeviceType
                            {
                                Id = ulong.Parse(reader["id"].ToString()),
                                Name = reader["name"].ToString(),
                                IsSwitchable = reader["switchable"].ToString() == "T",
                                IsGenerator = reader["name"].ToString() == "Circuit Breaker",
                                IsServicePoint = reader["name"].ToString() == "Service Point"
                            }
                        );
                    }
                }
            }
            log.Info("Device types loaded");
            return result;
        }

        private static IEnumerable<Device> LoadDevices(OracleConnection connection, Dictionary<ulong, DeviceType> deviceTypes)
        {
            log.Info("Loading Devices...");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select mslink, feature_id, normal_status, x_coord, y_coord from oms_connectivity where x_coord is not null and y_coord is not null";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    { 
                        yield return new Device
                        {
                            Id = ulong.Parse(reader["mslink"].ToString()),
                            Type = deviceTypes[ulong.Parse(reader["feature_id"].ToString())],
                            CanConduct = reader["normal_status"].ToString() == "C",
                            Position = new Model.Geography.LatLng(long.Parse(reader["x_coord"].ToString()), long.Parse(reader["y_coord"].ToString()))
                        };
                    }
                }
            }

            log.Info("Devices Loaded");
        }

        private static IEnumerable<Tuple<ulong,ulong>> LoadEdges(OracleConnection connection)
        {
            log.Info("Connecting Devices...");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"select
                  lhs.mslink as lhs,
                  rhs.mslink as rhs
                from
                  oms_connectivity lhs
                  inner join oms_connectivity rhs on
                  (
                    -- Devices are adjacent if they share a common node value...
                    decode(lhs.node1,0,null, lhs.node1) = decode(rhs.node1,0,null, rhs.node1)
                    or decode(lhs.node1,0,null, lhs.node1) = decode(rhs.node2,0,null, rhs.node2)
                    or decode(lhs.node2,0,null, lhs.node2) = decode(rhs.node1,0,null, rhs.node1)
                    or decode(lhs.node2,0,null, lhs.node2) = decode(rhs.node2,0,null, rhs.node2)
                  )
                where
                  lhs.mslink<rhs.mslink
                and lhs.x_coord is not null
                and lhs.y_coord is not null
                and rhs.x_coord is not null
                and rhs.y_coord is not null";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new Tuple<ulong, ulong>(ulong.Parse(reader["lhs"].ToString()), ulong.Parse(reader["rhs"].ToString()));
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            log.Info("NetworkManager Topology Builder Starting");
            using (var connection = new OracleConnection(ConfigurationManager.ConnectionStrings["OMS"].ConnectionString))
            {
                connection.Open();

                var topology = new NetworkTopology();
                var deviceTypes = LoadDeviceTypes(connection);
                topology.Add(LoadDevices(connection, deviceTypes));
                topology.Connect(LoadEdges(connection));
                topology.EnergizeNetwork();

                using (var fs = new FileStream(ConfigurationManager.AppSettings["NetworkTopologyPath"], FileMode.Create))
                {
                    topology.Save(fs);
                }
            }
        }
    }
}
