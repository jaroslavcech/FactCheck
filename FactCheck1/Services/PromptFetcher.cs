using FactCheck1.Data;
using Serilog;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace FactCheck1.Services;

public class PromptFetcher : IPromptFetcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FactcheckContext _context;

    public PromptFetcher(IHttpClientFactory httpClientFactory, FactcheckContext context)
    {
        this._httpClientFactory = httpClientFactory;
        this._context = context;
    }

    public async Task<Dictionary<string, string>> PostPrompt(string url, string prompt)
    {
        HttpClient httpClient = this._httpClientFactory.CreateClient();
        PostQuery p = new PostQuery()
        {
            text = prompt,
            name = "Bard"
        };
        string qs = p.ToString();
        StringContent content = new StringContent(qs, Encoding.UTF8, "application/json");
        Log.Debug("Call AI " + url + ", " + prompt);
        HttpResponseMessage response = await httpClient.PostAsync(url, (HttpContent) content);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            Log.Debug("Response:" + responseContent);
            Dictionary<string, string> rObj = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
            return rObj;
        }
        Log.Error($"POST request {url} failed. Status code: {response.StatusCode}");
        return new Dictionary<string, string>()
        {
            {
                "ERROR:",
                response.StatusCode.ToString()
            }
        };
    }
}