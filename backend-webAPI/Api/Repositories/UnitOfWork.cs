using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;


namespace Api.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Keywords = new KeywordRepository(_context);
            AnalysisLinks = new AnalysisLinkRepository(_context);
            // GeminiAIResponses = new GeminiAIRepository(_context);
        }

        public IKeywordRepository Keywords { get; private set; }
        // public IGeminiAIRepository GeminiAIResponses { get; private set; }
        public IAnalysisLinkRepository AnalysisLinks { get; private set; }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}