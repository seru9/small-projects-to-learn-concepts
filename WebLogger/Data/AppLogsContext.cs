using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PcsSelcomWebLogger.Models;

namespace PcsSelcomWebLogger.Data
{
    public class AppLogsContext : DbContext
    {
        public AppLogsContext (DbContextOptions<AppLogsContext> options)
            : base(options)
        {
        }
        public DbSet<PcsSelcomWebLogger.Models.AppLogs> AppLogs { get; set; }
        public DbSet<PcsSelcomWebLogger.Models.Users> Users { get; set; }
    }
}
