using HrmApp.Models;

namespace HrmApp.ViewModels;

public class CandidateProfilePageViewModel
{
    public CandidateProfileViewModel Profile { get; set; } = new();
    public CandidateEducationViewModel NewEducation { get; set; } = new();
    public CandidateExperienceViewModel NewExperience { get; set; } = new();
    public CandidateLanguageViewModel NewLanguage { get; set; } = new();
    public CandidateCertificateViewModel NewCertificate { get; set; } = new();

    public IList<CandidateEducation> Educations { get; set; } = new List<CandidateEducation>();
    public IList<CandidateExperience> Experiences { get; set; } = new List<CandidateExperience>();
    public IList<CandidateLanguage> Languages { get; set; } = new List<CandidateLanguage>();
    public IList<CandidateCertificate> Certificates { get; set; } = new List<CandidateCertificate>();
}
