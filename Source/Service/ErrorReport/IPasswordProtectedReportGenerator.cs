namespace Service.ErrorReport
{
    public interface IPasswordProtectedReportGenerator
    {
        string CreateReport(string fileId);
    }
}
