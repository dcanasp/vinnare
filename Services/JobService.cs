using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class JobService : IJobService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<JobService> _logger;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;

        public JobService(VinnareDbContext context, ILogger<JobService> logger, ICategoryService categoryService, IProductService productService, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _categoryService = categoryService;
            _productService = productService;
            _userService = userService;
        }

        public async Task<IEnumerable<ViewJobResponse>> GetAllJobsAsync()
        {
            var jobs = await _context.Jobs.ToListAsync();

            var jobList = new List<ViewJobResponse>();

            foreach (var job in jobs)
            {
                var categoryName = await _categoryService.GetCategoryNameByIdAsync(job.Category.Id);
                var productName = await _productService.GetProductNameByIdAsync(job.Product.Id);
                var creatorName = await _userService.GetUsernameById(job.CreatorId);
                jobList.Add(new ViewJobResponse
                {
                    id = job.Id,
                    JobType = job.Type.ToString(),
                    Operation = job.Operation.ToString(),
                    AssociatedId = (int)(job.ProductId ?? job.CategoryId),
                    CategoryName = categoryName,
                    ProductName = productName,
                    CreatorName = creatorName
                });
            }

            return jobList;
        }

        public async Task<JobDto?> GetJobByIdAsync(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return null;
            }

            return new JobDto
            {
                Id = job.Id,
                Type = job.Type,
                Operation = job.Operation,
                CreatorId = job.CreatorId,
                Date = job.Date,
                ProductId = job.ProductId,
                CategoryId = job.CategoryId,
            };
        }

        public async Task<JobDto> CreateJobAsync(JobDto jobDto)
        {
            var job = new Job
            {
                Type = jobDto.Type,
                Operation = jobDto.Operation,
                CreatorId = jobDto.CreatorId,
                CategoryId = jobDto.CategoryId,
                ProductId = jobDto.ProductId,
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return new JobDto
            {
                Id = job.Id,
                Type = job.Type,
                Operation = job.Operation,
                CreatorId = job.CreatorId,
                CategoryId = job.CategoryId,
                ProductId = job.ProductId,
                Date = job.Date

            };
        }


        public async Task<bool> RemoveJob(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);

            if (job == null)
            {
                return false;
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
