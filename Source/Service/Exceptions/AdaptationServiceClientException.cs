using System;

namespace Service.Exceptions
{
    public class AdaptationServiceClientException : ApplicationException
    {
        public AdaptationServiceClientException()
        {

        }

        public AdaptationServiceClientException(string message) : base(message)
        {
        }
    }
}