namespace ReceiverService.Entities
{
    public class Root
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Timestamp { get; set; }
        public string UserId { get; set; }
        public string DeviceId { get; set; }

        public Root(string type, string id, string timestamp, string userId, string deviceId)
        {
            Type = type;
            Id = id;
            Timestamp = timestamp;
            UserId = userId;
            DeviceId = deviceId;
        }

        public Root()
        {
            
        }
    }
}