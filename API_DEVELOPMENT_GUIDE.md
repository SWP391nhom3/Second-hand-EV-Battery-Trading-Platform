# Hướng dẫn Quy trình Tạo API trong EVehicle Backend

## Tổng quan

Tài liệu này mô tả quy trình chuẩn để tạo một API endpoint mới trong hệ thống EVehicle, tuân thủ Clean Architecture và SOLID principles.

## Kiến trúc Project

```
EVehicle/
├── EVehicle.Domain/          # Domain Layer (Entities, Domain Models)
├── EVehicle.Application/     # Application Layer (Business Logic, Use Cases)
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Validators/           # FluentValidation Validators
│   ├── Interfaces/           # Application Interfaces
│   └── Services/             # Application Services
├── EVehicle.Infrastructure/  # Infrastructure Layer (Data Access, External Services)
│   ├── Repositories/         # Repository Implementations
│   └── Services/             # Infrastructure Services
└── EVehicle.API/             # Presentation Layer (Controllers, Middleware)
    └── Controllers/          # API Controllers
```

## Quy trình Tạo API (Step-by-Step)

### Bước 1: Phân tích Use Case (UC)

Trước khi code, đọc và hiểu rõ Use Case từ tài liệu `UC_Overview.md`:
- Xác định Actor (ai sử dụng API)
- Xác định Input/Output
- Xác định Business Rules
- Xác định Validation Rules

**Ví dụ**: UC06 - Tạo Bài đăng mới
- Actor: Member (Người bán)
- Input: Thông tin bài đăng (title, description, price, images, ...)
- Output: Thông tin bài đăng đã tạo
- Business Rules: 
  - Phải chọn gói tin (Basic, Premium, Luxury)
  - Kiểm tra credits còn lại
  - Bài đăng được lưu với status PENDING
  - Chưa trừ credits tại thời điểm này (sẽ trừ khi Admin duyệt)

---

### Bước 2: Tạo DTOs (Data Transfer Objects)

**Location**: `EVehicle.Application/DTOs/{Module}/`

Tạo các DTOs cần thiết:
1. **Request DTO**: Dữ liệu đầu vào từ client
2. **Response DTO**: Dữ liệu trả về cho client
3. Sử dụng **BaseResponse<T>** hoặc **PagedResponse<T>** cho response

**Ví dụ**: Tạo bài đăng

```csharp
// PostCreateRequest.cs
namespace EVehicle.Application.DTOs.Posts;

public class PostCreateRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal BatteryCapacityCurrent { get; set; }
    public int? ChargeCount { get; set; }
    public int ProductionYear { get; set; }
    public string Condition { get; set; } = string.Empty;
    public int? Mileage { get; set; }
    public List<IFormFile> Images { get; set; } = new();
    public int PackageId { get; set; }
}
```

```csharp
// PostResponse.cs
namespace EVehicle.Application.DTOs.Posts;

public class PostResponse
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}
```

**Best Practices**:
- Đặt tên rõ ràng: `{Entity}{Action}Request`, `{Entity}Response`
- Sử dụng `BaseResponse<T>` cho success/error handling
- Sử dụng `PagedResponse<T>` cho phân trang
- Thêm XML comments cho documentation

---

### Bước 3: Tạo Validators (FluentValidation)

**Location**: `EVehicle.Application/Validators/{Module}/`

Tạo validator cho Request DTOs sử dụng FluentValidation:

```csharp
// PostCreateRequestValidator.cs
using EVehicle.Application.DTOs.Posts;
using FluentValidation;

namespace EVehicle.Application.Validators.Posts;

public class PostCreateRequestValidator : AbstractValidator<PostCreateRequest>
{
    public PostCreateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .MaximumLength(255).WithMessage("Tiêu đề không được vượt quá 255 ký tự");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả không được để trống")
            .MaximumLength(5000).WithMessage("Mô tả không được vượt quá 5000 ký tự");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Giá phải lớn hơn 0");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Danh mục không hợp lệ");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Thương hiệu không được để trống");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model không được để trống");

        RuleFor(x => x.ProductionYear)
            .InclusiveBetween(2000, DateTime.Now.Year)
            .WithMessage($"Năm sản xuất phải từ 2000 đến {DateTime.Now.Year}");

        RuleFor(x => x.Images)
            .NotEmpty().WithMessage("Phải có ít nhất 1 ảnh")
            .Must(images => images.Count <= 10).WithMessage("Tối đa 10 ảnh");

        RuleFor(x => x.PackageId)
            .GreaterThan(0).WithMessage("Gói tin không hợp lệ");
    }
}
```

**Best Practices**:
- Validate tất cả fields bắt buộc
- Validate format (email, phone, url, ...)
- Validate range (min, max, length, ...)
- Thêm message rõ ràng bằng tiếng Việt
- Validate business rules phức tạp trong Service layer

---

### Bước 4: Tạo Application Interfaces

**Location**: `EVehicle.Application/Interfaces/`

Tạo interface cho Application Service:

```csharp
// IPostService.cs
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;

namespace EVehicle.Application.Interfaces;

public interface IPostService
{
    Task<BaseResponse<PostResponse>> CreatePostAsync(
        Guid userId, 
        PostCreateRequest request);
    
    Task<PagedResponse<PostResponse>> GetPostsAsync(
        PostSearchRequest request);
    
    Task<BaseResponse<PostResponse>> GetPostByIdAsync(Guid postId);
    
    Task<BaseResponse<PostResponse>> UpdatePostAsync(
        Guid userId, 
        Guid postId, 
        PostUpdateRequest request);
    
    Task<BaseResponse> DeletePostAsync(Guid userId, Guid postId);
}
```

**Best Practices**:
- Đặt tên interface rõ ràng: `I{Entity}Service`
- Sử dụng async/await cho tất cả methods
- Sử dụng BaseResponse<T> hoặc PagedResponse<T>
- Thêm XML comments

---

### Bước 5: Tạo Repository Interface (nếu cần)

**Location**: `EVehicle.Application/Interfaces/`

Tạo interface cho Repository (nếu chưa có):

```csharp
// IPostRepository.cs
using EVehicle.Domain.Entities;
using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid postId);
    Task<Post?> GetByIdWithImagesAsync(Guid postId);
    Task<PagedResult<Post>> GetPostsAsync(
        int pageNumber, 
        int pageSize, 
        string? sortBy, 
        string? sortDirection);
    Task<Post> CreateAsync(Post post);
    Task<Post> UpdateAsync(Post post);
    Task DeleteAsync(Post post);
    Task<bool> ExistsAsync(Guid postId);
    Task<int> GetTotalCountAsync();
}
```

**Best Practices**:
- Repository chỉ làm việc với Domain Entities
- Không có business logic trong Repository
- Sử dụng async/await
- Trả về Domain Entities, không phải DTOs

---

### Bước 6: Implement Repository

**Location**: `EVehicle.Infrastructure/Repositories/`

Implement repository interface:

```csharp
// PostRepository.cs
using EVehicle.Application.Interfaces;
using EVehicle.Application.DTOs.Common;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly EVehicleDbContext _context;

    public PostRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetByIdAsync(Guid postId)
    {
        return await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<Post?> GetByIdWithImagesAsync(Guid postId)
    {
        return await _context.Posts
            .AsNoTracking()
            .Include(p => p.PostImages)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<PagedResult<Post>> GetPostsAsync(
        int pageNumber, 
        int pageSize, 
        string? sortBy, 
        string? sortDirection)
    {
        var query = _context.Posts.AsNoTracking();

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sortBy))
                : query.OrderBy(GetSortProperty(sortBy));
        }
        else
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<Post>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Post> CreateAsync(Post post)
    {
        await _context.Posts.AddAsync(post);
        return post;
    }

    public async Task<Post> UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        return await Task.FromResult(post);
    }

    public async Task DeleteAsync(Post post)
    {
        _context.Posts.Remove(post);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid postId)
    {
        return await _context.Posts.AnyAsync(p => p.Id == postId);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Posts.CountAsync();
    }

    private static System.Linq.Expressions.Expression<Func<Post, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "price" => p => p.Price,
            "createdat" => p => p.CreatedAt,
            "title" => p => p.Title,
            _ => p => p.CreatedAt
        };
    }
}
```

**Best Practices**:
- Sử dụng `AsNoTracking()` cho read operations
- Sử dụng `Include()` để eager load related entities
- Implement sorting, filtering, paging
- Handle exceptions properly

---

### Bước 7: Implement Application Service

**Location**: `EVehicle.Application/Services/`

Implement application service với business logic:

```csharp
// PostService.cs
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PostService> _logger;

    public PostService(
        IPostRepository postRepository,
        IUserRepository userRepository,
        IPackageRepository packageRepository,
        IFileStorageService fileStorageService,
        ILogger<PostService> logger)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _packageRepository = packageRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<BaseResponse<PostResponse>> CreatePostAsync(
        Guid userId, 
        PostCreateRequest request)
    {
        try
        {
            // 1. Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Người dùng không tồn tại");
            }

            // 2. Validate package exists and check credits
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Gói tin không tồn tại");
            }

            var userCredits = await _userRepository.GetPackageCreditsAsync(
                userId, 
                request.PackageId);
            if (userCredits == null || userCredits.CreditsRemaining <= 0)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Không đủ credits cho gói tin này");
            }

            // 3. Upload images
            var imageUrls = new List<string>();
            foreach (var image in request.Images)
            {
                var imageUrl = await _fileStorageService.UploadImageAsync(image);
                imageUrls.Add(imageUrl);
            }

            // 4. Create post entity
            var post = new Post
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CategoryId = request.CategoryId,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Location = request.Location,
                Brand = request.Brand,
                Model = request.Model,
                BatteryCapacityCurrent = request.BatteryCapacityCurrent,
                ChargeCount = request.ChargeCount,
                ProductionYear = request.ProductionYear,
                Condition = request.Condition,
                Mileage = request.Mileage,
                Status = "PENDING",
                IsActive = true,
                IsSold = false,
                CreatedAt = DateTime.UtcNow
            };

            // 5. Create post images
            foreach (var imageUrl in imageUrls)
            {
                post.PostImages.Add(new PostImage
                {
                    Id = Guid.NewGuid(),
                    PostId = post.Id,
                    ImageUrl = imageUrl,
                    IsThumbnail = imageUrls.IndexOf(imageUrl) == 0,
                    IsProof = false,
                    DisplayOrder = imageUrls.IndexOf(imageUrl)
                });
            }

            // 6. Save to database
            await _postRepository.CreateAsync(post);
            await _postRepository.SaveChangesAsync();

            _logger.LogInformation("Tạo bài đăng thành công, PostId: {PostId}, UserId: {UserId}", 
                post.Id, userId);

            // 7. Map to response
            var response = MapToResponse(post);

            return BaseResponse<PostResponse>.SuccessResponse(
                response, 
                "Tạo bài đăng thành công. Bài đăng đang chờ duyệt.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo bài đăng, UserId: {UserId}", userId);
            return BaseResponse<PostResponse>.FailureResponse(
                ex, 
                "Đã xảy ra lỗi khi tạo bài đăng");
        }
    }

    public async Task<PagedResponse<PostResponse>> GetPostsAsync(
        PostSearchRequest request)
    {
        try
        {
            request.IsValid();

            var result = await _postRepository.GetPostsAsync(
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDirection);

            var responses = result.Items.Select(MapToResponse).ToList();

            return PagedResponse<PostResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy danh sách bài đăng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài đăng");
            return PagedResponse<PostResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài đăng");
        }
    }

    // ... Other methods

    private PostResponse MapToResponse(Post post)
    {
        return new PostResponse
        {
            PostId = post.Id,
            UserId = post.UserId,
            CategoryId = post.CategoryId,
            Title = post.Title,
            Description = post.Description,
            Price = post.Price,
            Status = post.Status,
            CreatedAt = post.CreatedAt,
            ImageUrls = post.PostImages
                .OrderBy(img => img.DisplayOrder)
                .Select(img => img.ImageUrl)
                .ToList()
        };
    }
}
```

**Best Practices**:
- Implement business logic trong Service
- Validate business rules
- Log các thao tác quan trọng
- Handle exceptions properly
- Sử dụng BaseResponse<T> hoặc PagedResponse<T>
- Map Entity sang DTO (không trả về Entity trực tiếp)

---

### Bước 8: Đăng ký Services trong DI Container

**Location**: `EVehicle.Application/ApplicationExtensions.cs` và `EVehicle.Infrastructure/InfrastructureExtensions.cs`

```csharp
// ApplicationExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // Application Services
    services.AddScoped<IPostService, PostService>();
    
    // Validators
    services.AddValidatorsFromAssemblyContaining<PostCreateRequestValidator>();
    
    return services;
}

// InfrastructureExtensions.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // Repositories
    services.AddScoped<IPostRepository, PostRepository>();
    
    return services;
}
```

---

### Bước 9: Tạo Controller

**Location**: `EVehicle.API/Controllers/`

Tạo controller với endpoints:

```csharp
// PostsController.cs
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý bài đăng
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Posts")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IValidator<PostCreateRequest> _createValidator;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        IPostService postService,
        IValidator<PostCreateRequest> createValidator,
        ILogger<PostsController> logger)
    {
        _postService = postService;
        _createValidator = createValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC06: Tạo bài đăng mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<PostResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePost([FromForm] PostCreateRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<PostResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Validate request
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<PostResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _postService.CreatePostAsync(userId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetPostById),
                    new { id = response.Data?.PostId },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo bài đăng");
            return StatusCode(500, BaseResponse<PostResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo bài đăng"));
        }
    }

    /// <summary>
    /// Lấy danh sách bài đăng (có phân trang)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponse<PostResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPosts([FromQuery] PostSearchRequest request)
    {
        try
        {
            var response = await _postService.GetPostsAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài đăng");
            return StatusCode(500, PagedResponse<PostResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài đăng"));
        }
    }

    // Helper method
    private Guid? GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
```

**Best Practices**:
- Thin controllers: chỉ xử lý HTTP requests/responses
- Validate request với FluentValidation
- Get userId từ JWT token
- Sử dụng BaseResponse<T> hoặc PagedResponse<T>
- Log errors
- Thêm XML comments cho Swagger
- Sử dụng proper HTTP status codes
- Sử dụng [Authorize] cho protected endpoints

---

### Bước 10: Cấu hình Swagger Documentation

Swagger đã được cấu hình trong `Program.cs`. Đảm bảo:
- Thêm XML comments cho controller và methods
- Sử dụng `[ProducesResponseType]` attributes
- Sử dụng `[Tags]` để group endpoints
- Thêm `[Produces]` attribute

---

### Bước 11: Testing

1. **Test với Swagger UI**:
   - Chạy application: `dotnet run --project src/EVehicle.API`
   - Mở Swagger UI: `https://localhost:5001/swagger`
   - Test các endpoints

2. **Test Validation**:
   - Test với dữ liệu không hợp lệ
   - Test với dữ liệu hợp lệ

3. **Test Business Logic**:
   - Test các business rules
   - Test error cases

4. **Test Authentication**:
   - Test với token hợp lệ
   - Test với token không hợp lệ
   - Test với token hết hạn

---

## Checklist Tạo API

- [ ] Đã đọc và hiểu Use Case
- [ ] Đã tạo Request/Response DTOs
- [ ] Đã tạo Validators với FluentValidation
- [ ] Đã tạo Application Interface
- [ ] Đã tạo Repository Interface (nếu cần)
- [ ] Đã implement Repository
- [ ] Đã implement Application Service
- [ ] Đã đăng ký Services trong DI Container
- [ ] Đã tạo Controller
- [ ] Đã thêm XML comments cho Swagger
- [ ] Đã test với Swagger UI
- [ ] Đã test validation
- [ ] Đã test business logic
- [ ] Đã test authentication/authorization

---

## Common DTOs

### BaseResponse<T>

Sử dụng cho tất cả API responses:

```csharp
var response = BaseResponse<PostResponse>.SuccessResponse(
    postResponse, 
    "Tạo bài đăng thành công");

// Hoặc
var response = BaseResponse<PostResponse>.FailureResponse(
    "Lỗi xảy ra",
    new List<string> { "Error 1", "Error 2" });
```

### PagedResponse<T>

Sử dụng cho APIs có phân trang:

```csharp
var response = PagedResponse<PostResponse>.SuccessResponse(
    posts,
    pageNumber: 1,
    pageSize: 10,
    totalCount: 100,
    "Lấy danh sách thành công");
```

### PagedRequest

Sử dụng cho request có phân trang:

```csharp
public class PostSearchRequest : PagedRequest
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

---

## Best Practices

### 1. Naming Conventions
- **DTOs**: `{Entity}{Action}Request`, `{Entity}Response`
- **Validators**: `{Entity}{Action}RequestValidator`
- **Interfaces**: `I{Entity}Service`, `I{Entity}Repository`
- **Services**: `{Entity}Service`
- **Repositories**: `{Entity}Repository`
- **Controllers**: `{Entity}Controller`

### 2. Error Handling
- Sử dụng `BaseResponse<T>.FailureResponse()` cho errors
- Log errors với `ILogger`
- Trả về appropriate HTTP status codes
- Không expose internal errors ra client

### 3. Validation
- Validate input với FluentValidation
- Validate business rules trong Service layer
- Trả về error messages rõ ràng bằng tiếng Việt

### 4. Logging
- Log các thao tác quan trọng (create, update, delete)
- Log errors với đầy đủ context
- Sử dụng structured logging

### 5. Security
- Sử dụng `[Authorize]` cho protected endpoints
- Validate user permissions
- Sanitize input data
- Use parameterized queries (EF Core handles this)

### 6. Performance
- Sử dụng `AsNoTracking()` cho read operations
- Sử dụng `Include()` để eager load related entities
- Implement pagination cho large datasets
- Cache frequently accessed data (if needed)

### 7. Authorization & Role Validation
- Sử dụng `[AuthorizeRoles]` attribute để kiểm tra role
- Sử dụng `ClaimsHelper` để lấy thông tin từ JWT token
- Validate permissions trong Service layer khi cần
- Log các truy cập vào endpoints quan trọng

---

## Authorization và Role Validation

### 1. Sử dụng AuthorizeRolesAttribute

**AuthorizeRolesAttribute** cho phép bạn chỉ định các roles được phép truy cập endpoint:

```csharp
// Chỉ ADMIN mới được truy cập
[AuthorizeRoles("ADMIN")]
[HttpGet]
public async Task<IActionResult> GetAllUsers()
{
    // ...
}

// MEMBER, STAFF, ADMIN đều được truy cập
[AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(Guid id)
{
    // ...
}

// Yêu cầu authentication nhưng không yêu cầu role cụ thể
[Authorize]
[HttpGet("me")]
public IActionResult GetCurrentUser()
{
    // ...
}

// Cho phép anonymous access
[AllowAnonymous]
[HttpGet("public")]
public IActionResult GetPublicData()
{
    // ...
}
```

**Các Roles hợp lệ**:
- `MEMBER`: Thành viên (người mua/người bán)
- `STAFF`: Nhân viên môi giới
- `ADMIN`: Quản trị viên

### 2. Sử dụng ClaimsHelper

**ClaimsHelper** cung cấp các methods tiện lợi để lấy thông tin từ JWT token:

```csharp
using EVehicle.API.Helpers;

// Lấy UserId
var userId = ClaimsHelper.GetUserId(User);
if (userId == null)
{
    return Unauthorized(BaseResponse<object>.FailureResponse(
        "Không thể xác định người dùng"));
}

// Lấy Role
var role = ClaimsHelper.GetRole(User);

// Lấy Email
var email = ClaimsHelper.GetEmail(User);

// Lấy PhoneNumber
var phoneNumber = ClaimsHelper.GetPhoneNumber(User);

// Lấy tất cả thông tin user
var userInfo = ClaimsHelper.GetUserInfo(User);
if (userInfo != null)
{
    // userInfo.UserId
    // userInfo.Email
    // userInfo.PhoneNumber
    // userInfo.Role
    // userInfo.FullName
    // userInfo.Status
}

// Kiểm tra user có role cụ thể không
if (ClaimsHelper.HasRole(User, "ADMIN"))
{
    // User là ADMIN
}

// Kiểm tra user có bất kỳ role nào trong danh sách không
if (ClaimsHelper.HasAnyRole(User, "ADMIN", "STAFF"))
{
    // User là ADMIN hoặc STAFF
}
```

### 3. Ví dụ: Kiểm tra Permission trong Controller

```csharp
[HttpGet("{id}")]
[AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
public async Task<IActionResult> GetUser(Guid id)
{
    var currentUserInfo = ClaimsHelper.GetUserInfo(User);
    if (currentUserInfo == null)
    {
        return Unauthorized(BaseResponse<object>.FailureResponse(
            "Không thể xác định người dùng"));
    }

    // MEMBER chỉ được xem thông tin của chính mình
    if (currentUserInfo.Role == "MEMBER" && currentUserInfo.UserId != id)
    {
        return Forbid(); // Trả về 403 Forbidden
    }

    // STAFF và ADMIN có thể xem thông tin của tất cả users
    var user = await _userService.GetUserByIdAsync(id);
    return Ok(BaseResponse<object>.SuccessResponse(user));
}
```

### 4. Role Validation Middleware

**RoleValidationMiddleware** đã được cấu hình trong `Program.cs` và tự động:
- Log các truy cập vào endpoints có yêu cầu role
- Validate role có hợp lệ không (MEMBER, STAFF, ADMIN)
- Không block request (Authorization middleware xử lý authorization)

Middleware này chỉ để logging và monitoring, không thay thế Authorization middleware của ASP.NET Core.

---

## Sử dụng BaseResponse và PagedResponse

### 1. BaseResponse<T>

**BaseResponse<T>** là class chuẩn cho tất cả API responses:

```csharp
// Success Response
var response = BaseResponse<PostResponse>.SuccessResponse(
    postResponse, 
    "Tạo bài đăng thành công");

return Ok(response);

// Failure Response
var response = BaseResponse<PostResponse>.FailureResponse(
    "Dữ liệu không hợp lệ",
    new List<string> { "Email đã tồn tại", "Số điện thoại không hợp lệ" });

return BadRequest(response);

// Failure Response từ Exception
try
{
    // ...
}
catch (Exception ex)
{
    var response = BaseResponse<PostResponse>.FailureResponse(
        ex,
        "Đã xảy ra lỗi khi tạo bài đăng");
    return StatusCode(500, response);
}
```

**Response Structure**:
```json
{
  "success": true,
  "message": "Tạo bài đăng thành công",
  "data": {
    "postId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Xe điện VinFast",
    // ...
  },
  "errors": null,
  "timestamp": "2024-11-08T10:00:00Z"
}
```

### 2. BaseResponse (không có data)

Sử dụng cho các API không trả về data:

```csharp
// Success Response
var response = BaseResponse.SuccessResponse("Xóa bài đăng thành công");
return Ok(response);

// Failure Response
var response = BaseResponse.FailureResponse(
    "Không tìm thấy bài đăng");
return NotFound(response);
```

### 3. PagedResponse<T>

**PagedResponse<T>** sử dụng cho các API có phân trang:

```csharp
var result = await _postRepository.GetPostsAsync(
    request.PageNumber,
    request.PageSize,
    request.SortBy,
    request.SortDirection);

var response = PagedResponse<PostResponse>.SuccessResponse(
    result.Items.Select(MapToResponse).ToList(),
    result.PageNumber,
    result.PageSize,
    result.TotalCount,
    "Lấy danh sách bài đăng thành công");

return Ok(response);
```

**Response Structure**:
```json
{
  "success": true,
  "message": "Lấy danh sách bài đăng thành công",
  "data": [
    {
      "postId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Xe điện VinFast",
      // ...
    }
  ],
  "errors": null,
  "timestamp": "2024-11-08T10:00:00Z",
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPrevious": false,
  "hasNext": true
}
```

### 4. PagedRequest

**PagedRequest** là base class cho các request có phân trang:

```csharp
// Tạo Request class kế thừa PagedRequest
public class PostSearchRequest : PagedRequest
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Location { get; set; }
}

// Sử dụng trong Controller
[HttpGet]
[AllowAnonymous]
public async Task<IActionResult> GetPosts([FromQuery] PostSearchRequest request)
{
    // Validate request
    request.IsValid(); // Tự động validate và chuẩn hóa PageNumber, PageSize
    
    var response = await _postService.GetPostsAsync(request);
    return Ok(response);
}
```

**PagedRequest Properties**:
- `PageNumber`: Số trang (bắt đầu từ 1, mặc định 1)
- `PageSize`: Số lượng items trên mỗi trang (mặc định 10, tối đa 100)
- `SortBy`: Trường để sắp xếp
- `SortDirection`: Hướng sắp xếp ("asc" hoặc "desc", mặc định "asc")
- `Skip`: Tính toán số bản ghi cần bỏ qua (tự động)

### 5. PagedResult<T>

**PagedResult<T>** sử dụng trong Repository/Service để trả về kết quả phân trang:

```csharp
// Trong Repository
public async Task<PagedResult<Post>> GetPostsAsync(
    int pageNumber, 
    int pageSize, 
    string? sortBy, 
    string? sortDirection)
{
    var query = _context.Posts.AsNoTracking();

    // Apply sorting
    if (!string.IsNullOrWhiteSpace(sortBy))
    {
        query = sortDirection?.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sortBy))
            : query.OrderBy(GetSortProperty(sortBy));
    }
    else
    {
        query = query.OrderByDescending(p => p.CreatedAt);
    }

    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return PagedResult<Post>.Create(items, totalCount, pageNumber, pageSize);
}

// Trong Service
var result = await _postRepository.GetPostsAsync(
    request.PageNumber,
    request.PageSize,
    request.SortBy,
    request.SortDirection);

// Convert to Response
var response = PagedResponse<PostResponse>.SuccessResponse(
    result.Items.Select(MapToResponse).ToList(),
    result.PageNumber,
    result.PageSize,
    result.TotalCount,
    "Lấy danh sách bài đăng thành công");
```

---

## Best Practices cho Authorization

### 1. Luôn Validate User trong Controller

```csharp
[HttpPost]
[AuthorizeRoles("MEMBER")]
public async Task<IActionResult> CreatePost([FromBody] PostCreateRequest request)
{
    // Luôn validate user trước khi xử lý
    var userId = ClaimsHelper.GetUserId(User);
    if (userId == null)
    {
        return Unauthorized(BaseResponse<PostResponse>.FailureResponse(
            "Không thể xác định người dùng"));
    }

    // Pass userId vào service
    var response = await _postService.CreatePostAsync(userId.Value, request);
    return Ok(response);
}
```

### 2. Kiểm tra Ownership trong Service Layer

```csharp
// Trong Service
public async Task<BaseResponse<PostResponse>> UpdatePostAsync(
    Guid userId, 
    Guid postId, 
    PostUpdateRequest request)
{
    var post = await _postRepository.GetByIdAsync(postId);
    if (post == null)
    {
        return BaseResponse<PostResponse>.FailureResponse(
            "Không tìm thấy bài đăng");
    }

    // Kiểm tra user có quyền sửa bài đăng này không
    if (post.UserId != userId)
    {
        return BaseResponse<PostResponse>.FailureResponse(
            "Bạn không có quyền sửa bài đăng này");
    }

    // Update post
    // ...
}
```

### 3. Sử dụng Role-based Access Control

```csharp
// ADMIN có thể xem tất cả
// STAFF có thể xem bài đăng được gán
// MEMBER chỉ xem được bài đăng của mình
[HttpGet]
[AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
public async Task<IActionResult> GetPosts([FromQuery] PostSearchRequest request)
{
    var userInfo = ClaimsHelper.GetUserInfo(User);
    if (userInfo == null)
    {
        return Unauthorized();
    }

    // Filter based on role
    if (userInfo.Role == "MEMBER")
    {
        // MEMBER chỉ xem bài đăng của mình
        request.UserId = userInfo.UserId;
    }
    else if (userInfo.Role == "STAFF")
    {
        // STAFF xem bài đăng được gán
        request.StaffId = userInfo.UserId;
    }
    // ADMIN xem tất cả (không filter)

    var response = await _postService.GetPostsAsync(request);
    return Ok(response);
}
```

### 4. Log các Thao tác Quan trọng

```csharp
[HttpDelete("{id}")]
[AuthorizeRoles("ADMIN")]
public async Task<IActionResult> DeletePost(Guid id)
{
    var userInfo = ClaimsHelper.GetUserInfo(User);
    
    _logger.LogWarning(
        "Admin {AdminId} đang xóa bài đăng {PostId}",
        userInfo?.UserId,
        id);

    var response = await _postService.DeletePostAsync(id);
    
    if (response.Success)
    {
        _logger.LogInformation(
            "Admin {AdminId} đã xóa bài đăng {PostId} thành công",
            userInfo?.UserId,
            id);
    }

    return Ok(response);
}
```

---

## Ví dụ Hoàn chỉnh

### 1. AuthController (UC01-UC03)

Xem `AuthController.cs` và `AuthService.cs` để tham khảo implementation hoàn chỉnh của:
- UC01: Đăng ký tài khoản
- UC02: Đăng nhập
- UC03: Đăng nhập bằng mạng xã hội

### 2. UsersController

Xem `UsersController.cs` để tham khảo:
- Sử dụng `[AuthorizeRoles]` attribute
- Sử dụng `ClaimsHelper` để lấy thông tin user
- Kiểm tra permissions dựa trên role
- Sử dụng `BaseResponse<T>` cho responses

---

## Common Patterns

### 1. Pattern: Get UserId từ Token

```csharp
private Guid? GetUserIdFromToken()
{
    return ClaimsHelper.GetUserId(User);
}

// Sử dụng
var userId = GetUserIdFromToken();
if (userId == null)
{
    return Unauthorized(BaseResponse<object>.FailureResponse(
        "Không thể xác định người dùng"));
}
```

### 2. Pattern: Validate Ownership

```csharp
// Trong Service
private async Task<bool> ValidateOwnershipAsync(Guid userId, Guid resourceId)
{
    var resource = await _repository.GetByIdAsync(resourceId);
    if (resource == null)
        return false;

    return resource.UserId == userId;
}

// Sử dụng
if (!await ValidateOwnershipAsync(userId, postId))
{
    return BaseResponse<PostResponse>.FailureResponse(
        "Bạn không có quyền truy cập tài nguyên này");
}
```

### 3. Pattern: Filter by Role

```csharp
// Trong Service
public async Task<PagedResponse<PostResponse>> GetPostsAsync(
    Guid? userId,
    PostSearchRequest request)
{
    var query = _context.Posts.AsNoTracking();

    // Filter by role
    var userInfo = await GetUserInfoAsync(userId);
    if (userInfo?.Role == "MEMBER")
    {
        query = query.Where(p => p.UserId == userId);
    }
    else if (userInfo?.Role == "STAFF")
    {
        query = query.Where(p => p.PostStaffAssignments
            .Any(psa => psa.StaffId == userId));
    }
    // ADMIN: no filter

    // Apply search filters
    if (!string.IsNullOrWhiteSpace(request.Keyword))
    {
        query = query.Where(p => 
            p.Title.Contains(request.Keyword) ||
            p.Description.Contains(request.Keyword));
    }

    // Apply pagination
    var totalCount = await query.CountAsync();
    var items = await query
        .Skip(request.Skip)
        .Take(request.PageSize)
        .ToListAsync();

    return PagedResponse<PostResponse>.SuccessResponse(
        items.Select(MapToResponse).ToList(),
        request.PageNumber,
        request.PageSize,
        totalCount);
}
```

---

## Tài liệu Tham khảo

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [ASP.NET Core Web API Documentation](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/)
- [JWT Authentication](https://jwt.io/)

