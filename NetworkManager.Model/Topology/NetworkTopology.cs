using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;

namespace NetworkManager.Model.Topology
{
    /// <summary>
    /// Represents the topology of an electrical network
    /// </summary>
    public class NetworkTopology
    {
        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Dictionary<ulong, Device> Devices { get; private set; }
        public HashSet<Device> Generators { get; private set; }

        /// <summary>
        /// Indexer used to get devices by their Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Device this[ulong id] => Devices[id];

        private delegate bool TraversalPredicate(Device currentDevice, Device nextDevice);

        private delegate void TraversalVisitor(Device device);

        /// <summary>
        /// Perform a depth-first traversal of the network topology
        /// </summary>
        /// <param name="startingDevices">The devices to start the traversal from</param>
        /// <param name="traversalPredicate">A predicate used to determine which adjacent devices to traverse to next</param>
        /// <param name="visitor">An action to perform at each device visited</param>
        /// <returns></returns>
        private HashSet<Device> TraverseDepthFirst(IEnumerable<Device> startingDevices,
            TraversalPredicate traversalPredicate, TraversalVisitor visitor)
        {
            var visited = new HashSet<Device>();
            var expansionStack = new Stack<Device>(startingDevices);

            while (expansionStack.Any())
            {
                var device = expansionStack.Pop();

                // Only visit a device once
                if (visited.Contains(device)) continue;
                visited.Add(device);
                visitor(device);

                foreach (var nextDevice in device.AdjacentDevices)
                {
                    if (traversalPredicate(device, nextDevice) && !visited.Contains(nextDevice))
                    {
                        expansionStack.Push(nextDevice);
                    }
                }
            }

            return visited;
        }

        /// <summary>
        /// Determine which devices are energized and which are not
        /// </summary>
        public void EnergizeNetwork()
        {
            Parallel.ForEach(
                Devices.Values,
                d => d.IsEnergized = false);


            Parallel.ForEach(
                Generators.Where(g => g.CanConduct), // For all energized generators...
                g => TraverseDepthFirst( 
                    new[]{g}, // Start a traversal from the generator...
                    (currentDevice, nextDevice) => !nextDevice.IsEnergized && nextDevice.CanConduct, // Following devices that can conduct electricity...
                    d => d.IsEnergized = true // Marking the devices that we find energized
                    )
            );
        }
    }
}
