using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Api.Constants
{
    public class ExceptionHandler
    {
        public static void HandleException<T>(Exception ex, ResponseAPI<T> response) where T : class
        {
            switch (ex)
            {
                case ArgumentNullException _:
                case ArgumentOutOfRangeException _:
                    response.SetErrorResponse(ex, HttpStatusCodes.BadRequest);
                    break;
                case UnauthorizedAccessException _:
                    response.SetErrorResponse(ex, HttpStatusCodes.Unauthorized);
                    break;
                case KeyNotFoundException _: // Giả sử bạn có một ngoại lệ NotFoundException
                    response.SetErrorResponse(ex, HttpStatusCodes.NotFound);
                    break;
                case InvalidOperationException _:
                    response.SetErrorResponse(ex, HttpStatusCodes.Conflict);
                    break;
                case DbUpdateConcurrencyException _:
                    response.SetErrorResponse(ex, HttpStatusCodes.Conflict);
                    break;
                case DbUpdateException _:
                    response.SetErrorResponse(ex, HttpStatusCodes.InternalServerError);
                    break;
                default:
                    response.SetErrorResponse(ex, HttpStatusCodes.InternalServerError);
                    break;

            }
        }

    }
}