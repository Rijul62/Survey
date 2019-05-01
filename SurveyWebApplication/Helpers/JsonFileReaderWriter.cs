using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using SurveyWebApplication.Models;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Diagnostics;
using Newtonsoft.Json.Linq;

namespace SurveyWebApplication.Helpers
{
    public class JsonFileReaderWriter : IJsonFileReaderWriter

    {
        private List<Candidate> UpdatedCandidateList;
        public JsonFileReaderWriter()
        {
            UpdatedCandidateList = new List<Candidate>();
        }
        public List<Candidate> ReadJson()
        {
            try
            {
                using (StreamReader r = new StreamReader("candiateDetails.json"))
                {
                    var json = r.ReadToEnd();
                    var candidateList = JsonConvert.DeserializeObject<List<Candidate>>(json);
                    foreach (var item in candidateList)
                    {
                        return candidateList;
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WriteJson(Candidate candidate)
        {
            try
            {
                using (StreamReader r = new StreamReader("candiateDetails.json"))
                {
                    var json = r.ReadToEnd();
                    var candidateList = JsonConvert.DeserializeObject<List<Candidate>>(json);
                    UpdatedCandidateList = candidateList;
                    UpdatedCandidateList.Add(candidate);
                }

                var updatedJson = JsonConvert.SerializeObject(UpdatedCandidateList);
                File.WriteAllText("candiateDetails.json", updatedJson);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
