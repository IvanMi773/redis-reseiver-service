namespace ReceiverService.Entities
{
    public class ExtendedRoot
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Timestamp { get; set; }
        public string UserId { get; set; }
        public string DeviceId { get; set; }
        public int Price { get; set; }

        public ExtendedRoot(string type, string id, string timestamp, string userId, string deviceId, int price)
        {
            Type = type;
            Id = id;
            Timestamp = timestamp;
            UserId = userId;
            DeviceId = deviceId;
            Price = price;
        }

        public ExtendedRoot()
        {
            
        }
    }
}