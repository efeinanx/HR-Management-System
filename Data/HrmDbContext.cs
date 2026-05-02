using HrmApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HrmApp.Data;

public class HrmDbContext : IdentityDbContext<ApplicationUser>
{
    public HrmDbContext(DbContextOptions<HrmDbContext> options)
        : base(options)
    {
    }

    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<CandidateEducation> CandidateEducations => Set<CandidateEducation>();
    public DbSet<CandidateExperience> CandidateExperiences => Set<CandidateExperience>();
    public DbSet<CandidateLanguage> CandidateLanguages => Set<CandidateLanguage>();
    public DbSet<CandidateCertificate> CandidateCertificates => Set<CandidateCertificate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CompanyProfile>(e =>
        {
            e.HasOne(c => c.User)
                .WithOne(u => u.CompanyProfile)
                .HasForeignKey<CompanyProfile>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CandidateProfile>(e =>
        {
            e.HasOne(c => c.User)
                .WithOne(u => u.CandidateProfile)
                .HasForeignKey<CandidateProfile>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<JobPosting>(e =>
        {
            e.HasOne(j => j.Company)
                .WithMany(c => c.JobPostings)
                .HasForeignKey(j => j.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<JobApplication>(e =>
        {
            e.HasOne(a => a.JobPosting)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Candidate)
                .WithMany(c => c.Applications)
                .HasForeignKey(a => a.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(a => new { a.JobPostingId, a.CandidateProfileId }).IsUnique();
        });

        builder.Entity<CandidateEducation>(e =>
        {
            e.HasOne(x => x.CandidateProfile)
                .WithMany(p => p.Educations)
                .HasForeignKey(x => x.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CandidateExperience>(e =>
        {
            e.HasOne(x => x.CandidateProfile)
                .WithMany(p => p.Experiences)
                .HasForeignKey(x => x.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CandidateLanguage>(e =>
        {
            e.HasOne(x => x.CandidateProfile)
                .WithMany(p => p.Languages)
                .HasForeignKey(x => x.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CandidateCertificate>(e =>
        {
            e.HasOne(x => x.CandidateProfile)
                .WithMany(p => p.Certificates)
                .HasForeignKey(x => x.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
