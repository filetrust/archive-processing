using System;
using System.Runtime.Serialization;

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

        protected FileEncryptedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}