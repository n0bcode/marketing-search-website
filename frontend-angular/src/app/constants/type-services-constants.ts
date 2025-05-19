/**
 * Lớp chứa các hằng số cho các loại dịch vụ trong hệ thống.
 */
export class TypeServicesConstants {
  static readonly GoogleSearch = 'GoogleSearch';
  static readonly TwitterSearch = 'TwitterSearch';
  static readonly GeminiAI = 'GeminiAI';
  static readonly GoogleSender = 'GoogleSender';
  static readonly TwilioSender = 'TwilioSender';

  /**
   * Danh sách các dịch vụ đang được kích hoạt.
   */
  static readonly ActiveServices: string[] = [
    TypeServicesConstants.GoogleSearch,
    TypeServicesConstants.GeminiAI,
  ];

  /**
   * Mô tả và hướng dẫn lấy token cho từng dịch vụ.
   */
  static readonly ServiceDescriptions: Record<
    string,
    { description: string; guide: string }
  > = {
    [TypeServicesConstants.GoogleSearch]: {
      description: 'Tìm kiếm Google sử dụng API.',
      guide:
        'Truy cập https://console.cloud.google.com/apis/credentials để tạo API Key cho Google Search.',
    },
    [TypeServicesConstants.TwitterSearch]: {
      description: 'Tìm kiếm dữ liệu trên Twitter/X.',
      guide:
        'Đăng nhập https://developer.twitter.com/en/portal/projects-and-apps để lấy Bearer Token.',
    },
    [TypeServicesConstants.GeminiAI]: {
      description: 'Sử dụng AI Gemini để phân tích và sinh nội dung.',
      guide:
        'Truy cập https://aistudio.google.com/app/apikey để lấy Gemini API Key.',
    },
    [TypeServicesConstants.GoogleSender]: {
      description: 'Gửi email qua Google.',
      guide:
        'Tạo OAuth Client ID tại https://console.cloud.google.com/apis/credentials và lấy access token.',
    },
    [TypeServicesConstants.TwilioSender]: {
      description: 'Gửi tin nhắn SMS qua Twilio.',
      guide:
        'Đăng nhập https://www.twilio.com/console để lấy Account SID và Auth Token.',
    },
  };
}
