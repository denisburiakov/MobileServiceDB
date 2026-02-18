using Microsoft.EntityFrameworkCore;

namespace MobileServiceSite.Models
{
    public class ApplicationContext : DbContext 
    {
        public DbSet<Client> Users { get; set; }  
    }
}


 
