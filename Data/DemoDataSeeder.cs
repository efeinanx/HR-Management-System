using System.Globalization;
using System.Text;
using Bogus;
using HrmApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrmApp.Data;

public static class DemoDataSeeder
{
    public const string DemoPassword = "Demo123!";
    public const string DemoMarkerEmail = "company1@demo.hrmapp.local";

    private static readonly string[] Industries =
    {
        "Yazılım", "Finans", "Sağlık", "Perakende", "Üretim",
        "Eğitim", "Lojistik", "Enerji", "Medya", "Danışmanlık"
    };

    private static readonly string[] EmploymentTypes =
    {
        "Tam zamanlı", "Yarı zamanlı", "Sözleşmeli", "Staj"
    };

    private static readonly string[] ApplicationStatuses =
    {
        "Pending", "Approved", "Rejected"
    };

    private static readonly string[] Departments =
    {
        "Bilgisayar Mühendisliği", "Endüstri Mühendisliği", "İşletme",
        "İktisat", "Psikoloji", "Pazarlama", "Finans", "Hukuk"
    };

    private static readonly string[] Universities =
    {
        "Boğaziçi Üniversitesi", "Orta Doğu Teknik Üniversitesi", "İstanbul Üniversitesi",
        "Ankara Üniversitesi", "İstanbul Teknik Üniversitesi", "Hacettepe Üniversitesi",
        "Ege Üniversitesi", "Yıldız Teknik Üniversitesi"
    };

    private static readonly string[] Faculties =
    {
        "Mühendislik", "İktisadi ve İdari Bilimler", "Eğitim", "Tıp",
        "Hukuk", "Mimarlık", "İletişim", "Fen-Edebiyat"
    };

    private static readonly string[] Languages =
    {
        "Türkçe", "İngilizce", "Almanca", "Fransızca", "İspanyolca"
    };

    private static readonly string[] LanguageLevels =
    {
        "Başlangıç", "Orta", "İleri", "Ana dil"
    };

    private static readonly string[] JobTitles =
    {
        "Yazılım Geliştirici", "İnsan Kaynakları Uzmanı", "Pazarlama Uzmanı",
        "Satış Temsilcisi", "Muhasebe Uzmanı", "Veri Analisti", "Ürün Yöneticisi",
        "Grafik Tasarımcı", "Operasyon Uzmanı", "Müşteri İlişkileri Uzmanı"
    };

    public static async Task SeedAsync(HrmDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync(DemoMarkerEmail) != null)
        {
            await RefreshTurkishContentAsync(db);
            return;
        }

        var faker = new Faker("tr");
        var companies = await SeedCompaniesAsync(db, userManager, faker);
        var candidates = await SeedCandidatesAsync(db, userManager, faker);
        var jobs = SeedJobPostings(faker, companies);
        db.JobPostings.AddRange(jobs);
        await db.SaveChangesAsync();

        SeedApplications(db, faker, jobs, candidates);
        await db.SaveChangesAsync();
    }

    private static async Task RefreshTurkishContentAsync(HrmDbContext db)
    {
        var faker = new Faker("tr");
        var companies = await db.CompanyProfiles
            .Include(c => c.User)
            .Include(c => c.JobPostings)
            .Where(c => c.User.Email != null && c.User.Email.EndsWith("@demo.hrmapp.local"))
            .ToListAsync();

        foreach (var company in companies)
        {
            var industry = faker.PickRandom(Industries);
            var location = company.Location ?? faker.PickRandom(StaticOptions.Cities);
            company.Industry = industry;
            company.Location = location;
            company.Description = BuildCompanyDescription(company.CompanyName, industry, location);
            company.Website = BuildWebsite(company.CompanyName);

            foreach (var job in company.JobPostings)
            {
                job.Title = faker.PickRandom(JobTitles);
                job.Description = BuildJobDescription(job.Title, company.CompanyName, location);
                job.Location = faker.PickRandom(StaticOptions.Cities);
                job.EmploymentType = faker.PickRandom(EmploymentTypes);
            }
        }

        var candidates = await db.CandidateProfiles
            .Include(c => c.User)
            .Include(c => c.Educations)
            .Include(c => c.Experiences)
            .Include(c => c.Languages)
            .Include(c => c.Certificates)
            .Where(c => c.User.Email != null && c.User.Email.EndsWith("@demo.hrmapp.local"))
            .ToListAsync();

        foreach (var candidate in candidates)
        {
            candidate.Headline = faker.PickRandom(JobTitles);
            candidate.Summary = BuildCandidateSummary(candidate.FullName, candidate.Headline!);
            candidate.Location = faker.PickRandom(StaticOptions.Cities);

            foreach (var education in candidate.Educations)
            {
                education.University = faker.PickRandom(Universities);
                education.Faculty = faker.PickRandom(Faculties);
                education.Department = faker.PickRandom(Departments);
            }

            foreach (var experience in candidate.Experiences)
            {
                experience.CompanyName = faker.Company.CompanyName();
                experience.Position = faker.PickRandom(JobTitles);
                experience.Description = BuildExperienceDescription(experience.Position, experience.CompanyName);
            }

            foreach (var language in candidate.Languages)
            {
                language.Language = faker.PickRandom(Languages);
                language.Level = faker.PickRandom(LanguageLevels);
            }

            foreach (var certificate in candidate.Certificates)
            {
                certificate.CertificateName = $"{faker.PickRandom(JobTitles)} Sertifikası";
                certificate.Issuer = faker.PickRandom(Universities);
                certificate.CertificateLink = BuildCertificateLink(certificate.CertificateName);
            }
        }

        var applications = await db.JobApplications
            .Include(a => a.Candidate)
            .ThenInclude(c => c.User)
            .Where(a => a.Candidate.User.Email != null && a.Candidate.User.Email.EndsWith("@demo.hrmapp.local"))
            .ToListAsync();

        foreach (var application in applications)
            application.CoverLetter = BuildCoverLetter(application.Candidate.FullName);

        await db.SaveChangesAsync();
    }

    private static async Task<List<CompanyProfile>> SeedCompaniesAsync(
        HrmDbContext db,
        UserManager<ApplicationUser> userManager,
        Faker faker)
    {
        var profiles = new List<CompanyProfile>();

        for (var i = 1; i <= 10; i++)
        {
            var companyName = faker.Company.CompanyName();
            var email = $"company{i}@demo.hrmapp.local";
            var user = await CreateUserAsync(userManager, email, companyName, DbInitializer.CompanyRole);
            if (user == null)
                continue;

            var industry = faker.PickRandom(Industries);
            var location = faker.PickRandom(StaticOptions.Cities);
            var profile = new CompanyProfile
            {
                UserId = user.Id,
                CompanyName = companyName,
                Industry = industry,
                Description = BuildCompanyDescription(companyName, industry, location),
                Website = BuildWebsite(companyName),
                Location = location
            };

            db.CompanyProfiles.Add(profile);
            profiles.Add(profile);
        }

        await db.SaveChangesAsync();
        return profiles;
    }

    private static async Task<List<CandidateProfile>> SeedCandidatesAsync(
        HrmDbContext db,
        UserManager<ApplicationUser> userManager,
        Faker faker)
    {
        var profiles = new List<CandidateProfile>();

        for (var i = 1; i <= 25; i++)
        {
            var fullName = faker.Name.FullName();
            var email = $"candidate{i}@demo.hrmapp.local";
            var user = await CreateUserAsync(userManager, email, fullName, DbInitializer.CandidateRole);
            if (user == null)
                continue;

            var headline = faker.PickRandom(JobTitles);
            var profile = new CandidateProfile
            {
                UserId = user.Id,
                FullName = fullName,
                Headline = headline,
                Summary = BuildCandidateSummary(fullName, headline),
                Phone = faker.Phone.PhoneNumber("05## ### ## ##"),
                Location = faker.PickRandom(StaticOptions.Cities)
            };

            db.CandidateProfiles.Add(profile);
            profiles.Add(profile);
        }

        await db.SaveChangesAsync();

        foreach (var profile in profiles)
        {
            db.CandidateEducations.AddRange(CreateEducations(faker, profile.Id));
            db.CandidateExperiences.AddRange(CreateExperiences(faker, profile.Id));
            db.CandidateLanguages.AddRange(CreateLanguages(faker, profile.Id));
            db.CandidateCertificates.AddRange(CreateCertificates(faker, profile.Id));
        }

        await db.SaveChangesAsync();
        return profiles;
    }

    private static List<JobPosting> SeedJobPostings(Faker faker, IReadOnlyList<CompanyProfile> companies)
    {
        var jobs = new List<JobPosting>();

        foreach (var company in companies)
        {
            var jobCount = faker.Random.Int(3, 5);
            for (var i = 0; i < jobCount; i++)
            {
                var title = faker.PickRandom(JobTitles);
                var location = faker.PickRandom(StaticOptions.Cities);
                jobs.Add(new JobPosting
                {
                    CompanyProfileId = company.Id,
                    Title = title,
                    Description = BuildJobDescription(title, company.CompanyName, location),
                    Location = location,
                    EmploymentType = faker.PickRandom(EmploymentTypes),
                    IsActive = faker.Random.Bool(0.9f),
                    CreatedAt = faker.Date.Between(DateTime.UtcNow.AddMonths(-8), DateTime.UtcNow)
                });
            }
        }

        return jobs;
    }

    private static void SeedApplications(
        HrmDbContext db,
        Faker faker,
        IReadOnlyList<JobPosting> jobs,
        IReadOnlyList<CandidateProfile> candidates)
    {
        var activeJobs = jobs.Where(j => j.IsActive).ToList();
        if (activeJobs.Count == 0)
            return;

        var usedPairs = new HashSet<(int JobPostingId, int CandidateProfileId)>();

        foreach (var candidate in candidates)
        {
            var applicationCount = faker.Random.Int(2, 4);
            foreach (var job in faker.PickRandom(activeJobs, applicationCount))
            {
                if (!usedPairs.Add((job.Id, candidate.Id)))
                    continue;

                db.JobApplications.Add(new JobApplication
                {
                    JobPostingId = job.Id,
                    CandidateProfileId = candidate.Id,
                    Status = faker.PickRandom(ApplicationStatuses),
                    CoverLetter = BuildCoverLetter(candidate.FullName),
                    AppliedAt = faker.Date.Between(job.CreatedAt, DateTime.UtcNow)
                });
            }
        }
    }

    private static IEnumerable<CandidateEducation> CreateEducations(Faker faker, int candidateProfileId)
    {
        return Enumerable.Range(0, faker.Random.Int(1, 2)).Select(_ =>
        {
            var start = faker.Date.Between(new DateTime(2010, 1, 1), new DateTime(2020, 12, 1));
            var end = faker.Date.Between(start.AddYears(3), start.AddYears(5));
            return new CandidateEducation
            {
                CandidateProfileId = candidateProfileId,
                University = faker.PickRandom(Universities),
                Faculty = faker.PickRandom(Faculties),
                Department = faker.PickRandom(Departments),
                StartMonth = start.ToString("yyyy-MM"),
                EndMonth = end.ToString("yyyy-MM")
            };
        });
    }

    private static IEnumerable<CandidateExperience> CreateExperiences(Faker faker, int candidateProfileId)
    {
        return Enumerable.Range(0, faker.Random.Int(1, 3)).Select(_ =>
        {
            var start = faker.Date.Between(new DateTime(2015, 1, 1), new DateTime(2023, 12, 1));
            var end = faker.Random.Bool(0.75f)
                ? faker.Date.Between(start.AddMonths(6), DateTime.UtcNow)
                : (DateTime?)null;
            var companyName = faker.Company.CompanyName();
            var position = faker.PickRandom(JobTitles);
            return new CandidateExperience
            {
                CandidateProfileId = candidateProfileId,
                CompanyName = companyName,
                Position = position,
                StartMonth = start.ToString("yyyy-MM"),
                EndMonth = end?.ToString("yyyy-MM"),
                Description = BuildExperienceDescription(position, companyName)
            };
        });
    }

    private static IEnumerable<CandidateLanguage> CreateLanguages(Faker faker, int candidateProfileId)
    {
        return faker.PickRandom(Languages, faker.Random.Int(1, 3))
            .Distinct()
            .Select(language => new CandidateLanguage
            {
                CandidateProfileId = candidateProfileId,
                Language = language,
                Level = faker.PickRandom(LanguageLevels)
            });
    }

    private static IEnumerable<CandidateCertificate> CreateCertificates(Faker faker, int candidateProfileId)
    {
        return Enumerable.Range(0, faker.Random.Int(0, 2)).Select(_ =>
        {
            var certificateName = $"{faker.PickRandom(JobTitles)} Sertifikası";
            return new CandidateCertificate
            {
                CandidateProfileId = candidateProfileId,
                CertificateName = certificateName,
                Issuer = faker.PickRandom(Universities),
                Year = faker.Random.Int(2015, DateTime.UtcNow.Year),
                CertificateLink = BuildCertificateLink(certificateName)
            };
        });
    }

    private static string BuildCompanyDescription(string companyName, string industry, string location) =>
        $"{companyName}, {location} merkezli {industry} alanında faaliyet gösteren bir kuruluştur. " +
        "Çalışanlarına gelişim odaklı bir ortam sunar, ekip çalışmasına ve sürdürülebilir büyümeye önem verir.";

    private static string BuildJobDescription(string title, string companyName, string location) =>
        $"{companyName} bünyesinde {location} lokasyonunda {title} arayışımız bulunmaktadır. " +
        "Adaylardan ilgili alanda deneyim, güçlü iletişim becerileri ve ekip çalışmasına yatkınlık beklenmektedir.";

    private static string BuildCandidateSummary(string fullName, string headline) =>
        $"{fullName}, {headline} olarak farklı projelerde görev almış; analitik düşünme, planlama ve sonuç odaklı çalışma becerilerine sahiptir.";

    private static string BuildExperienceDescription(string position, string companyName) =>
        $"{companyName} bünyesinde {position} olarak süreç yönetimi, raporlama ve ekip içi koordinasyon görevlerini yürüttü.";

    private static string BuildCoverLetter(string fullName) =>
        $"Merhaba, ben {fullName}. İlanınızı inceledim ve yetkinliklerimin pozisyon gereksinimleriyle uyumlu olduğunu düşünüyorum. " +
        "Katkı sağlamak ve ekibinizde değer üretmek için sabırsızlanıyorum.";

    private static string BuildWebsite(string companyName)
    {
        var slug = Slugify(companyName);
        return string.IsNullOrWhiteSpace(slug)
            ? "https://www.beyhr-demo.com.tr"
            : $"https://www.{slug}.com.tr";
    }

    private static string BuildCertificateLink(string certificateName) =>
        $"https://www.beyhr-demo.com.tr/sertifikalar/{Slugify(certificateName)}";

    private static string Slugify(string value)
    {
        var normalized = value.ToLower(CultureInfo.GetCultureInfo("tr-TR"));
        var builder = new StringBuilder(normalized.Length);
        var previousDash = false;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousDash = false;
                continue;
            }

            if (!previousDash && builder.Length > 0)
            {
                builder.Append('-');
                previousDash = true;
            }
        }

        return builder.ToString().Trim('-');
    }

    private static async Task<ApplicationUser?> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string displayName,
        string role)
    {
        if (await userManager.FindByEmailAsync(email) != null)
            return null;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName
        };

        var result = await userManager.CreateAsync(user, DemoPassword);
        if (!result.Succeeded)
            return null;

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
