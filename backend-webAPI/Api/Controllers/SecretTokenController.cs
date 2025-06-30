using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Models;
using Api.Repositories.IRepositories;
using Api.Repositories.MongoDb;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SecretTokenController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkMongo _unitMongo;
        public SecretTokenController(IUnitOfWork unitOfWork,
                                     IUnitOfWorkMongo unitOfWorkMongo)
        {
            _unitOfWork = unitOfWork;
            _unitMongo = unitOfWorkMongo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var responseSecretTokens = await _unitMongo.SecretTokens.GetAllAsync();
            return Ok(responseSecretTokens);
        }
        [HttpPost]
        public async Task<IActionResult> Upsert([FromBody] SecretTokenRequestDTO secretTokenDTO)
        {
            if (secretTokenDTO == null)
            {
                return BadRequest("Invalid data.");
            }
            var response = await _unitMongo.SecretTokens.UpsertAsync(secretTokenDTO);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid ID.");
            }
            var result = await _unitMongo.SecretTokens.DeleteAsync(id);
            if (result)
            {
                return Ok(new { message = "Deleted successfully." });
            }
            return NotFound(new { message = "Not found." });
        }
    }
}