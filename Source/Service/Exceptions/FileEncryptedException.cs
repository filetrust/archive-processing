using System;

namespace Service.Exceptions
{
    [Serializable]
    public class FileEncryptedException : ApplicationException
    {
        public FileEncryptedException()
        {

        }

        public FileEncryptedException(string message) : base(message)
        {
        }
    }
}