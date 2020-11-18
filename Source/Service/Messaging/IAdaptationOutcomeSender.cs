namespace Service.Messaging
{
    public interface IAdaptationOutcomeSender
    {
        void Send(string status, string fileId, string replyTo);
    }
}
