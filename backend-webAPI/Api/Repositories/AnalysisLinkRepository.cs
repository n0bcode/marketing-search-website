using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Repositories.IRepositories;

namespace Api.Repositories
{
    public class AnalysisLinkRepository(AppDbContext db) : Repository<AnalysisLink>(db), IAnalysisLinkRepository
    {
        private readonly AppDbContext _db = db;
        public async Task<AnalysisLink?> GetAnalysisLinkOrNot(string link)
        {
            AnalysisLink? analysisLinkFromDbYet = await base.GetAsync(x => x.Link == link);
            return analysisLinkFromDbYet;
        }

        public async Task<AnalysisLink> AddOrUpdateText(AnalysisLink analysisLink)
        {
            await AddAsync(analysisLink);
            await _db.SaveChangesAsync();
            return analysisLink;
        }
    }
}