using System.IO;
using System.Text;

namespace Service.ErrorReport
{
    public class HtmlErrorReportGenerator : IErrorReportGenerator
    {
        private string _report;

        public string ErrorReport { get; set; }
        
        public HtmlErrorReportGenerator()
        {
            using(var reader = File.OpenText("Templates/ArchiveErrorReportTemplate.html"))
            {
                _report = reader.ReadToEnd();
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
