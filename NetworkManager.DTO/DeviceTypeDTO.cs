using ProtoBuf;

namespace NetworkManager.DTO
{
    [ProtoContract]
    public struct DeviceTypeDTO
    {
        [ProtoMember(1)] public ulong Id;
        [ProtoMember(2)] public string Name;
        [ProtoMember(3)] public bool IsSwitchable;
        [ProtoMember(4)] public bool IsGenerator;
        [ProtoMember(5)] public bool IsServicePoint;
    }
}
