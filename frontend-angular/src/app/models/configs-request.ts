class ConfigsRequest {
  headers?: {
    skipAuth?: boolean; // Để xác định xem yêu cầu có cần xác thực hay không
    [key: string]: any; // Cho phép thêm các header khác nếu cần
  };
  static getSkipAuthConfig() {
    return { headers: { skipAuth: true } };
  }

  static takeAuth() {
    return { headers: { skipAuth: false } };
  }
  static formDataRequest() {
    return { headers: { 'Content-Type': 'multipart/form-data' } };
  }
}

export default ConfigsRequest;
