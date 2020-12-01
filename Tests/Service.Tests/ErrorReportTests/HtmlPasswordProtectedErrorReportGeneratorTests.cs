using NUnit.Framework;
using Service.ErrorReport;
using System;
using Moq;
using Service.Configuration;

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
                var config = new Mock<IArchiveProcessorConfig>();

                var expectedId = Guid.NewGuid().ToString();

                var reportGenerator = new HtmlPasswordProtectedErrorReportGenerator(config.Object);

                // Act
                var result = reportGenerator.CreateReport(expectedId);

                // Assert
                Assert.That(result, Contains.Substring(expectedId));
            }

            [Test]
            public void Custom_Error_Is_Added_To_The_Report_Template()
            {
                // Arrange
                var config = new Mock<IArchiveProcessorConfig>();

                var expectedId = Guid.NewGuid().ToString();
                var expectedErrorMessage = "Error Should Be Added To Report";

                config.SetupGet(s => s.ArchivePasswordProtectedReportMessage).Returns(expectedErrorMessage);

                var reportGenerator = new HtmlPasswordProtectedErrorReportGenerator(config.Object);

                // Act
                var result = reportGenerator.CreateReport(expectedId);

                // Assert
                Assert.That(result, Contains.Substring(expectedErrorMessage));
            }

            [Test]
            public void Default_Error_Is_Added_To_The_Report_Template_If_Config_Is_Null()
            {
                // Arrange
                var config = new Mock<IArchiveProcessorConfig>();

                var expectedId = Guid.NewGuid().ToString();
                var defaultError = "The archive was password protected and unable to be analysed";

                config.SetupGet(s => s.ArchivePasswordProtectedReportMessage).Returns((string)null);

                var reportGenerator = new HtmlPasswordProtectedErrorReportGenerator(config.Object);

                // Act
                var result = reportGenerator.CreateReport(expectedId);

                // Assert
                Assert.That(result, Contains.Substring(defaultError));
            }
        }
    }
}
