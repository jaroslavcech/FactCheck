using FactCheck1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace FactCheck1.Services;

public class RSSDownload
  {
    private readonly IPromptFetcher _promptFetcher;
    private readonly FactcheckContext _context;
    private readonly IConfiguration _configuration;
    private string BARD = "http://localhost:8000/inputbard";

    public RSSDownload(
      IPromptFetcher promptFetcher,
      FactcheckContext context,
      IConfiguration configuration)
    {
      this._promptFetcher = promptFetcher;
      this._context = context;
      this._configuration = configuration;
    }

    public async Task Execute(string RssChannel, string Newspaper, string BardRole, string Lang)
    {
      try
      {
        XmlReader reader = XmlReader.Create(RssChannel);
        SyndicationFeed feed = SyndicationFeed.Load(reader);
        reader.Close();
        
        Log.Information($"RSS {Newspaper} started {DateTime.Now}");
        var newAtricles = 0;
        foreach (SyndicationItem item in (IEnumerable<SyndicationItem>) feed.Items.OrderByDescending<SyndicationItem, DateTimeOffset>((Func<SyndicationItem, DateTimeOffset>) (x => x.PublishDate)))
        {
          try
          {
            Article article = new Article();
            article.Title = item.Title.Text;
            article.Description = item.Summary.Text;
            if (item.Links != null)
              article.Url = item.Links[0].Uri.ToString();
            if (item.Id != null && item.Id.StartsWith("http") && article.Url == null)
              article.Url = item.Id;
            article.Date = DateTime.Now;
            article.Newspaper = Newspaper;
            article.Bard = "";
            article.Lang = Lang;
//            Log.Debug("RSS: " + JsonSerializer.Serialize<Article>(article));
            Article isThere = await this._context.Articles.FirstOrDefaultAsync<Article>((Expression<Func<Article, bool>>) (x => x.Url.Equals(article.Url)));
            if (isThere == null)
            {
              this._context.Articles.Add(article);
              int num = await this._context.SaveChangesAsync();
              Log.Debug("Article " + article.Url + " saved");
              newAtricles++;
            }
            isThere = (Article) null;
          }
          catch (Exception ex)
          {
            Log.Warning("RSS Failed with " + ex.Message + ", " + ex.ToString());
          }
        }
        reader = (XmlReader) null;
        feed = (SyndicationFeed) null;
        Log.Information($"Articles {Newspaper} saved {newAtricles}");
      }
      catch (Exception ex)
      {
        Log.Warning("RSS Failed with " + ex.Message + ", " + ex.ToString());
      }
    }
  }