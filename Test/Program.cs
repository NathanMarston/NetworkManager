using NetworkManager.Model.Topology;
using Newtonsoft.Json;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new NetworkTopology();
            var deviceType = new DeviceType { Id = 1, Name = "Test", IsGenerator = false, IsSwitchable = false, IsServicePoint = false };
            var device1 = new Device { Id = 1, Type = deviceType, CanConduct = false, IsEnergized = false };
            var device2 = new Device { Id = 2, Type = deviceType, CanConduct = false, IsEnergized = false };
            test.Add(new Device[]{ device1, device2 });
            test.Connect(new Tuple<ulong, ulong>[] { new Tuple<ulong, ulong>(1, 2) });


            string json = JsonConvert.SerializeObject(test, Formatting.Indented);
            var test2 = JsonConvert.DeserializeObject<NetworkTopology>(json);
        }
    }
}
