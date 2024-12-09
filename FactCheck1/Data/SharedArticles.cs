namespace FactCheck1.Data{
    public class SharedArticles
    {
        public List<Article> Articles { get; private set; }
        
        public event Action OnChange = null!;

        public void SetArticles(List<Article> articles)
        {
            this.Articles = articles;
            this.NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            Action onChange = this.OnChange;
            if (onChange == null)
                return;
            onChange();
        }
    }
}