using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Service.Configuration;
using Service.Enums;
using Service.ErrorReport;
using Service.Interfaces;
using Service.Messaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Tests
{
    [TestClass]
    public class ArchiveProcessorTests
    {
        public class ProcessMethod : ArchiveProcessorTests
        {
            private Mock<IAdaptationOutcomeSender> _mockAdaptationOutcomeSender;
            private Mock<IFileManager> _mockFileManager;
            private Mock<IArchiveManager> _mockArchiveManager;
            private Mock<IAdaptationResponseProducer> _mockAdaptationResponseProducer;
            private Mock<IAdaptationResponseConsumer> _mockAdaptationResponseConsumer;
            private Mock<IPasswordProtectedReportGenerator> _mockPasswordProtectedReportGenerator;
            private Mock<IArchiveProcessorConfig> _mockConfig;
            private Mock<ILogger<ArchiveProcessor>> _mockLogger;

            private ArchiveProcessor _archiveProcessor;

            [SetUp]
            public void SetUp()
            {
                _mockAdaptationOutcomeSender = new Mock<IAdaptationOutcomeSender>();
                _mockFileManager = new Mock<IFileManager>();
                _mockArchiveManager = new Mock<IArchiveManager>();
                _mockAdaptationResponseProducer = new Mock<IAdaptationResponseProducer>();
                _mockAdaptationResponseConsumer = new Mock<IAdaptationResponseConsumer>();
                _mockPasswordProtectedReportGenerator = new Mock<IPasswordProtectedReportGenerator>();
                _mockConfig = new Mock<IArchiveProcessorConfig>();
                _mockLogger = new Mock<ILogger<ArchiveProcessor>>();

                _mockConfig.SetupGet(s => s.ProcessingTimeoutDuration).Returns(TimeSpan.FromSeconds(1));


                _archiveProcessor = new ArchiveProcessor(
                    _mockAdaptationOutcomeSender.Object,
                    _mockFileManager.Object,
                    _mockArchiveManager.Object,
                    _mockAdaptationResponseProducer.Object,
                    _mockAdaptationResponseConsumer.Object,
                    _mockPasswordProtectedReportGenerator.Object,
                    _mockConfig.Object,
                    _mockLogger.Object);
            }

            [Test]
            public void ErrorIsSent_And_RebuiltFolderIsCleaned_When_FileDoesNotExists()
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
            public void ErrorReport_Is_Created_When_Archive_Is_Password_Protected()
            {
                // Arrange
                const string expectedReplyTo = "reply-to-me";
                const string expectedInput = "Folder-To-Process";
                const string expectedOutput = "Folder-To-Place";

                var expectedFileId = Guid.NewGuid().ToString();

                var expectedReport = "Error Report: Password Protected Archive";
                var expectedReportBytes = Encoding.UTF8.GetBytes(expectedReport);

                _mockConfig.SetupGet(s => s.ArchiveFileId).Returns(expectedFileId);
                _mockConfig.SetupGet(s => s.ReplyTo).Returns(expectedReplyTo);
                _mockConfig.SetupGet(s => s.InputPath).Returns(expectedInput);
                _mockConfig.SetupGet(s => s.OutputPath).Returns(expectedOutput);
                _mockFileManager.Setup(s => s.FileExists(It.IsAny<string>())).Returns(true);
                _mockArchiveManager.Setup(s => s.ExtractArchive(It.IsAny<string>(), It.IsAny<string>())).Returns(new Dictionary<Guid, string>());
                _mockPasswordProtectedReportGenerator.Setup(s => s.CreateReport(It.IsAny<string>())).Returns(expectedReport);

                var respQueue = new Mock<IProducerConsumerCollection<KeyValuePair<Guid, AdaptationOutcome>>>();

                // Act
                _archiveProcessor.Process();

                // Assert
                _mockPasswordProtectedReportGenerator.Verify(s => s.CreateReport(
                    It.Is<string>(id => id == expectedFileId)));

                _mockFileManager.Verify(s => s.WriteFile(
                    It.Is<string>(output => output == expectedOutput),
                    It.Is<byte[]>(report => report.Where((b, i) => b == expectedReportBytes[i]).Count() == expectedReportBytes.Length)));
            }

            [Test]
            public void FilesAreExtracted_And_ReplaceIsSent_When_FileExists()
            {
                // Arrange
                const string expectedReplyTo = "reply-to-me";
                const string expectedInput = "Folder-To-Process";
                const string expectedOutput = "Folder-To-Place";
                
                var expectedOriginalTmpFolder = $"{expectedInput}_tmp";
                var expectedRebuiltTmpFolder = $"{expectedOutput}_tmp";
                var expectedFileId = Guid.NewGuid().ToString();

                var fileOneId = Guid.NewGuid();
                var fileTwoId = Guid.NewGuid();
                var fileThreeId = Guid.NewGuid();

                var filePathOne = $"{expectedOriginalTmpFolder}/{fileOneId}";
                var filePathTwo = $"{expectedOriginalTmpFolder}/{fileTwoId}";
                var filePathThree = $"{expectedOriginalTmpFolder}/{fileThreeId}";

                var files = new string[] { filePathOne, filePathTwo, filePathThree };

                var fileMappings = new Dictionary<Guid, string>()
                {
                    { fileOneId, "FileOne" },
                    { fileTwoId, "FileTwo" },
                    { fileThreeId, "FileThree" },
                };

                _mockConfig.SetupGet(s => s.ArchiveFileId).Returns(expectedFileId);
                _mockConfig.SetupGet(s => s.ReplyTo).Returns(expectedReplyTo);
                _mockConfig.SetupGet(s => s.InputPath).Returns(expectedInput);
                _mockConfig.SetupGet(s => s.OutputPath).Returns(expectedOutput);
                _mockFileManager.Setup(s => s.FileExists(It.IsAny<string>())).Returns(true);
                _mockArchiveManager.Setup(s => s.ExtractArchive(It.IsAny<string>(), It.IsAny<string>())).Returns(fileMappings);

                var respQueue = new Mock<IProducerConsumerCollection<KeyValuePair<Guid, AdaptationOutcome>>>();

                _mockFileManager.Setup(s => s.GetFiles(It.IsAny<string>()))
                    .Returns(files);

                // Act
                _archiveProcessor.Process();

                // Assert
                _mockArchiveManager.Verify(s => s.ExtractArchive(
                    It.Is<string>(archive => archive == expectedInput),
                    It.Is<string>(output => output == expectedOriginalTmpFolder)));

                _mockArchiveManager.Verify(s => s.CreateArchive(
                    It.Is<string>(input => input == expectedRebuiltTmpFolder),
                    It.Is<string>(archive => archive == expectedOutput)));

                _mockAdaptationOutcomeSender.Verify(s => s.Send(
                    It.Is<string>(status => status == FileOutcome.Replace),
                    It.Is<string>(fileId => fileId == expectedFileId),
                    It.Is<string>(replyTo => replyTo == expectedReplyTo)));
            }

            [Test]
            public void Successful_Process_Should_Clear_Temp_Directories()
            {
                // Arrange
                const string expectedReplyTo = "reply-to-me";
                const string expectedInput = "Folder-To-Process";
                const string expectedOutput = "Folder-To-Place";

                var expectedOriginalTmpFolder = $"{expectedInput}_tmp";
                var expectedRebuiltTmpFolder = $"{expectedOutput}_tmp";
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
                _mockFileManager.Verify(m => m.DeleteDirectory(It.Is<string>(input => input == expectedOriginalTmpFolder)), Times.Once, "Original Temp Folder should be cleared on success");
                _mockFileManager.Verify(m => m.DeleteDirectory(It.Is<string>(input => input == expectedRebuiltTmpFolder)), Times.Once, "Rebuilt Temp Folder should be cleared on success");
            }

            [Test]
            public void Long_Running_Process_Should_Clear_Output_Store()
            {
                // Arrange
                const string expectedReplyTo = "reply-to-me";
                const string expectedInput = "Folder-To-Process";
                const string expectedOutput = "Folder-To-Place";

                var expectedOriginalTmpFolder = $"{expectedInput}_tmp";
                var expectedRebuiltTmpFolder = $"{expectedOutput}_tmp";
                var expectedFileId = Guid.NewGuid().ToString();

                _mockConfig.SetupGet(s => s.ArchiveFileId).Returns(expectedFileId);
                _mockConfig.SetupGet(s => s.ReplyTo).Returns(expectedReplyTo);
                _mockConfig.SetupGet(s => s.InputPath).Returns(expectedInput);
                _mockConfig.SetupGet(s => s.OutputPath).Returns(expectedOutput);

                _mockArchiveManager.Setup(s => s.ExtractArchive(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback((string t, string u) => Task.Delay(TimeSpan.FromMinutes(10)).Wait());
                _mockFileManager.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);
                _mockFileManager.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);

                // Act
                _archiveProcessor.Process();

                // Assert
                _mockFileManager.Verify(m => m.DeleteFile(It.Is<string>(s => s == expectedOutput)), Times.Once, "Rebuilt store should be cleared in event of long running process");
                _mockFileManager.Verify(m => m.DeleteDirectory(It.Is<string>(s => s == expectedOriginalTmpFolder)), Times.Once, "Original Temp Folder should be cleared in event of long running process");
                _mockFileManager.Verify(m => m.DeleteDirectory(It.Is<string>(s => s == expectedRebuiltTmpFolder)), Times.Once, "Original Temp Folder should be cleared in event of long running process");
            }

            [Test]
            public void Exception_Thrown_In_Process_Should_Clear_Output_Store()
            {
                // Arrange
                const string expectedReplyTo = "reply-to-me";
                const string expectedInput = "Folder-To-Process";
                const string expectedOutput = "Folder-To-Place";

                var expectedOriginalTmpFolder = $"{expectedInput}_tmp";
                var expectedRebuiltTmpFolder = $"{expectedOutput}_tmp";
                var expectedFileId = Guid.NewGuid().ToString();

                _mockConfig.SetupGet(s => s.ArchiveFileId).Returns(expectedFileId);
                _mockConfig.SetupGet(s => s.ReplyTo).Returns(expectedReplyTo);
                _mockConfig.SetupGet(s => s.InputPath).Returns(expectedInput);
                _mockConfig.SetupGet(s => s.OutputPath).Returns(expectedOutput);

                _mockArchiveManager.Setup(s => s.ExtractArchive(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception());
                _mockFileManager.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);
                _mockFileManager.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);

                // Act
                _archiveProcessor.Process();

                // Assert
                _mockFileManager.Verify(m => m.DeleteFile(It.Is<string>(s => s == expectedOutput)), Times.Once, "Rebuilt store should be cleared in event of an exception being thrown");
                _mockFileManager.Verify(m => m.DeleteDirectory(It.Is<string>(s => s == expectedOriginalTmpFolder)), Times.Once, "Original Temp Folder should be cleared in event of an exception being thrown");
                _mockFileManager.Verify(m => m.DeleteDirectory(It.Is<string>(s => s == expectedRebuiltTmpFolder)), Times.Once, "Original Temp Folder should be cleared in event of an exception being thrown");
            }
        }
    }
}
