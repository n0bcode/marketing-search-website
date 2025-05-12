import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = '';
      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Error: ${error.error.message}`;
      } else {
        // Server-side error
        errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
        // Thêm thông tin chi tiết từ response body nếu có
        if (error.error) {
          errorMessage += `\nDetails: ${JSON.stringify(error.error)}`;
        }
      }

      console.error(errorMessage); // Ghi log lỗi

      // Hiển thị thông báo cho người dùng (thay thế bằng service thông báo thực tế)
      // alert(errorMessage);

      return throwError(() => new Error(errorMessage)); // Ném lại lỗi để các components khác có thể xử lý
    })
  );
};
