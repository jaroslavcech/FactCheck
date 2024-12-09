using FactCheck1.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace FactCheck1.Services;

public class DbService
{
    private readonly FactcheckContext _context;
    private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

    public DbService(FactcheckContext context) => this._context = context;

    public async Task<List<Article>> GetArticles(string Lang)
    {
        await DbService.semaphoreSlim.WaitAsync();
        List<Article> p = new List<Article>();
        try
        {
            p = await this._context.Articles.Include(x => x.Hashtags).Where<Article>((Expression<Func<Article, bool>>) (x => x.Lang == Lang)).OrderByDescending<Article, DateTime>((Expression<Func<Article, DateTime>>) (y => y.Date)).ToListAsync<Article>();
        }
        finally
        {
            DbService.semaphoreSlim.Release();
        }
        List<Article> articles = p;
        p = (List<Article>) null;
        return articles;
    }
}