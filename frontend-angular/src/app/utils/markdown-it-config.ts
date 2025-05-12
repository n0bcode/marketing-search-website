import MarkdownIt from 'markdown-it';
export class MarkdownItConfig {
  static formatMessageMarkToHtml(message: string): string {
    // Khởi tạo MarkdownIt
    const md = new MarkdownIt({
      html: true, // cho phép HTML
      linkify: true, // tự động chuyển đổi URL thành liên kết
      typographer: true, // sử dụng các quy tắc định dạng
      breaks: true,
    });

    return md.render(message);
  }
}
