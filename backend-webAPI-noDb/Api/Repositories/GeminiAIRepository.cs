using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Constants;
using Api.Data;
using Api.Repositories.IRepositories;
using Api.Services.AIServices.Gemini;

namespace Api.Repositories
{
    public class GeminiAIRepository//(AppDbContext db) : Repository<GeminiAIResponse>(db), IGeminiAIRepository
    {
        // private readonly AppDbContext _db = db;

        // public async Task<ResponseAPI<string>> AddGeminiAIResponseConfigIdAsync(GeminiAIResponse response)
        // {
        //     ResponseAPI<string> responseAPI = new();
        //     try
        //     {
        //         response.Id = Guid.NewGuid().ToString(); // Generate a new Id for the response

        //         // await _db.GeminiAIResponses.AddAsync(response);
        //         await _db.SaveChangesAsync(); // Save changes to the database

        //         responseAPI.SetSuccessResponse();
        //         responseAPI.Data = response.Id; // Set the Id of the newly created response
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new Exception("Error adding Gemini AI response", ex);
        //     }
        //     return responseAPI;
        // }
    }
}