using ReceiverService.Entities;

namespace ReceiverService.Mappers
{
    public static class RootToExtendedRootMapper
    {
        public static ExtendedRoot Map(Root root, int price)
        {
            return new ExtendedRoot
            {
                Price = price,
                Id = root?.Id,
                Timestamp = root?.Timestamp,
                Type = root?.Type,
                UserId = root?.UserId,
                DeviceId = root?.DeviceId
            };
        }
    }
}