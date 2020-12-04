using System;

namespace Service.Exceptions
{
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