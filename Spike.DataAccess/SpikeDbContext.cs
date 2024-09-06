using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spike.DataAccess.Entities;

namespace Spike.DataAccess
{
    public class SpikeDbContext: IdentityDbContext<SpikeUser>
    {        
        public SpikeDbContext(DbContextOptions<SpikeDbContext> options)
              :base(options)
        { }
        
    }
}
