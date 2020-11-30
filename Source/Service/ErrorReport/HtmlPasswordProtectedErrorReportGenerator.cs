using System.IO;
using System.Text;

namespace Service.ErrorReport
{
    public class HtmlPasswordProtectedErrorReportGenerator : IPasswordProtectedReportGenerator
    {
        public string CreateReport(string fileId)
        {
            using (var reader = File.OpenText("Templates/PasswordProtectedErrorReportTemplate.html"))
            {
                var report = reader.ReadToEnd();

                var builder = new StringBuilder(report);

                builder.Replace("{{FILE_ID}}", $"{fileId}");

                return builder.ToString();
            }
        }
    }
}
