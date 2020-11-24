using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Service.Enums;
using Service.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Tests.MessagingTests
{
    public class AdaptationOutcomeProcessorTests
    {
        public class ProcessMethod : AdaptationOutcomeProcessorTests
        {
            private Mock<ILogger<AdaptationOutcomeProcessor>> _mockLogger;

            private AdaptationOutcomeProcessor _adaptationOutcomeProcessor;

            [SetUp]
            public void SetUp()
            {
                _mockLogger = new Mock<ILogger<AdaptationOutcomeProcessor>>();

                _adaptationOutcomeProcessor = new AdaptationOutcomeProcessor(_mockLogger.Object);
            }

            [Test]
            public void Missing_FileId_Returns_Error_Outcome_And_Empty_Guid()
            {
                // Arrange
                var headers = new Dictionary<string, object>();

                // Act
                var outcome = _adaptationOutcomeProcessor.Process(headers);

                // Assert
                Assert.That(outcome.Key, Is.EqualTo(Guid.Empty));
                Assert.That(outcome.Value, Is.EqualTo(AdaptationOutcome.Error));
            }

            [Test]
            public void Null_FileId_Returns_Error_Outcome_And_Empty_Guid()
            {
                // Arrange
                var headers = new Dictionary<string, object>()
                {
                    { "file-id", new byte[] { } }
                };

                // Act
                var outcome = _adaptationOutcomeProcessor.Process(headers);

                // Assert
                Assert.That(outcome.Key, Is.EqualTo(Guid.Empty));
                Assert.That(outcome.Value, Is.EqualTo(AdaptationOutcome.Error));
            }

            [Test]
            public void Invalid_FileId_Returns_Error_Outcome_And_Empty_Guid()
            {
                // Arrange
                var headers = new Dictionary<string, object>()
                {
                    { "file-id", Encoding.UTF8.GetBytes("I AM NOT A GUID") }
                };

                // Act
                var outcome = _adaptationOutcomeProcessor.Process(headers);

                // Assert
                Assert.That(outcome.Key, Is.EqualTo(Guid.Empty));
                Assert.That(outcome.Value, Is.EqualTo(AdaptationOutcome.Error));
            }


            [Test]
            public void Missing_FileOutcome_Returns_Error_Outcome_And_Empty_Guid()
            {
                // Arrange
                var headers = new Dictionary<string, object>()
                {
                    { "file-id", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) }
                };

                // Act
                var outcome = _adaptationOutcomeProcessor.Process(headers);

                // Assert
                Assert.That(outcome.Key, Is.EqualTo(Guid.Empty));
                Assert.That(outcome.Value, Is.EqualTo(AdaptationOutcome.Error));
            }

            [Test]
            public void Invalid_FileOutcome_Returns_Error_Outcome_And_Empty_Guid()
            {
                // Arrange
                var headers = new Dictionary<string, object>()
                {
                    { "file-id", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) },
                    { "file-outcome", Encoding.UTF8.GetBytes("I AM NOT A VALID OUTCOME") }
                };

                // Act
                var outcome = _adaptationOutcomeProcessor.Process(headers);

                // Assert
                Assert.That(outcome.Key, Is.EqualTo(Guid.Empty));
                Assert.That(outcome.Value, Is.EqualTo(AdaptationOutcome.Error));
            }

            [TestCase(AdaptationOutcome.Error)]
            [TestCase(AdaptationOutcome.Failed)]
            [TestCase(AdaptationOutcome.Replace)]
            [TestCase(AdaptationOutcome.Unmodified)]
            public void Outcome_And_FileId_Is_Returned_When_Message_Is_Valid(AdaptationOutcome expectedOutcome)
            {
                // Arrange
                var expectedFileId = Guid.NewGuid();

                var headers = new Dictionary<string, object>()
                {
                    { "file-id", Encoding.UTF8.GetBytes(expectedFileId.ToString()) },
                    { "file-outcome", Encoding.UTF8.GetBytes(expectedOutcome.ToString()) }
                };

                // Act
                var outcome = _adaptationOutcomeProcessor.Process(headers);

                // Assert
                Assert.That(outcome.Key, Is.EqualTo(expectedFileId));
                Assert.That(outcome.Value, Is.EqualTo(expectedOutcome));
            }
        }
    }
}
