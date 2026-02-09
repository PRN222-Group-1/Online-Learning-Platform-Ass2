# Hướng dẫn Tích hợp Whisper AI

Tài liệu này hướng dẫn cách kết nối và sử dụng dịch vụ Whisper AI từ các ứng dụng khác, website, hoặc các AI Agent **thông qua mạng Tailscale**.

## 📡 Thông tin kết nối

| Thông tin | Giá trị |
| :--- | :--- |
| **Base URL** | `http://100.86.222.32:8000` |
| **Endpoint chính** | `/transcribe/` |
| **Giao thức** | HTTP (qua VPN Tailscale) |

> **Lưu ý:** Máy của bạn cần được kết nối vào cùng mạng Tailscale để truy cập được server.

---

## 1. Kiểm tra kết nối

Trước khi tích hợp, hãy kiểm tra xem bạn có thể kết nối được tới server:

```bash
curl http://100.86.222.32:8000/health
```

Nếu thành công, bạn sẽ nhận được:
```json
{"status": "ok", "model_loaded": true}
```

---

## 2. Sử dụng qua HTTP API (REST)

Đây là cách phổ biến nhất để tích hợp vào các ứng dụng web, mobile app hoặc automation scripts.

### Thông tin Endpoint

- **URL:** `POST http://100.86.222.32:8000/transcribe/`
- **Content-Type:** `multipart/form-data`

### Tham số (Body)

| Tên | Kiểu | Bắt buộc | Mặc định | Mô tả |
| :--- | :--- | :--- | :--- | :--- |
| `file` | File | Có | - | File âm thanh hoặc video cần chuyển đổi (mp3, wav, mp4, mkv...) |
| `summarize` | Boolean | Không | `true` | Có tạo tóm tắt nội dung bằng Groq AI hay không. |

### Ví dụ code

#### cURL (Command Line)

```bash
curl -X POST "http://100.86.222.32:8000/transcribe/" \
     -F "file=@/path/to/your/audio.mp3" \
     -F "summarize=true"
```

#### Python (Sử dụng `requests`)

```python
import requests

url = "http://100.86.222.32:8000/transcribe/"
file_path = "meeting_recording.mp3"

with open(file_path, "rb") as f:
    files = {"file": f}
    data = {"summarize": "true"}
    
    response = requests.post(url, files=files, data=data)
    
    if response.status_code == 200:
        result = response.json()
        print("Trạng thái:", result["status"])
        print("Ngôn ngữ:", result["data"]["language"])
        print("Nội dung:", result["data"]["text"])
        if "summary" in result["data"]:
            print("Tóm tắt:", result["data"]["summary"])
    else:
        print("Lỗi:", response.text)
```

#### JavaScript / Node.js (Axios)

```javascript
const axios = require('axios');
const fs = require('fs');
const FormData = require('form-data');

async function transcribe() {
  const form = new FormData();
  form.append('file', fs.createReadStream('video.mp4'));
  form.append('summarize', 'true');

  try {
    const response = await axios.post('http://100.86.222.32:8000/transcribe/', form, {
      headers: form.getHeaders()
    });
    console.log(response.data);
  } catch (error) {
    console.error(error);
  }
}
transcribe();
```
### Cấu trúc phản hồi (Response JSON)

```json
{
  "status": "success",
  "data": {
    "text": "Nội dung đầy đủ của đoạn ghi âm...",
    "language": "vi",
    "duration": 45.5,
    "segments": [...],
    "summary": "Tóm tắt ngắn gọn nội dung cuộc họp..." // Chỉ có nếu summarize=true
  }
}
```

---

## 3. Sử dụng qua Model Context Protocol (MCP)

Giao thức này dành cho việc tích hợp vào các AI Agent như **Claude Desktop**, **Cursor** hoặc các ứng dụng hỗ trợ MCP.

### Cài đặt

MCP Server của dự án này hoạt động như một "cầu nối", nhận lệnh từ AI và gọi đến API Server đang chạy.

**Cấu hình trong `claude_desktop_config.json`:**

```json
{
  "mcpServers": {
    "whisper-tailscale": {
      "command": "uvx",
      "args": ["mcp-proxy", "--sse-url", "http://100.86.222.32:8000/mcp/sse"]
    }
  }
}
```

Hoặc nếu bạn chạy MCP server cục bộ trên máy client:

```json
{
  "mcpServers": {
    "whisper-local": {
      "command": "/bin/bash",
      "args": [
        "-c",
        "source /path/to/venv/bin/activate && python /path/to/src/mcp/server.py"
      ],
      "env": {
        "WHISPER_API_URL": "http://100.86.222.32:8000/transcribe/"
      }
    }
  }
}
```

### Các công cụ (Tools) cung cấp

Sau khi kết nối, AI Agent sẽ có khả năng sử dụng công cụ:

- **`transcribe_media_file(file_path)`**: Đưa vào đường dẫn file tuyệt đối trên máy tính, AI sẽ trả về nội dung văn bản và bản tóm tắt.

---

## 4. Lưu ý quan trọng

- **Yêu cầu Tailscale:** Máy client cần kết nối vào mạng Tailscale để truy cập được server.
- **Timeout:** Do transcription có thể mất nhiều thời gian (10-15 phút cho file dài), hãy set timeout cao (khuyến nghị: 600 giây).
- **Xử lý tuần tự:** API xử lý từng file một để tiết kiệm RAM. Nếu gửi nhiều request cùng lúc, chúng sẽ được xếp hàng.
- **Kích thước file:** Đảm bảo file upload không quá lớn nếu đường truyền mạng yếu.

---

## 5. Xử lý sự cố

| Vấn đề | Giải pháp |
| :--- | :--- |
| Không kết nối được | Kiểm tra Tailscale đã kết nối chưa (`tailscale status`) |
| Timeout | Tăng timeout trong client (ít nhất 600s) |
| Model chưa sẵn sàng | Gọi `/health` để kiểm tra `model_loaded: true` |
| Lỗi 503 | Server đang khởi động, đợi 30-60 giây |