using Service.Configuration;
using System;
using System.IO;
using System.Text;

namespace Service.ErrorReport
{
    public class HtmlErrorReportGenerator : IErrorReportGenerator
    {
        private const string DefaultMessage = "The archive contained files which does not comply with the current policy";

        private string _report;

        public HtmlErrorReportGenerator(IArchiveProcessorConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            using(var reader = File.OpenText("Templates/ArchiveErrorReportTemplate.html"))
            {
                _report = reader.ReadToEnd();

                var builder = new StringBuilder(_report);

                builder.Replace("{{MESSAGE}}", config.ArchiveErrorReportMessage ?? DefaultMessage);

                _report = builder.ToString();
            }
        }

        public string AddIdToReport(string fileId)
        {
            var builder = new StringBuilder(_report);

            builder.Replace("<ul>", $"<ul><li>{fileId}</li>");

            _report = builder.ToString();

            return _report;
        }
    }
}
