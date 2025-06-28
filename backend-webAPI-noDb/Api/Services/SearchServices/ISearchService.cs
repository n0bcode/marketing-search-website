using Microsoft.OpenApi.Services;

namespace Api.Services.SearchServices
{
    public interface ISearchService<IRequest, IResult>
    {
        Task<IResult> SearchAsync(IRequest request);
    }
}
