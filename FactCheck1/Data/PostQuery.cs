
using System.Text.Json;


#nullable enable

namespace FactCheck1.Data;

public class PostQuery
{
    public string text { get; set; }

    public string name { get; set; }

    public override string ToString() => JsonSerializer.Serialize<PostQuery>(this);
}