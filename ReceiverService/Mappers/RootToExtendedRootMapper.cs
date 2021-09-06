using ReceiverService.Entities;

namespace ReceiverService.Mappers
{
    public class RootToExtendedRootMapper
    {
        public ExtendedRoot Map(Root root, int price)
        {
            return new ExtendedRoot(root?.Type, root?.Id, root?.Timestamp, root?.UserId, root?.DeviceId, price);
        }
    }
}