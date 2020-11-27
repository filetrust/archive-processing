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
            private Mock<IErrorReportGenerator> _mockErrorReportGenerator;
            private Mock<IFileManager> _mockFileManager;
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
                _mockErrorReportGenerator = new Mock<IErrorReportGenerator>();
                _mockFileManager = new Mock<IFileManager>();
                _mockConfig = new Mock<IArchiveProcessorConfig>();
                _mockLogger = new Mock<ILogger<AdaptationResponseConsumer>>();

                _mockConfig.SetupGet(s => s.OutputPath).Returns(_outputPath);
                _mockConfig.SetupGet(s => s.ArchiveFileId).Returns(_archiveFileId.ToString());

                _adaptationResponseConsumer = new AdaptationResponseConsumer(
                    _mockCollection.Object,
                    _mockArchiveManager.Object,
                    _mockErrorReportGenerator.Object,
                    _mockFileManager.Object,
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

                var fileMappings = new Dictionary<Guid, string>()
                {
                    { expectedFileId,  expectedFileName}
                };
                
                var outcome = new KeyValuePair<Guid, AdaptationOutcome>(expectedFileId, AdaptationOutcome.Replace);

                _mockCollection.Setup(s => s.Take(It.IsAny<CancellationToken>())).Returns(outcome);

                _adaptationResponseConsumer.SetPendingFiles(new List<Guid>(fileMappings.Keys));

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

                var fileMappings = new Dictionary<Guid, string>()
                {
                    { expectedFileId,  expectedFileName}
                };

                var outcome = new KeyValuePair<Guid, AdaptationOutcome>(expectedFileId, AdaptationOutcome.Unmodified);

                _adaptationResponseConsumer.SetPendingFiles(new List<Guid>(fileMappings.Keys));

                _mockCollection.Setup(s => s.Take(It.IsAny<CancellationToken>())).Returns(outcome);

                // Act
                _adaptationResponseConsumer.ConsumeResponses(fileMappings, _rebuiltPath, _originalPath, new CancellationToken()).Wait();

                // Assert
                _mockArchiveManager.Verify(s => s.AddToArchive(
                    It.Is<string>(path => path == _outputPath),
                    It.Is<string>(source => source == expectedSource),
                    It.Is<string>(fileName => fileName == expectedFileName)), Times.Once);
            }

            [Test]
            public void ErrorReport_Is_Generated_And_Added_To_Archive_When_Outcome_Is_Failed()
            {
                // Arrange
                const string ErrorReportFileName = "ErrorReport.html";

                var expectedFileId = Guid.NewGuid();
                var expectedSource = $"{_rebuiltPath}/{ErrorReportFileName}";

                var fileMappings = new Dictionary<Guid, string>()
                {
                    { expectedFileId,  "DoesntMatter"}
                };

                _mockErrorReportGenerator.Setup(s => s.AddIdToReport(
                    It.Is<string>(id => id == $"{_archiveFileId}/{expectedFileId}")))
                    .Returns("Error Report");

                _adaptationResponseConsumer.SetPendingFiles(new List<Guid>(fileMappings.Keys));

                var outcome = new KeyValuePair<Guid, AdaptationOutcome>(expectedFileId, AdaptationOutcome.Failed);

                _mockCollection.Setup(s => s.Take(It.IsAny<CancellationToken>())).Returns(outcome);

                // Act
                _adaptationResponseConsumer.ConsumeResponses(fileMappings, _rebuiltPath, _originalPath, new CancellationToken()).Wait();

                // Assert
                _mockArchiveManager.Verify(s => s.AddToArchive(
                    It.Is<string>(path => path == _outputPath),
                    It.Is<string>(source => source == expectedSource),
                    It.Is<string>(fileName => fileName == ErrorReportFileName)), Times.Once);
            }

            [Test]
            public void Nothing_Is_Added_To_The_Archive_When_Pending_Is_Empty()
            {
                // Arrange
                var fileId = Guid.NewGuid();
                var fileName = "I AM FILE NAME";

                var fileMappings = new Dictionary<Guid, string>()
                {
                    { fileId,  fileName}
                };

                // Act
                _adaptationResponseConsumer.ConsumeResponses(fileMappings, _rebuiltPath, _originalPath, new CancellationToken()).Wait();

                // Assert
                _mockArchiveManager.Verify(s => s.AddToArchive(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()), Times.Never);
            }

            public delegate void CallbackDelegate(out KeyValuePair<Guid, AdaptationOutcome> outcome, CancellationToken token);

            [Test]
            public void All_Files_Are_Added_From_The_Collection()
            {
                // Arrange
                var expectedFileIdOne = Guid.NewGuid();
                var expectedFileIdTwo = Guid.NewGuid();
                var expectedFileIdThree = Guid.NewGuid();

                var expectedFileNameOne = "I AM FILE NAME ONE";
                var expectedFileNameTwo = "I AM FILE NAME TWO";
                var expectedFileNameThree = "I AM FILE NAME THREE";

                var expectedSourceOne = $"{_rebuiltPath}/{expectedFileIdOne}";
                var expectedSourceTwo = $"{_rebuiltPath}/{expectedFileIdTwo}";
                var expectedSourceThree = $"{_rebuiltPath}/{expectedFileIdThree}";

                var fileMappings = new Dictionary<Guid, string>()
                {
                    { expectedFileIdOne,  expectedFileNameOne},
                    { expectedFileIdTwo,  expectedFileNameTwo},
                    { expectedFileIdThree,  expectedFileNameThree}
                };

                var collection1 = new KeyValuePair<Guid, AdaptationOutcome>(expectedFileIdOne, AdaptationOutcome.Replace);
                var collection2 = new KeyValuePair<Guid, AdaptationOutcome>(expectedFileIdTwo, AdaptationOutcome.Replace);
                var collection3 = new KeyValuePair<Guid, AdaptationOutcome>(expectedFileIdThree, AdaptationOutcome.Replace);

                _mockCollection.SetupSequence(s => s.Take(It.IsAny<CancellationToken>()))
                    .Returns(collection1)
                    .Returns(collection2)
                    .Returns(collection3);

                _adaptationResponseConsumer.SetPendingFiles(new List<Guid>(fileMappings.Keys));

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
