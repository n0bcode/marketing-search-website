using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Utils
{
    public class ExceptionHandler
    {
        public static void HandleException<T>(Exception ex, ResponseAPI<T> response) where T : class
        {
            switch (ex)
            {
                case ArgumentNullException _:
                case ArgumentOutOfRangeException _:
                    response.SetErrorResponse(ex, (int)HttpStatusCode.BadRequest);
                    break;
                case UnauthorizedAccessException _:
                    response.SetErrorResponse(ex, (int)HttpStatusCode.Unauthorized);
                    break;
                case KeyNotFoundException _: // Giả sử bạn có một ngoại lệ NotFoundException
                    response.SetErrorResponse(ex, (int)HttpStatusCode.NotFound);
                    break;
                case InvalidOperationException _:
                    response.SetErrorResponse(ex, (int)HttpStatusCode.Conflict);
                    break;
                case DbUpdateConcurrencyException _:
                    response.SetErrorResponse(ex, (int)HttpStatusCode.Conflict);
                    break;
                case DbUpdateException _:
                    response.SetErrorResponse(ex, (int)HttpStatusCode.InternalServerError);
                    break;
                default:
                    response.SetErrorResponse(ex, (int)HttpStatusCode.InternalServerError);
                    break;

            }
        }

    }
}