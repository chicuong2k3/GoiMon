# Email Setup Guide

Hướng dẫn cấu hình email để gửi OTP trong GoiMon.

---

## Cấu trúc config

```json
"Email": {
  "Host": "...",
  "Port": 587,
  "EnableSsl": true,
  "Username": "...",
  "Password": "...",
  "DisplayName": "GoiMon"
}
```

Config được đọc từ `appsettings.json`. Môi trường `Development` sẽ override bằng `appsettings.Development.json`.

---

## Option 1 — Mailpit (local, dùng khi dev)

Mailpit là SMTP server chạy local, bắt toàn bộ email và hiển thị qua web UI. **Không cần tài khoản, không cần password.**

### Cài đặt

**Docker (khuyến nghị):**
```bash
docker run -d --name mailpit \
  -p 1025:1025 \
  -p 8025:8025 \
  axllent/mailpit
```

**Binary (Linux):**
```bash
curl -sL https://raw.githubusercontent.com/axllent/mailpit/develop/install.sh | bash
mailpit
```

### Config (`appsettings.Development.json`)

```json
"Email": {
  "Host": "localhost",
  "Port": 1025,
  "EnableSsl": false,
  "Username": "",
  "Password": "",
  "DisplayName": "GoiMon Dev"
}
```

### Xem email

Mở trình duyệt: **http://localhost:8025**

---

## Option 2 — Hotmail / Outlook (production hoặc staging)

### Yêu cầu

Tài khoản `@hotmail.com` hoặc `@outlook.com` với **xác thực 2 bước đã bật**.

### Bước 1 — Bật xác thực 2 bước

1. Vào https://account.microsoft.com
2. **Security** → **Advanced security options**
3. Bật **Two-step verification**

### Bước 2 — Tạo App Password

1. Vào **Security** → **Advanced security options** → **App passwords**
2. Chọn **Create a new app password**
3. Đặt tên (ví dụ: `GoiMon`)
4. Copy chuỗi password được sinh ra (dạng `xxxx xxxx xxxx xxxx`)

### Bước 3 — Điền config

```json
"Email": {
  "Host": "smtp-mail.outlook.com",
  "Port": 587,
  "EnableSsl": true,
  "Username": "yourname@hotmail.com",
  "Password": "xxxx xxxx xxxx xxxx",
  "DisplayName": "GoiMon"
}
```

---

## Option 3 — Gmail

### Bước 1 — Bật xác thực 2 bước

1. Vào https://myaccount.google.com
2. **Security** → **2-Step Verification** → Bật

### Bước 2 — Tạo App Password

1. Tìm kiếm **"App passwords"** trong Google Account
2. Chọn app: **Mail**, device: **Other** → đặt tên `GoiMon`
3. Copy 16 ký tự được sinh ra

### Bước 3 — Điền config

```json
"Email": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "Username": "yourname@gmail.com",
  "Password": "xxxx xxxx xxxx xxxx",
  "DisplayName": "GoiMon"
}
```

---

## Bảo mật — Không commit password vào git

Dùng **user secrets** thay vì ghi thẳng vào file:

```bash
cd src/GoiMon.Api
dotnet user-secrets set "Email:Username" "yourname@hotmail.com"
dotnet user-secrets set "Email:Password" "xxxx xxxx xxxx xxxx"
```

User secrets được lưu ngoài repo tại `~/.microsoft/usersecrets/` và **không bị commit**.

Hoặc dùng **environment variables** khi deploy:

```bash
export Email__Host="smtp-mail.outlook.com"
export Email__Username="yourname@hotmail.com"
export Email__Password="xxxx xxxx xxxx xxxx"
```

---

## So sánh nhanh

| | Mailpit | Hotmail | Gmail |
|---|---|---|---|
| Dùng khi | Dev local | Staging / Prod | Staging / Prod |
| Cần tài khoản | Không | Có | Có |
| Email đến thật | Không (chặn local) | Có | Có |
| SSL | Không | Có | Có |
| Port | 1025 | 587 | 587 |
