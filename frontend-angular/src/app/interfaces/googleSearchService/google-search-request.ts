export interface GoogleSearchRequest {
  /// <summary>
  /// Chuỗi tìm kiếm từ khóa (ví dụ: "học lập trình C#").
  /// Đây là tham số bắt buộc và không được để trống.
  /// </summary>
  q: string;
  /// <summary>
  /// Mã quốc gia (ví dụ: "vn" cho Việt Nam, "us" cho Hoa Kỳ).
  /// Tham số này có thể được sử dụng để xác định vị trí địa lý của người dùng và điều chỉnh kết quả tìm kiếm theo quốc gia cụ thể.
  /// </summary>
  /// <summary>
  ///
  /// </summary>
  /// <value>
  ///
  /// </value>
  gl: string;
  /// <summary>
  /// Vị trí tìm kiếm dưới dạng vĩ độ và kinh độ (ví dụ: "37.7749,-122.4194" cho San Francisco).
  /// </summary> <summary>
  ///
  /// </summary>
  /// <value></value>
  location: string;
  /// <summary>
  /// Ngôn ngữ của kết quả tìm kiếm (ví dụ: "en" cho tiếng Anh).
  /// </summary> <summary>
  ///
  /// </summary>
  /// <value></value>
  hl: string;
  /// <summary>
  /// Khoảng thời gian cho kết quả tìm kiếm (ví dụ: "w" cho tuần trước, "m" cho tháng trước).
  /// </summary> <summary>
  ///
  /// </summary>
  /// <value></value>
  tbs: string;
  /// <summary>
  /// Số lượng kết quả tìm kiếm để trả về (ví dụ: 10 cho 10 kết quả).
  /// </summary>
  /// <value></value>
  num: number;
  /// <summary>
  /// Loại tìm kiếm (ví dụ: "search" cho tìm kiếm thông thường).
  /// Tham số này có thể được sử dụng để xác định loại tìm kiếm mà người dùng đang thực hiện.
  /// </summary>
  /// <value></value>
  type: string;
  /// <summary>
  /// Vd: 'google', 'bing', 'yahoo', 'baidu', 'duckduckgo', 'yandex', 'ask', 'aol', 'ecosia', 'qwant'
  /// </summary> <summary>
  ///
  /// </summary>
  /// <value></value>
  engine: string;

  correctPhrase: string | null;
  anyWords: string | null;
  notWords: string | null;
  site: string | null;
}
