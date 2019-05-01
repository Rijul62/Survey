using SurveyWebApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebApplication.Helpers
{
    public interface IJsonFileReaderWriter
    {
        List<Candidate> ReadJson();
        void WriteJson(Candidate candidate);
    }
}
