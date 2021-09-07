namespace ReceiverService.Entities
{
    public class ExtendedRoot : Root
    {
        public int Price { get; set; }

        public ExtendedRoot(string type, string id, string timestamp, string userId, string deviceId, int price)
            : base(type, id, timestamp, userId, deviceId)
        {
            Price = price;
        }

        public ExtendedRoot()
        {
        }
    }
}