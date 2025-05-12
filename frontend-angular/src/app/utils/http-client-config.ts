import { Injectable } from '@angular/core';
import {
  HttpClient,
  HttpHeaders,
  HttpErrorResponse,
} from '@angular/common/http';
import { catchError, Observable, throwError, map } from 'rxjs';
import { environment } from '../../environments/environment'; // Đảm bảo rằng bạn đã định nghĩa biến môi trường này
import ConfigsRequest from '../models/configs-request'; // Đảm bảo rằng bạn đã định nghĩa ConfigsRequest
import { TokenConstants } from '../constants/token-constants';
@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private apiUrl = environment.apiUrl; // Sửa lỗi chính tả từ 'enviroment' thành 'environment'

  constructor(private http: HttpClient) {}

  private handleError(error: HttpErrorResponse) {
    // Xử lý lỗi tại đây
    console.error('API Error:', error);
    return throwError(error);
  }

  // Hàm GET
  getFromApi<T>(
    url: string,
    config: ConfigsRequest = ConfigsRequest.getSkipAuthConfig(),
    returnAsResponseAPI: boolean = false // Tham số để xác định có trả về ResponseAPI hay không
  ) {
    return this.http
      .get<T>(`${this.apiUrl}/api${url}`, {
        headers: this.createHeaders(config),
      })
      .pipe(
        catchError(this.handleError),
        map(
          (data) => data
          // returnAsResponseAPI ? this.mapToResponseAPI<T>(data) : data
        ) // Chuyển đổi dữ liệu thành ResponseAPI nếu cần
      );
  }

  // Hàm POST
  postToApi<T>(
    url: string,
    data: any,
    config: ConfigsRequest = ConfigsRequest.getSkipAuthConfig()
  ) {
    return this.http
      .post<T>(`${this.apiUrl}/api${url}`, data, {
        headers: this.createHeaders(config),
      })
      .pipe(
        catchError(this.handleError),
        map(
          (data) => data
          // returnAsResponseAPI ? this.mapToResponseAPI<T>(data) : data
        ) // Chuyển đổi dữ liệu thành ResponseAPI nếu cần
      );
  }

  // Hàm PUT
  putToApi<T>(
    url: string,
    data: any,
    config: ConfigsRequest = ConfigsRequest.getSkipAuthConfig(),
    returnAsResponseAPI: boolean = false // Tham số để xác định có trả về ResponseAPI hay không
  ) {
    return this.http
      .put<T>(`${this.apiUrl}/api${url}`, data, {
        headers: this.createHeaders(config),
      })
      .pipe(
        catchError(this.handleError),
        map(
          (data) => data
          // returnAsResponseAPI ? this.mapToResponseAPI<T>(data) : data
        ) // Chuyển đổi dữ liệu thành ResponseAPI nếu cần
      );
  }

  // Hàm PATCH
  patchToApi<T>(
    url: string,
    data: any,
    config: ConfigsRequest = ConfigsRequest.getSkipAuthConfig(),
    returnAsResponseAPI: boolean = false // Tham số để xác định có trả về ResponseAPI hay không
  ) {
    return this.http
      .patch<T>(`${this.apiUrl}/api${url}`, data, {
        headers: this.createHeaders(config),
      })
      .pipe(
        catchError(this.handleError),
        map(
          (data) => data
          // returnAsResponseAPI ? this.mapToResponseAPI<T>(data) : data
        ) // Chuyển đổi dữ liệu thành ResponseAPI nếu cần
      );
  }

  // Hàm DELETE
  deleteFromApi<T>(
    url: string,
    config: ConfigsRequest = ConfigsRequest.getSkipAuthConfig(),
    returnAsResponseAPI: boolean = false // Tham số để xác định có trả về ResponseAPI hay không
  ) {
    return this.http
      .delete<T>(`${this.apiUrl}/api${url}`, {
        headers: this.createHeaders(config),
      })
      .pipe(
        catchError(this.handleError),
        map(
          (data) => data
          // returnAsResponseAPI ? this.mapToResponseAPI<T>(data) : data
        ) // Chuyển đổi dữ liệu thành ResponseAPI nếu cần
      );
  }

  private createHeaders(config: ConfigsRequest): HttpHeaders {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    // Thêm Authorization header nếu cần
    if (!config.headers?.skipAuth) {
      const accessToken = this.getAccessToken(); // Hàm lấy access token từ cookie hoặc local storage
      if (accessToken) {
        headers = headers.set('Authorization', `Bearer ${accessToken}`);
      }
    }

    return headers;
  }

  private getAccessToken(): string | null {
    // Lấy access token từ cookie hoặc local storage
    return localStorage.getItem(TokenConstants.accessToken) || null; // Hoặc sử dụng Cookies nếu bạn đã cài đặt
  }

  // private mapToResponseAPI<T>(data: T): ResponseAPI<T> {
  //   // Chuyển đổi dữ liệu thành ResponseAPI
  //   return new ResponseAPI<T>(data); // Giả sử ResponseAPI có constructor nhận dữ liệu
  // }
}
