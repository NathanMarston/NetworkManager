using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using NetworkManager.Common;

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

        /// <summary>
        /// The devices, keyed by ID
        /// </summary>
        private readonly Dictionary<ulong, Device> _devices = new Dictionary<ulong, Device>();

        /// <summary>
        /// The generators
        /// </summary>
        private HashSet<Device> _generators = new HashSet<Device>();

        /// <summary>
        /// Synchronization primitive
        /// </summary>
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

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

                // Add to visited list, and execute visiting function
                visited.Add(device);
                visitor(device);

                // Enqueue the non-visited, adjacent devices to this device
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


        private delegate bool DevicePredicate(Device input);

        /// <summary>
        /// Check that the network has been energized correctly
        /// </summary>
        /// <param name="getCanConduct">Get the value of the "CanConduct" attribute of a device</param>
        /// <param name="getIsEnergized">Get the value of the "IsEnergized" attribute of a device</param>
        [Conditional("DEBUG")]
        private void CheckEnergization(DevicePredicate canConduct, DevicePredicate isEnergized)
        {
            // All generators in the closed state are energized
            Contract.Assert(_generators.Where(g => canConduct(g)).All(d => isEnergized(d)),
                "Not all active generators have been energized!");

            // All devices in the open state are de-energized
            Contract.Assert(_devices.Values.Where(d => !canConduct(d)).All(d => !isEnergized(d)),
                "Not all open devices are de-energized!");

            // For each pair of adjacent, conducting devices, either both devices are energized or both devices are de-energized
            Contract.Assert(_devices.Values.AsParallel().Where(d => canConduct(d)).
                SelectMany(d => d.AdjacentDevices.Where(ad => canConduct(ad)), (d, ad) => new Tuple<Device, Device>(d, ad)).
                All(x => isEnergized(x.Item1) == isEnergized(x.Item2)),
                "Inconsistent network energization detected!");
        }

        /// <summary>
        /// Check that the network has been energized correctly
        /// </summary>
        [Conditional("DEBUG")]
        private void CheckEnergization()
        {
            CheckEnergization(d => d.CanConduct, d => d.IsEnergized);   
        }

        /// <summary>
        /// Determine which devices are energized and which are not
        /// </summary>
        public void EnergizeNetwork()
        {
            using (var scope = _lock.Write())
            {
                // De-energize everything
                Parallel.ForEach(
                    _devices.Values,
                    d => d.IsEnergized = false);

                Parallel.ForEach(
                    _generators.Where(g => g.CanConduct), // For all energized generators...
                    g => TraverseDepthFirst(
                        new[] { g }, // Start a traversal from the generator...
                        (currentDevice, nextDevice) => !nextDevice.IsEnergized && nextDevice.CanConduct, // Following devices that can conduct electricity...
                        d => d.IsEnergized = true // Marking the devices that we find energized
                        )
                );

                CheckEnergization();
            }
        }

        /// <summary>
        /// Add devices to the network model
        /// </summary>
        /// <param name="devices"></param>

        public void Add(IEnumerable<Device> devices)
        {
            using (var scope = _lock.Write())
            {
                foreach (var device in devices)
                {
                    _devices.Add(device.Id, device);
                    if (device.Type.IsGenerator) _generators.Add(device);
                }
            }
        }

        /// <summary>
        /// Remove devices from the network model
        /// </summary>
        /// <param name="devices"></param>
        public void Remove(IEnumerable<Device> devices)
        {
            using (var scope = _lock.Write())
            {
                foreach (var device in devices)
                {
                    _devices.Remove(device.Id);
                    _generators.Remove(device);
                }
            }
        }

        public void Connect(IEnumerable<Tuple<ulong, ulong>> devicesToConnect)
        {
            using (var scope = _lock.Write())
            {
                foreach (var deviceIdPair in devicesToConnect)
                {
                    _devices[deviceIdPair.Item1].AdjacentDevices.Add(_devices[deviceIdPair.Item2]);
                    _devices[deviceIdPair.Item2].AdjacentDevices.Add(_devices[deviceIdPair.Item1]);
                }
            }
        }

        public void Disconnect(IEnumerable<Tuple<ulong, ulong>> devicesToDisconnect)
        {
            using (var scope = _lock.Write())
            {
                foreach (var deviceIdPair in devicesToDisconnect)
                {
                    _devices[deviceIdPair.Item1].AdjacentDevices.Remove(_devices[deviceIdPair.Item2]);
                    _devices[deviceIdPair.Item2].AdjacentDevices.Remove(_devices[deviceIdPair.Item1]);
                }
            }
        }

        #region Opening Devices
        /// <summary>
        /// Figure out which devices should be de-energized as a result of opening some devices
        /// </summary>
        /// <param name="deviceIDs">The IDs of the devices to be de-energized</param>
        /// <returns></returns>
        private HashSet<Device> OpenDevicesImplementation(ulong[] deviceIDs)
        {
            // Get the devices that actually need to be opened
            var devicesToOpen = new HashSet<Device>(deviceIDs.Select(id => _devices[id]).Where(d => d.CanConduct));

            // This represents to set of devices de-energized by opening the devices passed
            var result = new HashSet<Device>();

            // Figure out which devices will be de-energized by opening the devices passed
            foreach (var d in devicesToOpen.Where(d2 => d2.IsEnergized)) 
                // Only consider the input devices that are energized (opening an already de-energized device does nothing to the network)
            {
                result.Add(d);

                // For each device adjacent to the device being opened, figure out whether it (and anything reachable from it) is still energized
                // by trying to find a path of conducting devices back to a generator.
                foreach (var ad in d.AdjacentDevices.Where(d2 => d2.IsEnergized))
                {
                    bool shouldBeEnergized = false;
                    var visited = TraverseDepthFirst(
                        // Start at the adjacent device
                        new HashSet<Device> { ad },
                        // Traverse through conducting devices, except those in the process of being opened
                        // Terminate the search if we find this sub-network should be energized
                        (current, next) => !shouldBeEnergized && next.CanConduct && !devicesToOpen.Contains(next),
                        // If we encounter a generator, it means the sub-network should remain energized
                        x => { if (x.Type.IsGenerator) shouldBeEnergized = true; }
                    );

                    // If we finish searching without finding a generator, all devices visited should be de-energized
                    if (!shouldBeEnergized)
                        result.UnionWith(visited);
                }
            }

            return result;
        }

        /// <summary>
        /// Test opening some devices (without changing the network state)
        /// </summary>
        /// <param name="deviceIDs"></param>
        /// <returns>The set of devices de-energized by opening the input devices</returns>
        public HashSet<Device> TestOpeningDevices(ulong[] deviceIDs)
        {
            using (var scope = _lock.Read())
            {
                var result = OpenDevicesImplementation(deviceIDs);
                CheckEnergization(
                    d => deviceIDs.Contains(d.Id) ? false : d.CanConduct,
                    d => result.Contains(d) ? false : d.IsEnergized
                );
                return result;
            }
        }

        /// <summary>
        /// Open some devices, and return the set of devices de-energized
        /// </summary>
        /// <param name="deviceIDs"></param>
        /// <returns>The set of devices de-energized by opening the input devices</returns>
        public HashSet<Device> OpenDevices(ulong[] deviceIDs)
        {
            using (var scope = _lock.Write())
            {
                var deenergizedDevices = OpenDevicesImplementation(deviceIDs);

                foreach (var id in deviceIDs)
                {
                    _devices[id].CanConduct = false;
                }

                foreach (var d in deenergizedDevices)
                {
                    d.IsEnergized = false;
                }

                CheckEnergization();

                return deenergizedDevices;
            }
        }
        #endregion

        #region Closing Devices
        /// <summary>
        /// Find the set of devices energized by closing some set of devices
        /// </summary>
        /// <param name="deviceIDs"></param>
        /// <returns></returns>
        private HashSet<Device> CloseDevicesImplementation(ulong[] deviceIDs)
        {
            // Get the set of devices we actually need to close
            var devicesToClose = new HashSet<Device>(
                deviceIDs.Select(id => _devices[id]).Where(d => !d.CanConduct));

            return TraverseDepthFirst(
                // If the device being closed is a generator, or has ay adjacent energized devices, then closing device "x" will cause it to
                // become energized, as well as anything electrically reachable from it.
                new HashSet<Device>(
                    devicesToClose.Where(x => x.Type.IsGenerator || x.AdjacentDevices.Any(d2 => d2.IsEnergized))),
                // Traverse to de-energized, closed devices only
                (current, next) => next.CanConduct && !next.IsEnergized,
                // Do nothing... just find out what needs to be energized
                x => { }
            );
        }

        /// <summary>
        /// Test opening some devices
        /// </summary>
        /// <param name="deviceIDs"></param>
        /// <returns>The set of devices de-energized by opening those devices</returns>
        public HashSet<Device> TestClosingDevices(ulong[] deviceIDs)
        {
            using (var scope = _lock.Read())
            {
                var result = CloseDevicesImplementation(deviceIDs);

                CheckEnergization(
                    d => deviceIDs.Contains(d.Id) ? true : d.CanConduct,
                    d => result.Contains(d) ? true : d.IsEnergized
                );

                return result;
            }
        }

        public HashSet<Device> CloseDevices(ulong[] deviceIDs)
        {
            using (var scope = _lock.Write())
            {
                var result = CloseDevicesImplementation(deviceIDs);

                foreach (var id in deviceIDs)
                {
                    _devices[id].CanConduct = true;
                }

                foreach (var d in result)
                {
                    d.IsEnergized = true;
                }

                CheckEnergization();

                return result;
            }
        }
        #endregion
    }
}
