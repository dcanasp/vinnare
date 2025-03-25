using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<ProductService> _logger;
        private readonly IJobService _jobService;
        private readonly IUserService _userService;
        private readonly IReviewService _reviewService;


        public ProductService(VinnareDbContext context, ILogger<ProductService> logger, IJobService jobService, IUserService userService, IReviewService reviewService)
        {
            _context = context;
            _logger = logger;
            _jobService = jobService;
            _userService = userService;
            _reviewService = reviewService;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {

            /*/var products = await _context.Products
            .Include(p => p.Category)
            .ToListAsync();*/

            _logger.LogInformation("TESING");
            return await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    OwnerId = p.OwnerId,
                    Title = p.Title,
                    Price = p.Price,
                    Category = p.Category,
                    Description = p.Description,
                    Image = p.Image,
                    Approved = p.Approved,
                    Quantity = p.Quantity,
                    Available = p.Available
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductViewPage>> GetAvailableProductsPageAsync()
        {
            var products = await _context.Products
                .Where(p => p.Available > 0 && p.Approved == true)
                .ToListAsync();

            var productViewPages = new List<ProductViewPage>();

            foreach (var product in products)
            {
                var rate = await _reviewService.GetReviewsRateByIdAsync(product.Id);
                productViewPages.Add(new ProductViewPage
                {
                    Id = product.Id,
                    Title = product.Title,
                    Price = product.Price,
                    Description = product.Description,
                    Category = product.Category,
                    Image = product.Image,
                    Rate = rate
                });
            }

            return productViewPages;
        }


        public async Task<ProductDetail> GetProductByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            var rate = await _reviewService.GetReviewsRateByIdAsync(product.Id);

            return new ProductDetail
            {
                Id = product.Id,
                Title = product.Title,
                Price = product.Price,
                Description = product.Description,
                Category = product.Category,
                Image = product.Image,
                Rate = rate,
                Quantity = product.Quantity,
                Available = product.Available
            };
        }

        public async Task<IEnumerable<ProductViewPage>> GetProductsByCategoryAsync(int id)
        {
            var products = await _context.Products
                .Where(p => p.Available > 0 && p.Approved == true && p.CategoryId == id)
                .ToListAsync();

            var productViewPages = new List<ProductViewPage>();

            foreach (var product in products)
            {
                var rate = await _reviewService.GetReviewsRateByIdAsync(product.Id);
                productViewPages.Add(new ProductViewPage
                {
                    Id = product.Id,
                    Title = product.Title,
                    Price = product.Price,
                    Description = product.Description,
                    Category = product.Category,
                    Image = product.Image,
                    Rate = rate
                });
            }

            return productViewPages;
        }

        public async Task<Product> CreateProductAsync(ProductRequest productDto)
        {

            // Verify if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == productDto.OwnerId);
            if (!userExists)
            {
                throw new Exception("The owner doesn't exists.");
            }

            //Verify if the category exists
            var categoryExists = await _context.Categories.FirstOrDefaultAsync(c => c.Name == productDto.Category);
            if (categoryExists == null)
            {
                throw new Exception("The category doesn't exists.");
            }

            var product = new Product
            {
                OwnerId = productDto.OwnerId,
                Title = productDto.Title,
                Price = productDto.Price,
                Category = productDto.Category,
                CategoryId = categoryExists.Id,
                Approved = true,
                Description = productDto.Description,
                Image = productDto.Image,
                Quantity = productDto.Quantity,
                Available = productDto.Available
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Product> CreateProductByEmployeeAsync(ProductRequest productDto)
        {

            // Verify if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == productDto.OwnerId);
            if (!userExists)
            {
                throw new Exception("The owner doesn't exists.");
            }

            //Verify if the category exists
            var categoryExists = await _context.Categories.FirstOrDefaultAsync(c => c.Name == productDto.Category);
            if (categoryExists == null)
            {
                throw new Exception("The category doesn't exists.");
            }

            Guid userId = (Guid) await _userService.GetIdByUsername(productDto.Username);

            if (userId == Guid.Empty)
            {
                throw new NotFoundException("User not found.");
                //Console.WriteLine($"User ID: {userId}");
            }

            var product = new Product
            {
                OwnerId = productDto.OwnerId,
                Title = productDto.Title,
                Price = productDto.Price,
                Category = productDto.Category,
                CategoryId = categoryExists.Id,
                Approved = false,
                Description = productDto.Description,
                Image = productDto.Image,
                Quantity = productDto.Quantity,
                Available = productDto.Available
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();


            await _jobService.CreateJobAsync(new JobDto
            {
                Type = JobType.Product,
                Operation = OperationType.Create,
                CreatorId = userId,
                ProductId = product.Id
            });
            //await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Product> UpdateProductAsync(int id, ProductUpdate productDto)
        {

            //Verify if the category exists
            var categoryExists = await _context.Categories.FirstOrDefaultAsync(c => c.Name == productDto.Category);
            if (categoryExists == null)
            {
                throw new Exception("The category doesn't exists.");
            }

            //Verify if the product exists
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                throw new Exception("The product doesn't exists.");
            }

            product.OwnerId = productDto.OwnerId ?? product.OwnerId;
            product.Title = productDto.Title ?? product.Title;
            product.Price = productDto.Price ?? product.Price;
            //product.Approved = productDto.Approved ?? product.Approved;
            product.Description = productDto.Description ?? product.Description;
            product.Image = productDto.Image ?? product.Image;
            product.Quantity = productDto.Quantity ?? product.Quantity;
            product.Available = productDto.Available ?? product.Available;
            product.Category = productDto.Category ?? product.Category;
            product.CategoryId = categoryExists.Id;

            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<string> DeleteProductAsync(int id)
        {
            string message = "Product deleted successfully";
            // Verify if the product exists
            var productExists = await _context.Products.AnyAsync(u => u.Id == id);
            if (!productExists)
            {
                throw new NotFoundException("The product doesn't exists.");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return "null";

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<string> DeleteProductByEmployeeAsync(int id, string username)
        {
            string message = "You can't delete a product";
            // Verify if the product exists
            var productExists = await _context.Products.AnyAsync(u => u.Id == id);
            if (!productExists)
            {
                throw new NotFoundException("The product doesn't exists.");
            }

            Guid userId = (Guid)await _userService.GetIdByUsername(username);

            if (userId == Guid.Empty)
            {
                throw new NotFoundException("User not found.");
                //Console.WriteLine($"User ID: {userId}");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return "null";

            await _context.SaveChangesAsync();

            await _jobService.CreateJobAsync(new JobDto
            {
                Type = JobType.Product,
                Operation = OperationType.Delete,
                CreatorId = userId,
                ProductId = product.Id
            });

            //_context.Products.Remove(product);
            //await _context.SaveChangesAsync();
            return message;
        }

        public async Task ApproveProduct(int productId, bool approve)
        {

            var product = await _context.Products.FindAsync(productId);
            product!.Approved = approve;
            await _context.SaveChangesAsync();

        }
    }
}
