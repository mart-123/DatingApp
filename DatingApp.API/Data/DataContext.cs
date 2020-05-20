using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        // 'cons' to select auto-create a constructor
        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        // 'prop' to select auto-create a property (with get/set accessors)
        public DbSet<Value> Values { get; set; }
        
    }
}