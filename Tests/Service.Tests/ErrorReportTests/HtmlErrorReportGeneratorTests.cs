using NUnit.Framework;
using Service.Configuration;
using Service.ErrorReport;
using System;
using Moq;

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
                var config = new Mock<IArchiveProcessorConfig>();

                var expectedId = Guid.NewGuid().ToString();

                var reportGenerator = new HtmlErrorReportGenerator(config.Object);

                // Act
                var result = reportGenerator.AddIdToReport(expectedId);

                // Assert
                Assert.That(result, Contains.Substring(expectedId));
            }

            [Test]
            public void Multiple_Ids_Are_Added_To_The_Report_Template()
            {
                // Arrange
                var config = new Mock<IArchiveProcessorConfig>();

                var expectedIdOne = Guid.NewGuid().ToString();
                var expectedIdTwo = Guid.NewGuid().ToString();
                var expectedIdThree = Guid.NewGuid().ToString();

                var reportGenerator = new HtmlErrorReportGenerator(config.Object);

                // Act
                var result = reportGenerator.AddIdToReport(expectedIdOne);
                result = reportGenerator.AddIdToReport(expectedIdTwo);
                result = reportGenerator.AddIdToReport(expectedIdThree);

                // Assert
                Assert.That(result, Contains.Substring(expectedIdOne));
                Assert.That(result, Contains.Substring(expectedIdTwo));
                Assert.That(result, Contains.Substring(expectedIdThree));
            }

            [Test]
            public void Custom_Error_Is_Added_To_The_Report_Template()
            {
                // Arrange
                var config = new Mock<IArchiveProcessorConfig>();
                var expectedMessage = "Expected Error Message";
                var fileId = Guid.NewGuid().ToString();

                config.SetupGet(s => s.ArchiveErrorReportMessage).Returns(expectedMessage);

                var reportGenerator = new HtmlErrorReportGenerator(config.Object);

                // Act
                var result = reportGenerator.AddIdToReport(fileId);

                // Assert
                Assert.That(result, Contains.Substring(expectedMessage));
            }

            [Test]
            public void Default_Error_Is_Added_To_The_Report_Template_If_Config_Is_Null()
            {
                // Arrange
                var config = new Mock<IArchiveProcessorConfig>();
                var defaultErrorMessage = "The archive contained files which does not comply with the current policy";
                var fileId = Guid.NewGuid().ToString();

                config.SetupGet(s => s.ArchiveErrorReportMessage).Returns((string)null);

                var reportGenerator = new HtmlErrorReportGenerator(config.Object);

                // Act
                var result = reportGenerator.AddIdToReport(fileId);

                // Assert
                Assert.That(result, Contains.Substring(defaultErrorMessage));
            }
        }
    }
}
