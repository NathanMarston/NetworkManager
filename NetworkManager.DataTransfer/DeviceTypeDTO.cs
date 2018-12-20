namespace NetworkManager.DataTransfer
{
    /// <summary>
    /// Serializable Data Transfer Object for Device Types
    /// </summary>
    public struct DeviceTypeDTO
    {
        public ulong Id;
        public string Name;
        public bool IsSwitchable;
        public bool IsGenerator;
        public bool IsServicePoint;
    }
}
