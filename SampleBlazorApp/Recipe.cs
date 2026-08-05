namespace SampleBlazorApp
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Ingredients { get; set; } = [];
        public List<string> Instructions { get; set; } = [];
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public int Servings { get; set; }
        public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;
    }
}
