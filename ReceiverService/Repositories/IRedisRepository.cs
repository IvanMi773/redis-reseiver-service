namespace ReceiverService.Repositories
{
    public interface IRedisRepository
    {
        public void PushStringToList(string listName, string str);
        public string PopStringFromList(string listName);
    }
}