using FactCheck1.Data;
using Microsoft.Extensions.Configuration;
using Quartz;
using Serilog;
using System.Threading.Tasks;
namespace FactCheck1.Services;

public class RSSDownloadJob : IJob
{
    private IPromptFetcher _promptFetcher;
    private readonly FactcheckContext _context;
    private readonly IConfiguration _configuration;

    public RSSDownloadJob(
        IPromptFetcher promptFetcher,
        FactcheckContext context,
        IConfiguration configuration)
    {
        this._promptFetcher = promptFetcher;
        this._context = context;
        this._configuration = configuration;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        RSSDownload downloadRSS = new RSSDownload(this._promptFetcher, this._context, this._configuration);
        await downloadRSS.Execute("https://www.forum24.cz/feed", "Forum24", "", "CZ");
        await downloadRSS.Execute("https://www.e15.cz/rss", "E15", "", "CZ");
        await downloadRSS.Execute("https://servis.idnes.cz/rss.aspx?c=zpravodaj", "iDnes", "", "CZ");
        await downloadRSS.Execute("https://ct24.ceskatelevize.cz/rss/hlavni-zpravy", "CT24", "", "CZ");
        Log.Debug("Quarz job");
        downloadRSS = (RSSDownload) null;
    }
}