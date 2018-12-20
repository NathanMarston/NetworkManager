using ProtoBuf;

namespace NetworkManager.DTO
{
    [ProtoContract]
    public struct EdgeDTO
    {
        [ProtoMember(1)] public ulong lhs;
        [ProtoMember(2)] public ulong rhs;
    }
}
