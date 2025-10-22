# DBIntegrationHub API

Clean Architecture ile geliştirilmiş .NET 9.0 tabanlı modern bir API projesi.

## 🏗️ Mimari Yapı

Proje **Clean Architecture** prensiplerine göre 4 katmandan oluşmaktadır:

### 📦 Domain Layer (DBIntegrationHub.Domain)
- Uygulamanın çekirdeği
- Entity'ler, Value Object'ler, Domain Event'ler
- Repository Interface'leri
- Domain Exception'ları
- **Hiçbir dış bağımlılığı yok**

### 🎯 Application Layer (DBIntegrationHub.Application)
- CQRS Pattern (MediatR)
- Command ve Query Handler'lar
- FluentValidation ile doğrulama
- Abstractions (Cache, Email, Data)
- Pipeline Behaviors (Logging, Validation)

### 🔧 Infrastructure Layer (DBIntegrationHub.Infrastructure)
- Entity Framework Core (PostgreSQL)
- Repository implementasyonları
- Cache servisi (Distributed Memory Cache)
- Email servisi
- External servisler

### 🌐 Presentation Layer (DBIntegrationHub.Presentation)
- Web API Controllers
- Custom Middleware'ler
- Swagger/OpenAPI
- Dependency Injection yapılandırması

## 🚀 Teknolojiler

- **.NET 9.0**
- **Entity Framework Core 9.0** - ORM
- **PostgreSQL** - Veritabanı
- **MediatR** - CQRS Pattern
- **FluentValidation** - Doğrulama
- **Serilog** - Loglama
- **Swagger** - API Dokümantasyonu
- **Distributed Cache** - Önbellekleme

## 📁 Proje Yapısı

```
DBIntegrationHub.API/
├── DBIntegrationHub.Domain/
│   ├── DomainEvents/
│   ├── Entities/
│   ├── Exceptions/
│   │   ├── DomainException.cs
│   │   └── NotFoundException.cs
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Shared/
│   │   ├── Entity.cs
│   │   ├── ValueObject.cs
│   │   ├── IDomainEvent.cs
│   │   └── Result.cs
│   └── ValueObjects/
│
├── DBIntegrationHub.Application/
│   ├── Abstractions/
│   │   ├── Caching/
│   │   │   └── ICacheService.cs
│   │   ├── Data/
│   │   │   └── IApplicationDbContext.cs
│   │   ├── Email/
│   │   │   └── IEmailService.cs
│   │   └── Messaging/
│   │       ├── ICommand.cs
│   │       ├── ICommandHandler.cs
│   │       ├── IQuery.cs
│   │       └── IQueryHandler.cs
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   └── LoggingBehavior.cs
│   ├── Contracts/
│   └── DependencyInjection.cs
│
├── DBIntegrationHub.Infrastructure/
│   ├── Caching/
│   │   └── CacheService.cs
│   ├── Email/
│   │   └── EmailService.cs
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   ├── UnitOfWork.cs
│   │   └── Repositories/
│   │       └── Repository.cs
│   └── DependencyInjection.cs
│
└── DBIntegrationHub.Presentation/
    ├── Controllers/
    │   ├── ApiController.cs
    │   └── HealthController.cs
    ├── Middleware/
    │   ├── ExceptionHandlingMiddleware.cs
    │   └── RequestLoggingMiddleware.cs
    ├── Extensions/
    │   └── MiddlewareExtensions.cs
    ├── Program.cs
    └── appsettings.json
```

## ⚙️ Kurulum

### Gereksinimler
- .NET 9.0 SDK
- PostgreSQL

### Adımlar

1. **Veritabanı Bağlantısını Ayarlayın**
   
   `DBIntegrationHub.Presentation/appsettings.json` dosyasında connection string'i güncelleyin:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=DBIntegrationHub;Username=postgres;Password=your_password"
     }
   }
   ```

2. **Migration Oluşturun**
   ```bash
   cd DBIntegrationHub.Infrastructure
   dotnet ef migrations add InitialCreate --startup-project ../DBIntegrationHub.Presentation
   ```

3. **Veritabanını Güncelleyin**
   ```bash
   dotnet ef database update --startup-project ../DBIntegrationHub.Presentation
   ```

4. **Uygulamayı Çalıştırın**
   ```bash
   cd DBIntegrationHub.Presentation
   dotnet run
   ```

5. **Swagger'a Erişin**
   
   Tarayıcınızda: `https://localhost:5001/swagger` veya `http://localhost:5000/swagger`

## 🔑 Özellikler

### ✅ Validasyon
FluentValidation kullanarak otomatik model doğrulama:
```csharp
public class CreateCommandValidator : AbstractValidator<CreateCommand>
{
    public CreateCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

### 📝 Loglama
Serilog ile yapılandırılmış loglama:
- Console'a log
- Dosyaya log (`logs/log-{Date}.txt`)
- Request/Response loglama middleware

### 🗄️ Cache
Distributed Memory Cache implementasyonu:
```csharp
await _cacheService.SetAsync("key", data, TimeSpan.FromMinutes(5));
var cachedData = await _cacheService.GetAsync<MyData>("key");
```

### 🔄 CQRS Pattern
MediatR ile Command/Query ayrımı:
```csharp
// Command
public record CreateCommand(string Name) : ICommand<Guid>;

// Query
public record GetByIdQuery(Guid Id) : IQuery<MyDto>;
```

### 🛡️ Exception Handling
Global exception handling middleware ile merkezi hata yönetimi

## 📊 Health Check

API durumunu kontrol etmek için:
```
GET /api/health
```

Yanıt:
```json
{
  "status": "Healthy",
  "timestamp": "2025-10-22T12:00:00Z",
  "version": "1.0.0"
}
```

## 🔧 Yapılandırma

### Serilog
`appsettings.json` dosyasında log seviyelerini ayarlayabilirsiniz:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    }
  }
}
```

### CORS
Varsayılan olarak tüm origin'lere izin verilmektedir. Production'da bunu kısıtlayın:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## 🏃 Geliştirme

### Yeni Entity Ekleme
1. `Domain/Entities/` klasörüne entity ekleyin
2. `Domain/Repositories/` klasörüne repository interface'i ekleyin
3. `Infrastructure/Persistence/Repositories/` klasörüne implementasyon ekleyin
4. `Infrastructure/Persistence/EntityConfigurations/` klasörüne EF configuration ekleyin

### Yeni Feature Ekleme
1. `Application/` klasöründe entity için klasör oluşturun
2. `Commands/` ve `Queries/` klasörlerini oluşturun
3. Command/Query ve Handler'ları ekleyin
4. Validator'ları ekleyin
5. `Presentation/Controllers/` klasörüne controller ekleyin

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 👨‍💻 Geliştirici

DBIntegrationHub API - Clean Architecture Implementation

