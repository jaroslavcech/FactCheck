using System.Collections.Generic;
using System.Threading.Tasks;

namespace FactCheck1.Services;

public interface IPromptFetcher
{
    Task<Dictionary<string, string>> PostPrompt(string url, string prompt);
}