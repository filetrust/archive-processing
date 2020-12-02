using Service.Configuration;
using System;
using System.IO;
using System.Text;

namespace Service.ErrorReport
{
    public class HtmlPasswordProtectedErrorReportGenerator : IPasswordProtectedReportGenerator
    {
        private const string DefaultMessage = "The archive was password protected and unable to be analysed";

        private readonly IArchiveProcessorConfig _config;

        public HtmlPasswordProtectedErrorReportGenerator(IArchiveProcessorConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public string CreateReport(string fileId)
        {
            using (var reader = File.OpenText("Templates/PasswordProtectedErrorReportTemplate.html"))
            {
                var report = reader.ReadToEnd();

                var builder = new StringBuilder(report);

                builder.Replace("{{MESSAGE}}", _config.ArchivePasswordProtectedReportMessage ?? DefaultMessage);
                builder.Replace("{{FILE_ID}}", fileId);

                return builder.ToString();
            }
        }
    }
}
