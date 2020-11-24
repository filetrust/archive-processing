using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Service.Configuration;
using Service.Enums;
using Service.Interfaces;
using Service.Messaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Tests
{
    [TestClass]
    public class AdaptationResponseConsumerTests
    {
        public class ConsumeResponsesMethod : AdaptationResponseConsumerTests
        {
            private Guid _archiveFileId = Guid.NewGuid();

            private string _outputPath;
            private string _rebuiltPath;
            private string _originalPath;


            private Mock<IAdaptationResponseCollection> _mockCollection;
            private Mock<IArchiveManager> _mockArchiveManager;
            private Mock<IArchiveProcessorConfig> _mockConfig;
            private Mock<ILogger<AdaptationResponseConsumer>> _mockLogger;

            private AdaptationResponseConsumer _adaptationResponseConsumer;

            [SetUp]
            public void SetUp()
            {
                _outputPath = $"/var/target/{_archiveFileId}";
                _rebuiltPath = $"/var/target/{_archiveFileId}_tmp";
                _originalPath = $"/var/source/{_archiveFileId}_tmp";

                _mockCollection = new Mock<IAdaptationResponseCollection>();
                _mockArchiveManager = new Mock<IArchiveManager>();
                _mockConfig = new Mock<IArchiveProcessorConfig>();
                _mockLogger = new Mock<ILogger<AdaptationResponseConsumer>>();

                _mockConfig.SetupGet(s => s.OutputPath).Returns(_outputPath);
                _mockConfig.SetupGet(s => s.ArchiveFileId).Returns(_archiveFileId.ToString());

                _adaptationResponseConsumer = new AdaptationResponseConsumer(
                    _mockCollection.Object,
                    _mockArchiveManager.Object,
                    _mockConfig.Object,
                    _mockLogger.Object);
            }

            [Test]
            public void RebuiltFile_Is_Added_To_Archive_When_Outcome_Is_Replace()
            {
                // Arrange
                var expectedFileId = Guid.NewGuid();
                var expectedFileName = "I AM FILE NAME";
                var expectedSource = $"{_rebuiltPath}/{expectedFileId}";

                var fileMappings = new Dictionary<string, string>()
                {
                    { expectedFileId.ToString(),  expectedFileName}
                };
                
                _mockCollection.SetupSequence(s => s.IsCompleted)
                    .Returns(false)
                    .Returns(true);

                _mockCollection.Setup(s => s.Take(It.IsAny<CancellationToken>())).Returns(new KeyValuePair<Guid, AdaptationOutcome>(expectedFileId, AdaptationOutcome.Replace));

                // Act
                _adaptationResponseConsumer.ConsumeResponses(fileMappings, _rebuiltPath, _originalPath, new CancellationToken()).Wait();

                // Assert
                _mockArchiveManager.Verify(s => s.AddToArchive(
                    It.Is<string>(path => path == _outputPath),
                    It.Is<string>(source => source == expectedSource),
                    It.Is<string>(fileName => fileName == expectedFileName)), Times.Once);
            }

            [Test]
            public void OriginalFile_Is_Added_To_Archive_When_Outcome_Is_Unmodified()
            {
                // Arrange
                var expectedFileId = Guid.NewGuid();
                var expectedFileName = "I AM FILE NAME";
                var expectedSource = $"{_originalPath}/{expectedFileId}";

                var fileMappings = new Dictionary<string, string>()
                {
                    { expectedFileId.ToString(),  expectedFileName}
                };

                _mockCollection.SetupSequence(s => s.IsCompleted)
                    .Returns(false)
                    .Returns(true);

                _mockCollection.Setup(s => s.Take(It.IsAny<CancellationToken>())).Returns(new KeyValuePair<Guid, AdaptationOutcome>(expectedFileId, AdaptationOutcome.Unmodified));

                // Act
                _adaptationResponseConsumer.ConsumeResponses(fileMappings, _rebuiltPath, _originalPath, new CancellationToken()).Wait();

                // Assert
                _mockArchiveManager.Verify(s => s.AddToArchive(
                    It.Is<string>(path => path == _outputPath),
                    It.Is<string>(source => source == expectedSource),
                    It.Is<string>(fileName => fileName == expectedFileName)), Times.Once);
            }

            [Test]
            public void Nothing_Is_Added_To_The_Archive_When_Collection_Is_Complete()
            {
                // Arrange
                var fileId = Guid.NewGuid();
                var fileName = "I AM FILE NAME";

                var fileMappings = new Dictionary<string, string>()
                {
                    { fileId.ToString(),  fileName}
                };

                _mockCollection.SetupSequence(s => s.IsCompleted).Returns(true);

                // Act
                _adaptationResponseConsumer.ConsumeResponses(fileMappings, _rebuiltPath, _originalPath, new CancellationToken()).Wait();

                // Assert
                _mockArchiveManager.Verify(s => s.AddToArchive(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()), Times.Never);
            }

            [Test]
            public void All_Files_Are_Added_From_The_Collection()
            {
                // Arrange
                var expectedFileIdOne = Guid.NewGuid();
                var expectedFileIdTwo = Guid.NewGuid();
                var expectedFileIdThree = Guid.NewGuid();

                var expectedFileNameOne = "I AM FILE NAME ONE";
                var expectedFileNameTwo = "I AM FILE NAME TWO";
                var expectedFileNameThree  = "I AM FILE NAME THREE";

                var expectedSourceOne = $"{_rebuiltPath}/{expectedFileIdOne}";
                var expectedSourceTwo = $"{_rebuiltPath}/{expectedFileIdTwo}";
                var expectedSourceThree = $"{_rebuiltPath}/{expectedFileIdThree}";

                var fileMappings = new Dictionary<string, string>()
                {
                    { expectedFileIdOne.ToString(),  expectedFileNameOne},
                    { expectedFileIdTwo.ToString(),  expectedFileNameTwo},
                    { expectedFileIdThree.ToString(),  expectedFileNameThree}
                };

                _mockCollection.SetupSequence(s => s.IsCompleted)
                    .Returns(false)
                    .Returns(false)
                    .Returns(false)
                    .Returns(true);

                _mockCollection.SetupSequence(s => s.Take(It.IsAny<CancellationToken>()))
                    .Returns(new KeyValuePair<Guid, AdaptationOutcome>(expectedFileIdOne, AdaptationOutcome.Replace))
                    .Returns(new KeyValuePair<Guid, AdaptationOutcome>(expectedFileIdTwo, AdaptationOutcome.Replace))
                    .Returns(new KeyValuePair<Guid, AdaptationOutcome>(expectedFileIdThree, AdaptationOutcome.Replace));

                // Act
                _adaptationResponseConsumer.ConsumeResponses(fileMappings, _rebuiltPath, _originalPath, new CancellationToken()).Wait();

                // Assert
                _mockArchiveManager.Verify(s => s.AddToArchive(
                    It.Is<string>(path => path == _outputPath),
                    It.Is<string>(source => source == expectedSourceOne),
                    It.Is<string>(fileName => fileName == expectedFileNameOne)), Times.Once);

                _mockArchiveManager.Verify(s => s.AddToArchive(
                    It.Is<string>(path => path == _outputPath),
                    It.Is<string>(source => source == expectedSourceTwo),
                    It.Is<string>(fileName => fileName == expectedFileNameTwo)), Times.Once);

                _mockArchiveManager.Verify(s => s.AddToArchive(
                    It.Is<string>(path => path == _outputPath),
                    It.Is<string>(source => source == expectedSourceThree),
                    It.Is<string>(fileName => fileName == expectedFileNameThree)), Times.Once);
            }
        }
    }
}
