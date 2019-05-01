namespace SurveyWebApplication.Models
{
    public enum PreferredModeOfInterview
    {
        CodingChallenge = 1,
        Project = 2
    }
    public class Candidate
    {
        public string Name { get; set; }
        public double YearsOfExperience { get; set; }
        public PreferredModeOfInterview PreferredModeOfInterview { get; set; }

    }
}