using FactCheck1.Data;
using Microsoft.Extensions.Configuration;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FactCheck1.Services;

public class BardJob : IJob
  {
    private string _urlBard = "http://localhost:8000/inputbard";
    private string _urlBardHash = "http://localhost:8000/inputbardold";
    private string hashNotProcessed = "notprocessed";
    private IPromptFetcher _promptFetcher;
    private readonly FactcheckContext _context;
    private readonly IConfiguration _configuration;

    public BardJob(
      IPromptFetcher promptFetcher,
      FactcheckContext context,
      IConfiguration configuration)
    {
      this._promptFetcher = promptFetcher;
      this._context = context;
      this._configuration = configuration;
    }

    private List<string> GetHashtags(string Text, int num)
    {
      var retVal = new List<string>();
      string pattern = @"#(\w+)";
        
      MatchCollection matches = Regex.Matches(Text, pattern);
        
      int count = 0;
      foreach (Match match in matches)
      {
        retVal.Add(match.Value);
        count++;
        if (count == num) break; // Omezení na prvních 5 hashtagů
      }

      if (retVal.Count == 0)
      {
        retVal.Add(hashNotProcessed);
      }
      return retVal;
    }

    public async Task Execute(IJobExecutionContext context)
    {
      Article article = this._context.Articles.OrderByDescending<Article, DateTime>((Expression<Func<Article, DateTime>>) (x => x.Date)).FirstOrDefault<Article>((Expression<Func<Article, bool>>) (y => y.Bard.Equals("")));
      if (article != null)
      {
        Random random = new Random();
        int delayTime = random.Next(1, 41) * 1000;
        await Task.Delay(delayTime);
        Log.Debug("Found uncommented " + article.Url);
        string prompt = this._configuration["BardPrompt:" + article.Lang];
        Dictionary<string, string> result = await this._promptFetcher.PostPrompt(this._urlBard, prompt.Replace("<TITLE>", article.Title).Replace("<DESCRIPTION>", article.Description).Replace("<URL>", article.Url));
        if (!result.Keys.Contains<string>("ERROR"))
        {
          string response = result.FirstOrDefault<KeyValuePair<string, string>>().Value;
          
          Match match = Regex.Match(response, "<bad>[^<]*?(\\d+)");
          if (match.Success)
          {
            string valueBetweenTags = match.Groups[1].Value;
            try
            {
              article.Errors = int.Parse(valueBetweenTags);
              article.Bard = response;
              int num = await this._context.SaveChangesAsync();
            }
            catch
            {
              Log.Warning("Cannot parse to int value " + valueBetweenTags);
              article.Errors = -1;
            }
            valueBetweenTags = (string) null;
          }
          else if (!response.Equals("InternalServerError"))
          {
            article.Errors = 0;
            article.Bard = response;
            int num = await this._context.SaveChangesAsync();
          }
          response = (string) null;
          match = (Match) null;
        }
        else
          Log.Warning("Fetch error: " + result.Values.First<string>());
        random = (Random) null;
        prompt = (string) null;
        result = (Dictionary<string, string>) null;
        article = (Article) null;
        return;
      }
      else
      {
        Log.Debug("No uncommented article found");
        article = (Article) null;
      }
      Article articleUnhashed = this._context.Articles.Include(b => b.Hashtags).OrderByDescending<Article, DateTime>((Expression<Func<Article, DateTime>>) (x => x.Date)).FirstOrDefault<Article>((Expression<Func<Article, bool>>) (y => !y.Hashtags.Any()));
      var hashtags = _context.Hashtags.FirstOrDefault(x => x.ArticleId == articleUnhashed.Id);
      if (hashtags == null)
      {
        articleUnhashed.Hashtags = new List<Hashtag>();
        Random random = new Random();
        int delayTime = random.Next(1, 41) * 1000;
        await Task.Delay(delayTime);
        Log.Debug("Found unhashed " + articleUnhashed.Url);
        string prompt = this._configuration["BardPrompt:" + articleUnhashed.Lang + "_Hash"];
        Dictionary<string, string> result = await this._promptFetcher.PostPrompt(this._urlBardHash, prompt.Replace("<URL>", articleUnhashed.Url));
        if (!result.Keys.Contains<string>("ERROR"))
        {
          string response = result.FirstOrDefault<KeyValuePair<string, string>>().Value;
          List<string> hastags = GetHashtags(response, 5);
          foreach (var h in hastags)
          {
            Hashtag ha = new Hashtag
            {
              Article = articleUnhashed,
              Value = h,
              ArticleId = articleUnhashed.Id
            };
            articleUnhashed.Hashtags.Add(ha);
          }
          int num = await this._context.SaveChangesAsync();
        }
        else
          Log.Warning("Fetch error: " + result.Values.First<string>());
        random = (Random) null;
        prompt = (string) null;
        result = (Dictionary<string, string>) null;
        article = (Article) null;
        return;
      }
      else
      {
        Log.Debug("No unhashed article found");
        article = (Article) null;
      }
    }
  }