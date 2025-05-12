using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Repositories.IRepositories;

namespace Api.Repositories.IRepositories
{
    public interface IUnitOfWork
    {
        IKeywordRepository Keywords { get; }
        // IGeminiAIRepository GeminiAIResponses { get; }
        IAnalysisLinkRepository AnalysisLinks { get; }
        Task SaveAsync();
    }
}