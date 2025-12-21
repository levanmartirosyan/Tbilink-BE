📌 Tbilink Backend

Tbilink Backend is the backend service for the Tbilink Social Media platform, built with ASP.NET Core Web API and MS SQL Server.
It provides all REST API endpoints for authentication, user management, posts, comments, messaging, notifications, storage, search, and admin moderation.

🚀 Tech Stack

ASP.NET Core Web API (C#)

Entity Framework Core

Supabase PostgreSQL

JWT Authentication

Swagger / Swashbuckle (API documentation)

⚙️ Getting Started
1️⃣ Clone the Repository
```bash
git clone https://github.com/levanmartirosyan/Tbilink-Back.git
cd Tbilink-Back
```

2️⃣ Configure Database

Update the connection string in appsettings.json:
```bash
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "JwtSettings": {
    "SecretKey": "",
    "Issuer": "Tbilink.WebApi",
    "Audience": "Tbilink.Client",
    "ExpiryMinutes": 1440
  },
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "User": "",
    "Password": "",
    "EnableSsl": true
  },
  "SupabaseStorage": {
    "Url": "",
    "ServiceRoleKey": "",
    "Buckets": {
      "PublicBucket": "",
      "PrivateBucket": ""
    }
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200"
    ]
  },
```

Apply migrations:
```bash
dotnet ef database update
```

3️⃣ Run the API
```bash
dotnet run
```

Backend will be available at:

https://localhost:7292

📖 Swagger API Docs

Swagger UI is enabled in Development mode.

After starting the API, visit:

👉 https://localhost:7292/swagger

You can explore and test all endpoints directly from the browser.

📂 Project Structure
```bash
Tbilink-BE/
│── Application/        # Business logic
│── Domain/             # Entity models
│── Infrastructure/     # DbContext & migrations
│── WebApi/             # Api Controllers
```
🔌 API Endpoints

Base URL: /api
Total Endpoints: 51
🔐 Protected endpoints require Authorization: Bearer <token>

<details> <summary><strong>🔐 Auth</strong> <code>/api/auth</code></summary>

POST /register – Register new user

POST /login – User login

POST /send-verification-code – Send email verification code

POST /verify-email – Verify email

POST /reset-password – Reset password

</details>
<details> <summary><strong>👤 Users</strong> <code>/api/users</code></summary>

GET / – Get all users

GET /{username} – Get user by username

PUT /profile – Update profile

POST /follow/{userId} – Follow user

POST /unfollow/{userId} – Unfollow user

POST /change-password – Change password

GET /{id}/followers – Get followers

GET /{id}/following – Get following

GET /{id}/likes – Liked posts

GET /{id}/posts – User posts

GET /{id}/saved – Saved posts

</details>
<details> <summary><strong>📝 Posts</strong> <code>/api/posts</code></summary>

GET /all – Get all posts

GET /{id} – Get post by ID

GET /user/{userId} – Posts by user

POST / – Create post

PUT /{id} – Update post

DELETE /{id} – Delete post

POST /{postId}/like – Like / Unlike post

POST /{postId}/comments – Add comment

PUT /comments/{commentId} – Update comment

DELETE /comments/{commentId} – Delete comment

GET /{id}/comments – Get comments

POST /report – Report post

</details>
<details> <summary><strong>💬 Messages</strong> <code>/api/messages</code></summary>

GET /chats – Get user chats

GET /chat/{chatId} – Get chat messages

POST /send – Send message

GET /unread – Unread messages

GET /{id} – Get message by ID

DELETE /{id} – Delete message

</details>
<details> <summary><strong>🔔 Notifications</strong> <code>/api/notifications</code></summary>

GET /user/{userId} – Get user notifications

</details>
<details> <summary><strong>🗂 Storage</strong> <code>/api/storage</code></summary>

POST /upload/public – Upload public file

POST /upload/private – Upload private file

DELETE /delete – Delete file

GET /signed-url – Generate signed URL

</details>
<details> <summary><strong>🔍 Search</strong> <code>/api/search</code></summary>

GET / – Global search (users, posts)

</details>
<details> <summary><strong>🛡 Admin</strong> <code>/api/admin</code></summary>

GET /users – Get all users

GET /users/{id} – Get user details

GET /posts – Get all posts

POST /ban-user/{id} – Ban user

POST /unban-user/{id} – Unban user

PUT /users/{id} – Update user

DELETE /users/{id} – Delete user

POST /restore/{id} – Restore user

GET /statistics – Platform statistics

</details>
🔒 Authorization Rules

🔐 JWT authentication required for protected routes

👑 Admin role required for /api/admin/*

👤 Users can only modify their own data

🧪 Testing

You can test endpoints using:

Swagger UI

Postman

Example:

POST /api/auth/signin
GET  /api/posts/all
POST /api/posts/{id}/like
