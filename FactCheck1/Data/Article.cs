using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


#nullable enable

namespace FactCheck1.Data;

public class Article
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Newspaper { get; set; }

    public string Url { get; set; }

    public DateTime Date { get; set; }

    public string Bard { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string Lang { get; set; }

    public int Errors { get; set; }
    
    public virtual ICollection<Hashtag>? Hashtags { get; set; }
}