namespace NetworkManager.DataTransfer
{
    /// <summary>
    /// Serializable data transfer object for a device
    /// </summary>
    public struct DeviceDTO
    {
        public ulong Id;
        public ulong TypeId;
        public bool CanConduct;
        public bool IsEnergized;
    }
}
