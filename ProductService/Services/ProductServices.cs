using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.Entity;
using ProductService.Productdto;

namespace ProductService.Services
{
    public class ProductServices:IProductService
    {
        private readonly ProductContext _context;
        public ProductServices (ProductContext context)
        {
            _context = context; 
        }
        public async Task<IEnumerable<ProductResponsedto>> GetAll()
        {
            return await _context.Products.Select(p => new ProductResponsedto
            {
                Id = p.Id,
                Title = p.Title,
                Category = p.Category,
                SubCategory = p.SubCategory,
                Brand = p.Brand,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,

            }).ToListAsync();
        }

        public async Task<ProductResponsedto?> GetById(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) {
                return null;
                    }

            return new ProductResponsedto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                SubCategory = p.SubCategory,
                Brand = p.Brand,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
            };
        }
        public async Task<ProductResponsedto> Create(ProductCreatedto p)
        {
            var pro = new Product
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                SubCategory = p.SubCategory,
                Brand = p.Brand,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,

            };
            _context.Products.Add(pro);
            await _context.SaveChangesAsync();
            return await GetById(pro.Id);
        }


        public async Task<ProductResponsedto> Update(ProductUpdatedto dto)
        {

            var p = await _context.Products.FindAsync(dto.Id);
            if (p == null)
            {
                return null;
            }

            p.Title = dto.Title;
            p.Description = dto.Description;
            p.Category = dto.Category;
            p.SubCategory = dto.SubCategory;
            p.Brand = dto.Brand;
            p.Price = dto.Price;
            p.DiscountPrice = dto.DiscountPrice;
            p.Stock = dto.Stock;
            p.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();
            return await GetById(p.Id);
        }

        public async Task<bool> Delete (int id)
        {
            var p=await _context.Products.FindAsync(id);
            if(p==null) { return false; }   

            _context.Products.Remove(p);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
