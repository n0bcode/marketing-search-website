using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories
{
    public class AnalysisLinkRepository(AppDbContext db) : Repository<AnalysisLink>(db), IAnalysisLinkRepository
    {
        private readonly AppDbContext _db = db;
        public async Task<AnalysisLink?> GetAnalysisLinkOrNot(string linkOrKeyword)
        {
            return await _db.AnalysisLinks
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.LinkOrKeyword == linkOrKeyword);
        }
        public async Task AddAsync(AnalysisLink result)
        {
            await _db.AnalysisLinks.AddAsync(result);
            await _db.SaveChangesAsync();
        }

        public async Task<AnalysisLink> AddOrUpdateText(AnalysisLink analysisLink)
        {
            await AddAsync(analysisLink);
            await _db.SaveChangesAsync();
            return analysisLink;
        }
    }
}