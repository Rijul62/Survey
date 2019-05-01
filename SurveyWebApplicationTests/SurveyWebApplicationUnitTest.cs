using AutoMoqCore;
using System;
using SurveyWebApplication.Controllers;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SurveyWebApplication.Helpers;
using SurveyWebApplication.Models;
using System.Collections.Generic;
using Halcyon.HAL;
using FluentAssertions;
using Newtonsoft.Json;
using System.Linq;
using System.Net;

namespace SurveyWebApplicationTests
{
    [Trait("Category", "UnitTests")]
    public class SurveyWebApplicationUnitTest
    {
        private static List<Candidate> GenerateRandomCandidates(int numberOfCandidates)
        {
            var candidateList = new List<Candidate>();
            for (int i = 1; i <= numberOfCandidates; i++)
            {
                var item = new Candidate
                {
                    Name = string.Concat("Name", i),
                    YearsOfExperience = i,
                    PreferredModeOfInterview = (i % 2) != 0 ? PreferredModeOfInterview.CodingChallenge : PreferredModeOfInterview.Project
                };
                candidateList.Add(item);
            }

            return candidateList;
        }

        [Fact]
        public void Get_ReturnsRecordsWithExpectedValues()
        {
            // Arrange
            var autoMoq = new AutoMoqer();

            var surveyController = autoMoq.Create<SurveyController>();

            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            surveyController.ControllerContext = controllerContext;
            var jsonFileReaderWriter = autoMoq.GetMock<IJsonFileReaderWriter>();
            jsonFileReaderWriter.Setup(mock => mock.ReadJson()).Returns(GenerateRandomCandidates(10));

            // Act
            var urlHelper = autoMoq.GetMock<IUrlHelper>();
            surveyController.Url = urlHelper.Object;
            var result = surveyController.Analyse() as OkObjectResult;

            // Assert
            result.Should().NotBeNull();

            var halResponse = (HALResponse)result?.Value;
            var jObject = halResponse?.ToJObject(new JsonSerializer());
            var recordsToken = jObject?[HALResponse.EmbeddedKey]["candidates"];
            var records = JsonConvert.DeserializeObject<List<Candidate>>(recordsToken?.ToString());
            records.Count.Should().Be(10);
            records.First().Should().NotBeNull();
            records.First().Name.Should().Be("Name1");
            records.First().YearsOfExperience.Should().Be(1);
            records.First().PreferredModeOfInterview.Should().Be(PreferredModeOfInterview.CodingChallenge);
            records.Last().Name.Should().Be("Name10");
            records.Last().YearsOfExperience.Should().Be(10);
            records.Last().PreferredModeOfInterview.Should().Be(PreferredModeOfInterview.Project);
        }

        [Fact]
        public void Get_NoRecordsAreAvailable_ReturnsNoContent()
        {
            // Arrange
            var autoMoq = new AutoMoqer();

            var surveyController = autoMoq.Create<SurveyController>();

            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            surveyController.ControllerContext = controllerContext;
            var jsonFileReaderWriter = autoMoq.GetMock<IJsonFileReaderWriter>();
            jsonFileReaderWriter.Setup(mock => mock.ReadJson()).Returns(GenerateRandomCandidates(0));

            // Act
            var urlHelper = autoMoq.GetMock<IUrlHelper>();
            surveyController.Url = urlHelper.Object;
            var result = surveyController.Analyse() as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NoContentResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        [Fact]
        public void Get_ExceptionIsThrown_ReturnsInternalServerError()
        {
            // Arrange
            var autoMoq = new AutoMoqer();

            var surveyController = autoMoq.Create<SurveyController>();

            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            surveyController.ControllerContext = controllerContext;
            var jsonFileReaderWriter = autoMoq.GetMock<IJsonFileReaderWriter>();
            jsonFileReaderWriter.Setup(mock => mock.ReadJson()).Throws(new Exception("Unknown Exception"));

            // Act
            var urlHelper = autoMoq.GetMock<IUrlHelper>();
            surveyController.Url = urlHelper.Object;
            var result = surveyController.Analyse() as ObjectResult;

            // Assert
            result.Should().NotBeNull();

            var halResponse = (HALResponse)result?.Value;
            var jObject = halResponse?.ToJObject(new JsonSerializer());
            var errorsToken = jObject?[HALResponse.EmbeddedKey]["_errors"];
            var errors = JsonConvert.DeserializeObject<IEnumerable<Error>>(errorsToken?.ToString());
            var errorList = errors.ToList();
            errorList.Should().HaveCount(1);
            errorList.First().Status.Should().Be(HttpStatusCode.InternalServerError);
            errorList.First().Code.Should().Be("Domain.SurveyService.6000001");
            errorList.First().Source.Should().NotBeNull();
            errorList.First().Source.Pointer.Should().BeNullOrEmpty();
            errorList.First().Title.Should().Be("Unknown Exception");
        }

        [Fact]
        public void Post_ReturnsSuccess()
        {
            // Arrange
            var autoMoq = new AutoMoqer();

            var surveyController = autoMoq.Create<SurveyController>();

            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            surveyController.ControllerContext = controllerContext;
            var jsonFileReaderWriter = autoMoq.GetMock<IJsonFileReaderWriter>();
            jsonFileReaderWriter.Setup(mock => mock.WriteJson(new Candidate
            {
                Name = string.Concat("Name1"),
                YearsOfExperience = 1,
                PreferredModeOfInterview = PreferredModeOfInterview.CodingChallenge
            }));

            // Act
            var urlHelper = autoMoq.GetMock<IUrlHelper>();
            surveyController.Url = urlHelper.Object;
            var result = surveyController.RecordSurvey(new Candidate()
            {
                Name = string.Concat("Name1"),
                YearsOfExperience = 1,
                PreferredModeOfInterview = PreferredModeOfInterview.CodingChallenge
            }) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);

        }

        [Fact]
        public void Post_OnNullDataProvided_ReturnsBadRequest()
        {
            // Arrange
            var autoMoq = new AutoMoqer();

            var surveyController = autoMoq.Create<SurveyController>();

            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            surveyController.ControllerContext = controllerContext;
            var jsonFileReaderWriter = autoMoq.GetMock<IJsonFileReaderWriter>();
            jsonFileReaderWriter.SetupAllProperties();

            // Act
            var urlHelper = autoMoq.GetMock<IUrlHelper>();
            surveyController.Url = urlHelper.Object;
            var result = surveyController.RecordSurvey(null) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

    }
}
