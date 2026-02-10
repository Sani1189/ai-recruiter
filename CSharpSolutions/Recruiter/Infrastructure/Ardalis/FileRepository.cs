using Recruiter.Application.File.Interfaces;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

public class FileRepository : EfRepository<Domain.Models.File>, IFileRepository
{
    public FileRepository(RecruiterDbContext context) : base(context)
    {
    }
}
