import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { inject } from '@angular/core';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unknown error occurred!';
      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Error: ${error.error.message}`;
      } else {
        // Server-side error
        errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
        // Add detailed info from response body if available
        if (error.error && error.error.message) {
          errorMessage = error.error.message;
        } else if (error.error) {
          errorMessage += `\nDetails: ${JSON.stringify(error.error)}`;
        }
      }

      console.error(errorMessage); // Log the error

      // Display error message to the user using MatSnackBar
      snackBar.open(errorMessage, 'Close', {
        duration: 5000,
        panelClass: ['error-snackbar'], // Optional: for custom styling
      });

      return throwError(() => new Error(errorMessage)); // Re-throw the error for other components to handle
    })
  );
};
