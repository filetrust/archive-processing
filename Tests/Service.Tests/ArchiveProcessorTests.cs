using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Service.Configuration;
using Service.Messaging;
using System;
using System.Threading.Tasks;

namespace Service.Tests
{
    [TestClass]
    public class ArchiveProcessorTests
    {
        public class ProcessMethod : ArchiveProcessorTests
        {
            private Mock<IAdaptationOutcomeSender> _mockAdaptationOutcomeSender;
            private Mock<IAdaptationRequestSender> _mockAdaptationRequestSender;
            private Mock<IFileManager> _mockFileManager;
            private Mock<IArchiveManager> _mockArchiveManager;
            private Mock<IArchiveProcessorConfig> _mockConfig;
            private Mock<ILogger<ArchiveProcessor>> _mockLogger;

            private ArchiveProcessor _archiveProcessor;

            [SetUp]
            public void SetUp()
            {
                _mockAdaptationOutcomeSender = new Mock<IAdaptationOutcomeSender>();
                _mockAdaptationRequestSender = new Mock<IAdaptationRequestSender>();
                _mockFileManager = new Mock<IFileManager>();
                _mockArchiveManager = new Mock<IArchiveManager>();
                _mockConfig = new Mock<IArchiveProcessorConfig>();
                _mockLogger = new Mock<ILogger<ArchiveProcessor>>();

                _mockConfig.SetupGet(s => s.ProcessingTimeoutDuration).Returns(TimeSpan.FromSeconds(1));


                _archiveProcessor = new ArchiveProcessor(
                    _mockAdaptationOutcomeSender.Object,
                    _mockAdaptationRequestSender.Object,
                    _mockFileManager.Object,
                    _mockArchiveManager.Object,
                    _mockConfig.Object,
                    _mockLogger.Object);
            }

            [Test]
            public void ErrorIsSent_And_RebuiltFolderIsCleaner_When_FileDoesNotExists()
            {
                // Arrange
                const string expectedReplyTo = "reply-to-me";
                const string expectedInput = "Folder-To-Process";
                const string expectedOutput = "Folder-To-Place";
                var expectedFileId = Guid.NewGuid().ToString();

                _mockConfig.SetupGet(s => s.ArchiveFileId).Returns(expectedFileId);
                _mockConfig.SetupGet(s => s.ReplyTo).Returns(expectedReplyTo);
                _mockConfig.SetupGet(s => s.InputPath).Returns(expectedInput);
                _mockConfig.SetupGet(s => s.OutputPath).Returns(expectedOutput);
                _mockFileManager.Setup(s => s.FileExists(It.Is<string>(s => s == expectedInput))).Returns(false);
                _mockFileManager.Setup(s => s.FileExists(It.Is<string>(s => s == expectedOutput))).Returns(true);

                // Act
                _archiveProcessor.Process();

                // Assert
                _mockFileManager.Verify(s => s.DeleteFile(
                    It.Is<string>(path => path == expectedOutput)));

                _mockAdaptationOutcomeSender.Verify(s => s.Send(
                    It.Is<string>(status => status == FileOutcome.Error),
                    It.Is<string>(fileId => fileId == expectedFileId),
                    It.Is<string>(replyTo => replyTo == expectedReplyTo)));
            }

            [Test]
            public void FilesAreExtractedAndRepacked_And_ReplaceIsSent_When_FileExists()
            {
                // Arrange
                const string expectedReplyTo = "reply-to-me";
                const string expectedInput = "Folder-To-Process";
                const string expectedOutput = "Folder-To-Place";
                
                var expectedTmpFolder = $"{expectedInput}_tmp";
                var expectedFileId = Guid.NewGuid().ToString();

                _mockConfig.SetupGet(s => s.ArchiveFileId).Returns(expectedFileId);
                _mockConfig.SetupGet(s => s.ReplyTo).Returns(expectedReplyTo);
                _mockConfig.SetupGet(s => s.InputPath).Returns(expectedInput);
                _mockConfig.SetupGet(s => s.OutputPath).Returns(expectedOutput);
                _mockFileManager.Setup(s => s.FileExists(It.IsAny<string>())).Returns(true);

                // Act
                _archiveProcessor.Process();

                // Assert
                _mockArchiveManager.Verify(s => s.ExtractArchive(
                    It.Is<string>(archive => archive == expectedInput),
                    It.Is<string>(output => output == expectedTmpFolder)));

                _mockArchiveManager.Verify(s => s.CreateArchive(
                    It.Is<string>(input => input == expectedTmpFolder),
                    It.Is<string>(archive => archive == expectedOutput)));

                _mockAdaptationOutcomeSender.Verify(s => s.Send(
                    It.Is<string>(status => status == FileOutcome.Replace),
                    It.Is<string>(fileId => fileId == expectedFileId),
                    It.Is<string>(replyTo => replyTo == expectedReplyTo)));
            }

            [Test]
            public void OriginalTmpFolder_Is_Cleared_On_Success()
            {
                // Arrange
                const string expectedReplyTo = "reply-to-me";
                const string expectedInput = "Folder-To-Process";
                const string expectedOutput = "Folder-To-Place";

                var expectedTmpFolder = $"{expectedInput}_tmp";
                var expectedFileId = Guid.NewGuid().ToString();

                _mockConfig.SetupGet(s => s.ArchiveFileId).Returns(expectedFileId);
                _mockConfig.SetupGet(s => s.ReplyTo).Returns(expectedReplyTo);
                _mockConfig.SetupGet(s => s.InputPath).Returns(expectedInput);
                _mockConfig.SetupGet(s => s.OutputPath).Returns(expectedOutput);
                _mockFileManager.Setup(s => s.FileExists(It.IsAny<string>())).Returns(true);
                _mockFileManager.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);

                // Act
                _archiveProcessor.Process();

                // Assert
                _mockFileManager.Verify(m => m.DeleteDirectory(It.Is<string>(input => input == expectedTmpFolder)), Times.Once, "Original Temp Folder should be cleared in event of long running process");

            }

            [Test]
            public void Long_Running_Process_Should_Clear_Output_Store()
            {
                _mockArchiveManager.Setup(s => s.ExtractArchive(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback((string t, string u) => Task.Delay(TimeSpan.FromMinutes(10)).Wait());
                _mockFileManager.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);
                _mockFileManager.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);

                _archiveProcessor.Process();

                _mockFileManager.Verify(m => m.DeleteFile(It.IsAny<string>()), Times.Once, "Store should be cleared in event of long running process");
                _mockFileManager.Verify(m => m.DeleteDirectory(It.IsAny<string>()), Times.Once, "Original Temp Folder should be cleared in event of long running process");
            }

            [Test]
            public void Exception_Thrown_In_Process_Should_Clear_Output_Store()
            {
                _mockArchiveManager.Setup(s => s.ExtractArchive(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception());
                _mockFileManager.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);
                _mockFileManager.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);

                _archiveProcessor.Process();

                _mockFileManager.Verify(m => m.DeleteFile(It.IsAny<string>()), Times.Once, "Store should be cleared in event of long running process");
                _mockFileManager.Verify(m => m.DeleteDirectory(It.IsAny<string>()), Times.Once, "Original Temp Folder should be cleared in event of long running process");

            }
        }
    }
}
