using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Calculator.Data
{
    // w wpf  ef nie może zbudować AppDbContext w czasie projektowania wiec trzbea dodać tą klase
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=calculator.db")
                .Options;

            return new AppDbContext(options);
        }
    }
}
