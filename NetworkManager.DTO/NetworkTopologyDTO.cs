using ProtoBuf;

namespace NetworkManager.DTO
{
    [ProtoContract]
    public struct NetworkTopologyDTO
    {
        [ProtoMember(1)] public DeviceTypeDTO[] DeviceTypes;
        [ProtoMember(2)] public DeviceDTO[] Devices;
        [ProtoMember(3)] public EdgeDTO[] Edges;
    }
}
