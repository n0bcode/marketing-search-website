using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly AppDbContext _db;

        public DbInitializer(AppDbContext db)
        {
            _db = db;
        }

        public void Initializer()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() == 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gặp lỗi: " + ex.Message);
                Console.WriteLine("Liên hệ nhà phát triển qua số sau để được hỗ trợ: 0347-018-582.");
            }
        }
    }
}