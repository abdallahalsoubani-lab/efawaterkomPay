# DirectPay Payment Gateway

A complete payment gateway integration with eFAWATEERcom DirectPay system for Jordan. Production-ready with only configuration values needed to go live.

## Features

- **Payment Processing**: Full DirectPay integration with eFAWATEERcom
- **User Authentication**: JWT-based authentication with refresh tokens
- **Role-Based Access Control**: Admin, User, and Merchant roles
- **Transaction Management**: Complete transaction history and tracking
- **Admin Dashboard**: Statistics, user management, and transaction exports
- **BCrypt Security**: Secure hash validation per eFAWATEERcom specs
- **Rate Limiting**: Protection against abuse
- **Comprehensive Logging**: Audit trails for all transactions

## Tech Stack

- **Backend**: .NET Core 9 Web API
- **Frontend**: React 18 with TypeScript
- **Database**: SQL Server with EF Core (Code First)
- **Authentication**: ASP.NET Core Identity with JWT
- **Build Tool**: Webpack 5

## Prerequisites

- .NET 9 SDK
- Node.js 18+
- SQL Server 2019+

## Quick Start

### 1. Clone and Configure

```bash
git clone <repository-url>
cd efawaterkomPay
```

### 2. Update Configuration

Edit `src/API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DirectPayGateway;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "Secret": "YOUR_JWT_SECRET_KEY_MIN_32_CHARS_HERE_CHANGE_IN_PRODUCTION"
  },
  "DirectPay": {
    "BillerCode": "YOUR_BILLER_CODE_HERE",
    "ServiceCode": "YOUR_SERVICE_CODE_HERE",
    "SecretToken": "YOUR_SECRET_TOKEN_HERE",
    "CallbackUrl": "https://yourdomain.com/api/payment/callback",
    "Environment": "Staging"
  }
}
```

### 3. Run the Backend

```bash
cd src/API
dotnet restore
dotnet run
```

The API will start at `https://localhost:5001` (or `http://localhost:5000`).

### 4. Run the Frontend (Development)

```bash
cd src/Web
npm install
npm start
```

The frontend will start at `http://localhost:3000`.

### 5. Build for Production

```bash
# Build frontend (outputs to src/API/wwwroot)
cd src/Web
npm run build

# Build and run API
cd src/API
dotnet publish -c Release
```

## Database Migration

The database is created automatically on first run. To create migrations manually:

```bash
cd src/API
dotnet ef migrations add InitialCreate -p ../Infrastructure -o Data/Migrations
dotnet ef database update
```

## Default Admin Account

On first run, the system creates an admin user:

- **Email**: admin@admin.com
- **Password**: Admin123!

**Important**: Change this password immediately in production!

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | User login |
| POST | `/api/auth/refresh-token` | Refresh JWT token |
| GET | `/api/auth/me` | Get current user |
| POST | `/api/auth/change-password` | Change password |
| POST | `/api/auth/logout` | Logout |

### Payments
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/payment/initiate` | Start payment (returns redirect URL) |
| GET | `/api/payment/callback` | DirectPay callback handler |
| GET | `/api/payment/{id}` | Get transaction details |
| GET | `/api/payment/status/{billerTrxNo}` | Check transaction status |
| GET | `/api/payment/history` | User transaction history |

### Admin (Requires Admin Role)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/dashboard` | Dashboard statistics |
| GET | `/api/admin/transactions` | All transactions |
| GET | `/api/admin/transactions/export` | Export to CSV |
| GET | `/api/admin/users` | User management |
| PUT | `/api/admin/users/{id}` | Update user |
| PUT | `/api/admin/users/{id}/roles` | Update user roles |

## Payment Flow

1. User fills payment form on frontend
2. Frontend calls `POST /api/payment/initiate`
3. Backend creates transaction and returns DirectPay redirect URL
4. User is redirected to eFAWATEERcom DirectPay page
5. User completes payment on DirectPay
6. DirectPay redirects to `GET /api/payment/callback` with response
7. Backend validates hash and updates transaction status
8. User is redirected to result page

## DirectPay Error Codes

| Code | English | Arabic |
|------|---------|--------|
| 1 | Success | نجاح |
| 2 | Wrong Biller Transaction No | رقم معاملة خاطئ |
| 3 | Wrong Biller Code | رمز المفوتر خاطئ |
| 4 | Wrong Service Code | رمز الخدمة خاطئ |
| 5 | Wrong Prepaid Category Code | رمز فئة الدفع المسبق خاطئ |
| 6 | Wrong Amount | المبلغ خاطئ |
| 7 | Wrong Customer Email | البريد الإلكتروني خاطئ |
| 8 | Wrong Call Back URL | رابط الاستجابة خاطئ |
| 9 | Parsing Error | خطأ في تحليل البيانات |
| 10 | Internal Error | خطأ داخلي |
| 11 | Unable to process payment | تعذر معالجة الدفع |
| 12 | Payment canceled by customer | تم إلغاء العملية من قبل العميل |
| 13 | Insufficient balance | رصيد غير كافٍ |
| 14 | No registered mobile number | لا يوجد رقم جوال مسجل |
| 15 | Unable to process (bank issue) | تعذر معالجة الدفع |
| 16 | Incorrect OTP | رمز التحقق غير صحيح |
| 17 | Expired OTP | رمز التحقق منتهي الصلاحية |
| 18 | Max OTP requests reached | تجاوزت الحد الأقصى لطلب رمز تحقق |
| 19 | Bank not available | البنك غير متاح |
| 20 | Invalid Token | رمز غير صالح |

## Production Checklist

- [ ] Change JWT secret to a strong random value (min 32 chars)
- [ ] Update DirectPay credentials (BillerCode, ServiceCode, SecretToken)
- [ ] Change DirectPay environment to "Production"
- [ ] Update CallbackUrl to your production domain
- [ ] Whitelist your domain with eFAWATEERcom
- [ ] Change default admin password
- [ ] Enable HTTPS
- [ ] Update CORS settings for your domain
- [ ] Set up SSL certificate
- [ ] Configure SQL Server authentication
- [ ] Set up database backups
- [ ] Configure logging/monitoring

## Docker Support

```bash
# Start SQL Server
docker-compose up -d

# Connection string for Docker SQL Server
Server=localhost,1433;Database=DirectPayGateway;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;
```

## Security Features

- BCrypt hash validation for all DirectPay responses
- JWT token authentication with refresh tokens
- Rate limiting on sensitive endpoints
- Input validation (no special characters: ~ " ' & # %)
- SQL injection prevention via EF Core parameterized queries
- XSS prevention in React
- CORS configuration
- Audit logging for all transactions

## Project Structure

```
/efawaterkomPay
├── /src
│   ├── /API                    # ASP.NET Core Web API
│   │   ├── /Controllers        # API endpoints
│   │   ├── /Middleware         # Exception handling, logging
│   │   ├── /Extensions         # Helper extensions
│   │   └── Program.cs          # Application entry point
│   ├── /Core                   # Domain & business logic
│   │   ├── /Entities           # Domain models
│   │   ├── /Interfaces         # Service contracts
│   │   ├── /Services           # Business logic
│   │   ├── /DTOs               # Data transfer objects
│   │   ├── /Exceptions         # Custom exceptions
│   │   └── /Constants          # Error codes, roles
│   ├── /Infrastructure         # External concerns
│   │   ├── /Data               # EF Core DbContext
│   │   ├── /Repositories       # Data access
│   │   └── /ExternalServices   # DirectPay integration
│   └── /Web                    # React frontend
│       ├── /src
│       │   ├── /components     # Reusable components
│       │   ├── /pages          # Page components
│       │   ├── /services       # API clients
│       │   ├── /context        # React context (auth)
│       │   └── /hooks          # Custom hooks
│       ├── webpack.config.js
│       └── package.json
└── /tests                      # Unit/integration tests
```

## License

MIT

## Support

For issues with DirectPay integration, contact eFAWATEERcom support.
For application issues, open a GitHub issue.
