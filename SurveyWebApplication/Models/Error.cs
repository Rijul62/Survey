using System.Collections.Generic;
using System.Net;

namespace SurveyWebApplication.Models
{
    public class Error
    {
        public const string Domain = "Domain";
        public const string MicroServiceName = "SurveyService";

        public HttpStatusCode Status;
        public string Code;
        public Source Source;
        public string Title;

        public static IEnumerable<Error> CreateError(string exceptionMessage, HttpStatusCode httpStatusCode, int errorCode)
        {
            yield return new Error
            {
                Status = httpStatusCode,
                Code = string.Concat(Domain, ".", MicroServiceName, ".", errorCode),
                Source = new Source { Pointer = null },
                Title = exceptionMessage
            };
        }
    }

    public class Source
    {
        public string Pointer;
    }

    public enum ErrorCode
    {
        MissingCandidateInformation = 1000000,
        UnknownError = 6000001
    }

    public static class ErrorMessage
    {
        public const string UnexpectedErrorOccur = "An unexpected error occurs";
        public const string MissingCandidateInformation = "Missing candidate Information";
    }
}

