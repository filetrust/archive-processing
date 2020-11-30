using NUnit.Framework;
using Service.ErrorReport;
using System;

namespace Service.Tests
{
    public class HtmlErrorReportGeneratorTests
    {
        public class AddIdToReportTests : HtmlErrorReportGeneratorTests
        {
            [Test]
            public void Id_Is_Added_To_The_Report_Template()
            {
                // Arrange
                var expectedId = Guid.NewGuid().ToString();

                var reportGenerator = new HtmlErrorReportGenerator();

                // Act
                var result = reportGenerator.AddIdToReport(expectedId);

                // Assert
                Assert.That(result, Contains.Substring(expectedId));
            }

            [Test]
            public void Multiple_Ids_Are_Added_To_The_Report_Template()
            {
                // Arrange
                var expectedIdOne = Guid.NewGuid().ToString();
                var expectedIdTwo = Guid.NewGuid().ToString();
                var expectedIdThree = Guid.NewGuid().ToString();

                var reportGenerator = new HtmlErrorReportGenerator();

                // Act
                var result = reportGenerator.AddIdToReport(expectedIdOne);
                result = reportGenerator.AddIdToReport(expectedIdTwo);
                result = reportGenerator.AddIdToReport(expectedIdThree);

                // Assert
                Assert.That(result, Contains.Substring(expectedIdOne));
                Assert.That(result, Contains.Substring(expectedIdTwo));
                Assert.That(result, Contains.Substring(expectedIdThree));
            }
        }
    }
}
