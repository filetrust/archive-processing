using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Service.Configuration;
using Service.Messaging;
using System;

namespace Service.Tests
{
    [TestClass]
    public class ArchiveProcessorTests
    {
        public class ProcessMethod : ArchiveProcessorTests
        {
            private Mock<IAdaptationOutcomeSender> _mockAdaptationOutcomeSender;
            private Mock<IFileManager> _mockFileManager;
            private Mock<IArchiveProcessorConfig> _mockConfig;

            private ArchiveProcessor _archiveProcessor;

            [SetUp]
            public void SetUp()
            {
                _mockAdaptationOutcomeSender = new Mock<IAdaptationOutcomeSender>();
                _mockFileManager = new Mock<IFileManager>();
                _mockConfig = new Mock<IArchiveProcessorConfig>();

                _mockConfig.SetupGet(s => s.ProcessingTimeoutDuration).Returns(TimeSpan.FromSeconds(121));


                _archiveProcessor = new ArchiveProcessor(
                    _mockAdaptationOutcomeSender.Object,
                    _mockFileManager.Object,
                    _mockConfig.Object);
            }

            [Test]
            public void ReplaceIsSent_And_FileIsCopied_When_FileExists()
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
                _mockFileManager.Setup(s => s.FileExists(It.IsAny<string>())).Returns(true);

                // Act
                _archiveProcessor.Process();

                // Assert
                _mockFileManager.Verify(s => s.CopyFile(
                    It.Is<string>(input => input == expectedInput), 
                    It.Is<string>(output => output == expectedOutput)));

                _mockAdaptationOutcomeSender.Verify(s => s.Send(
                    It.Is<string>(status => status == FileOutcome.Replace),
                    It.Is<string>(fileId => fileId == expectedFileId),
                    It.Is<string>(replyTo => replyTo == expectedReplyTo)));
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
        }
    }
}
