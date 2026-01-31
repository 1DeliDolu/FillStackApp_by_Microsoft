namespace ClientApp.Models
{
    public sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
        public CategoryInfo? Category { get; set; }
    }

    public sealed class CategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
