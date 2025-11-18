namespace ProductService.Entity
{
    public class Product
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public  string? Category { get; set; }
        public string? SubCategory { get; set; }

        public string? Brand { get; set; }
        public double Price { get; set; }
        
        public decimal? DiscountPrice { get; set; }
        public int Stock {  get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
    }
}
