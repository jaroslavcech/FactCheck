namespace FactCheck1.Data;

public class Hashtag
{
    public int Id { get; set; }
    public string Value { get; set; }
    public int ArticleId { get; set; }
    public virtual Article Article { get; set; }
}