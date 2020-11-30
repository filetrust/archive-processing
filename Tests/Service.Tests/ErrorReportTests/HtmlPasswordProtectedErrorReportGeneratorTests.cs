using NUnit.Framework;
using Service.ErrorReport;
using System;

namespace Service.Tests
{
    public class HtmlPasswordProtectedErrorReportGeneratorTests
    {
        public class AddIdToReportTests : HtmlPasswordProtectedErrorReportGeneratorTests
        {
            [Test]
            public void Id_Is_Added_To_The_Report_Template()
            {
                // Arrange
                var expectedId = Guid.NewGuid().ToString();

                var reportGenerator = new HtmlPasswordProtectedErrorReportGenerator();

                // Act
                var result = reportGenerator.CreateReport(expectedId);

                // Assert
                Assert.That(result, Contains.Substring(expectedId));
            }
        }
    }
}
