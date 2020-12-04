using System;

namespace Service.Configuration
{
    public interface IArchiveProcessorConfig
    {
        string ArchiveFileId { get; }
        string ArchiveFileType { get; }
        string InputPath { get; }
        string OutputPath { get; }
        string ReplyTo { get; }
        TimeSpan ProcessingTimeoutDuration { get; }
        string MessageBrokerUser { get; }
        string MessageBrokerPassword { get; }
        string AdaptationRequestQueueHostname { get; }
        int AdaptationRequestQueuePort { get; }
        string ArchiveErrorReportMessage { get; }
        string ArchivePasswordProtectedReportMessage { get; }
    }
}