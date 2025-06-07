using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Models;
using Api.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SecretTokenController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public SecretTokenController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var responseSecretTokens = await _unitOfWork.SecretTokens.GetAllAsync();
            return Ok(responseSecretTokens);
        }
        [HttpPost]
        public async Task<IActionResult> Upsert([FromBody] SecretTokenRequestDTO secretTokenDTO)
        {
            if (secretTokenDTO == null)
            {
                return BadRequest("Invalid data.");
            }
            var response = await _unitOfWork.SecretTokens.UpsertAsync(secretTokenDTO);
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
            var result = await _unitOfWork.SecretTokens.DeleteAsync(id);
            if (result)
            {
                return Ok(new { message = "Deleted successfully." });
            }
            return NotFound(new { message = "Not found." });
        }
    }
}