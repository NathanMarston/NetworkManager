using ProtoBuf;

namespace NetworkManager.DTO
{
    [ProtoContract]
    public struct DeviceDTO
    {
        [ProtoMember(1)] public ulong Id;
        [ProtoMember(2)] public ulong DeviceTypeId;
        [ProtoMember(3)] public bool CanConduct;
        [ProtoMember(4)] public bool IsEnergized;
    }
}
