using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Enums;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Messaging
{
    public class AdaptationOutcomeProcessor : IResponseProcessor
    {
        private readonly ILogger<AdaptationOutcomeProcessor> _logger;

        public AdaptationOutcomeProcessor(ILogger<AdaptationOutcomeProcessor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public AdaptationOutcome Process(IDictionary<string, object> headers)
        {
            try
            {
                if (!headers.ContainsKey("file-id"))
                    throw new AdaptationServiceClientException("Missing File Id");

                var fileIdString = Encoding.UTF8.GetString((byte[])headers["file-id"]);

                Guid fileId = Guid.Empty;

                if (fileIdString == null || !Guid.TryParse(fileIdString, out fileId))
                    throw new AdaptationServiceClientException($"Error in FileID: { fileIdString ?? "-" }");

                if (!headers.ContainsKey("file-outcome"))
                    throw new AdaptationServiceClientException($"Missing outcome for File Id {fileId}");

                var outcomeString = Encoding.UTF8.GetString((byte[])headers["file-outcome"]);
                AdaptationOutcome outcome = (AdaptationOutcome)Enum.Parse(typeof(AdaptationOutcome), outcomeString, ignoreCase: true);
                return outcome;
            }
            catch (ArgumentException aex)
            {
                _logger.LogError($"Unrecognised enumeration processing adaptation outcome {aex.Message}");
                return AdaptationOutcome.Error;
            }
            catch (JsonReaderException jre)
            {
                _logger.LogError($"Poorly formated adaptation outcome : {jre.Message}");
                return AdaptationOutcome.Error;
            }
            catch (AdaptationServiceClientException asce)
            {
                _logger.LogError($"Poorly formated adaptation outcome : {asce.Message}");
                return AdaptationOutcome.Error;
            }
        }
    }
}