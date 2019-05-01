using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Halcyon.HAL;
using Halcyon.Web.HAL;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SurveyWebApplication.Helpers;
using SurveyWebApplication.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace SurveyWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly IJsonFileReaderWriter _jsonFileReaderWriter;
        private List<Candidate> DatabaseRecords { get; set; }

        public SurveyController(IJsonFileReaderWriter jsonFileReaderWriter)
        {
            _jsonFileReaderWriter = jsonFileReaderWriter;
            DatabaseRecords = new List<Candidate>();
        }

        /// <summary>
        /// Record the survey.
        /// It return the data always in HAL-JSON format (see http://stateless.co/hal_specification.html).
        /// </summary>
        /// <param name="candidate">Fill the survey details </param>
        [HttpPost]
        [Route("Record")]
        [Produces("application/hal+json", "application/json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "An unexpected error occurs", typeof(Error))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "The Request is wrong", typeof(Error))]
        [SwaggerResponse((int)HttpStatusCode.OK, null, typeof(IEnumerable<Candidate>))]
        public IActionResult RecordSurvey([Required] [FromBody] Candidate candidate)
        {
            try
            {
                var response = new HALResponse(null);
                if (candidate == null)
                {
                    response.AddSelfLinkIfNotExists(Request);
                    response.AddEmbeddedCollection("_errors",
                        Error.CreateError(ErrorMessage.MissingCandidateInformation, HttpStatusCode.BadRequest,
                            (int)ErrorCode.MissingCandidateInformation));
                    return BadRequest(response);
                }

                // Persistent Database not added.
                UpdateToPersistentDatabase(candidate);

                return Ok(response);
            }
            catch (Exception exception)
            {

                var response = new HALResponse(null);
                response.AddEmbeddedCollection("_errors", CreateErrorResponse(exception.Message, HttpStatusCode.InternalServerError, ErrorCode.UnknownError));
                Log.Error(exception, $"Exception inside method {nameof(RecordSurvey)}");
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        public static IEnumerable<Error> CreateErrorResponse(string exceptionMessage, HttpStatusCode httpStatusCode, ErrorCode errorCode)
        {
            return Error.CreateError(exceptionMessage, httpStatusCode, (int)errorCode);
        }


        private void UpdateToPersistentDatabase(Candidate candidate)
        {
            try
            {
                _jsonFileReaderWriter.WriteJson(candidate);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Return the analysis of survey.
        /// It return the data always in HAL-JSON format (see http://stateless.co/hal_specification.html).
        /// </summary>
        [HttpGet]
        [Route("Analyse")]
        [Produces("application/hal+json", "application/json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "An unexpected error occurs", typeof(Error))]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "No survey records found")]
        [SwaggerResponse((int)HttpStatusCode.OK, null, typeof(IEnumerable<Candidate>))]
        public IActionResult Analyse()
        {
            try
            {
                var response = new HALResponse(null);
                var surveyRecords = _jsonFileReaderWriter.ReadJson();

                if (surveyRecords.Count == 0)
                {
                    return NoContent();
                }

                response.AddEmbeddedCollection("candidates", surveyRecords);
                response.AddSelfLinkIfNotExists(Request);

                return Ok(response);
            }
            catch (Exception exception)
            {

                var response = new HALResponse(null);
                response.AddEmbeddedCollection("_errors", CreateErrorResponse(exception.Message, HttpStatusCode.InternalServerError, ErrorCode.UnknownError));
                Log.Error(exception, $"Exception inside method {nameof(Analyse)}");
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

    }
}
