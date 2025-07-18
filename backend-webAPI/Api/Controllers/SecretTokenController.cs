using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Models;
using Api.Repositories.MongoDb;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Controller for managing secret tokens.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SecretTokenController : ControllerBase
    {
        #region Fields

        private readonly IUnitOfWorkMongo _unitMongo;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretTokenController"/> class.
        /// </summary>
        /// <param name="unitOfWorkMongo">The unit of work for MongoDB operations.</param>
        public SecretTokenController(IUnitOfWorkMongo unitOfWorkMongo)
        {
            _unitMongo = unitOfWorkMongo;
        }

        #endregion

        #region Public Endpoints

        /// <summary>
        /// Retrieves all secret tokens.
        /// </summary>
        /// <returns>A list of <see cref="SecretToken"/> objects.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var responseSecretTokens = await _unitMongo.SecretTokens.GetAllAsync();
            return Ok(responseSecretTokens);
        }

        /// <summary>
        /// Inserts or updates a secret token.
        /// </summary>
        /// <param name="secretTokenDTO">The secret token data transfer object.</param>
        /// <returns>The result of the upsert operation.</returns>
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

        /// <summary>
        /// Deletes a secret token by its ID.
        /// </summary>
        /// <param name="id">The ID of the secret token to delete.</param>
        /// <returns>A confirmation message if deletion is successful, otherwise a not found message.</returns>
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

        #endregion
    }
}