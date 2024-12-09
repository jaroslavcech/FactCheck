using Microsoft.EntityFrameworkCore;

namespace FactCheck1.Data;

public class FactcheckContext: DbContext
{
    public FactcheckContext(DbContextOptions<FactcheckContext> options)
        : base((DbContextOptions) options)
    {
    }
    
    public DbSet<Article> Articles { get; set; } = (DbSet<Article>) null;
    public DbSet<Hashtag> Hashtags { get; set; } = (DbSet<Hashtag>) null;
}