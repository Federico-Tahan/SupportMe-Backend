using Microsoft.EntityFrameworkCore;
using SupportMe.Data;
using SupportMe.DTOs.CategoryDTOs;

namespace SupportMe.Services
{
    public class CategoryService
    {
        public readonly DataContext _context;

        public CategoryService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<SimpleCategory>> GetCategories() 
        {
            var categories = await _context.Category.Select(x => new SimpleCategory { Id = x.Id, Name = x.Name }).ToListAsync();
            return categories;
        }
    }
}
