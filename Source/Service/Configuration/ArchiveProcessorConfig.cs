using System;

namespace Service.Configuration
{
    public class ArchiveProcessorConfig : IArchiveProcessorConfig
    {
        public string ArchiveFileId { get; set; }
        public string ArchiveFileType { get; set; }
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public string ReplyTo { get; set; }
        public TimeSpan ProcessingTimeoutDuration { get; set; }
        public string MessageBrokerUser { get; set; }
        public string MessageBrokerPassword { get; set; }
        public string AdaptationRequestQueueHostname { get; set; }
        public int AdaptationRequestQueuePort { get; set; }
        public string ArchiveErrorReportMessage { get; set; }
        public string ArchivePasswordProtectedReportMessage { get; set; }
    }
}