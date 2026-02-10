# Clean Architecture Infrastructure Guide

This guide explains how to use the infrastructure components to write minimal, maintainable code following senior C# and ASP.NET best practices.

## 🏗️ Architecture Overview

The infrastructure is designed around these principles:
- **DRY (Don't Repeat Yourself)**: Common patterns are abstracted into reusable components
- **SOLID Principles**: Each component has a single responsibility
- **Clean Architecture**: Clear separation of concerns across layers
- **Ardalis Specifications**: Powerful query building with reusable patterns

## 📁 Project Structure

```
Recruiter/
├── Domain/
│   ├── Models/           # Domain entities
│   ├── ValueObjects/     # Value objects (ISBN, Money, etc.)
│   └── Events/           # Domain events infrastructure
├── Application/
│   ├── Common/           # Shared infrastructure
│   │   ├── Extensions/   # Result extensions
│   │   ├── Mapping/      # Base AutoMapper profiles
│   │   ├── Services/     # Base service interfaces
│   │   ├── Specifications/ # Base specifications
│   │   └── Validation/   # Custom validation attributes
│   └── [Module]/         # Feature modules
│       ├── Dto/          # Data Transfer Objects
│       ├── Mapping/      # AutoMapper profiles
│       ├── Specifications/ # Query specifications
│       └── [Module]Service.cs # Service implementation
├── Infrastructure/
│   ├── Ardalis/          # Repository implementations
│   ├── Extensions/       # Service registration
│   └── Interceptors/     # EF Core interceptors
└── WebApi/
    └── Endpoints/        # API controllers
```

## 🚀 Quick Start

### 1. Create a New Entity

```csharp
// Domain/Models/Product.cs
public class Product : BaseDbModel
{
    [Required]
    [StringLength(200)]
    public string Name { get; private set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; private set; }
    
    [PositiveNumber] // Custom validation attribute
    public decimal Price { get; private set; }
    
    [StringLength(50)]
    public string? Sku { get; private set; }
    
    // Business logic methods
    public Product(string name, decimal price, string? description = null, string? sku = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Price = price;
        Description = description;
        Sku = sku;
        Created = DateTimeOffset.UtcNow;
        Updated = DateTimeOffset.UtcNow;
    }
    
    public void UpdateProduct(string name, decimal price, string? description = null, string? sku = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Price = price;
        Description = description;
        Sku = sku;
        Updated = DateTimeOffset.UtcNow;
    }
}
```

### 2. Create DTO and Search Request

```csharp
// Application/Product/Dto/ProductDto.cs
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Sku { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Updated { get; set; }
}

// Application/Product/SearchRequest/ProductSearchRequest.cs
public class ProductSearchRequest : SearchRequest
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Sku { get; set; }
}
```

### 3. Create AutoMapper Profile

```csharp
// Application/Product/Mapping/ProductProfile.cs
public class ProductProfile : BaseMappingProfile<Product, ProductDto>
{
    // BaseMappingProfile handles all common mappings automatically
    // Override ConfigureMappings() for custom mappings if needed
}
```

### 4. Create Search Specification

```csharp
// Application/Product/Specifications/ProductSearchSpecification.cs
public class ProductSearchSpecification : PagedSpecification<Product>
{
    public ProductSearchSpecification(ProductSearchRequest searchRequest) : base(searchRequest)
    {
        if (!string.IsNullOrWhiteSpace(searchRequest.Name))
            Query.Where(x => x.Name.Contains(searchRequest.Name));
            
        if (searchRequest.MinPrice.HasValue)
            Query.Where(x => x.Price >= searchRequest.MinPrice.Value);
            
        if (searchRequest.MaxPrice.HasValue)
            Query.Where(x => x.Price <= searchRequest.MaxPrice.Value);
            
        if (!string.IsNullOrWhiteSpace(searchRequest.Sku))
            Query.Where(x => x.Sku == searchRequest.Sku);
    }
    
    protected override string[] GetSearchableProperties()
    {
        return new[] { nameof(Product.Name), nameof(Product.Description) };
    }
}
```

### 5. Create Service

```csharp
// Application/Product/ProductService.cs
public class ProductService : BaseService<Product, ProductDto, ProductSearchRequest>
{
    public ProductService(IRepository<Product> repository, IMapper mapper) 
        : base(repository, mapper)
    {
    }

    protected override ISpecification<Product> CreateSearchSpecification(ProductSearchRequest searchRequest)
    {
        return new ProductSearchSpecification(searchRequest);
    }
    
    // Add custom business logic here
    public async Task<Result<List<ProductDto>>> GetExpensiveProductsAsync(decimal minPrice, CancellationToken cancellationToken = default)
    {
        var specification = new ExpensiveProductsSpecification(minPrice);
        var products = await _repository.ListAsync(specification, cancellationToken);
        var dtos = _mapper.Map<List<ProductDto>>(products);
        return Result<List<ProductDto>>.Success(dtos);
    }
}
```

### 6. Register Services

```csharp
// Program.cs or Startup.cs
services.AddInfrastructure(connectionString);
services.AddCommonServices();
services.AddScoped<ProductService>();
```

### 7. Create API Controller

```csharp
// WebApi/Endpoints/ProductController.cs
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductSearchRequest request)
    {
        var result = await _productService.SearchAsync(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductDto dto)
    {
        var result = await _productService.CreateAsync(dto);
        return result.IsSuccess ? CreatedAtAction(nameof(GetProduct), new { id = result.Value.Id }, result.Value) : BadRequest(result.Errors);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] ProductDto dto)
    {
        var result = await _productService.UpdateAsync(id, dto);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var result = await _productService.DeleteAsync(id);
        return result.IsSuccess ? NoContent() : BadRequest(result.Errors);
    }
}
```

## 🔧 Infrastructure Components

### BaseDbModel
- Provides common audit fields (Id, Created, Updated, CreatedBy, UpdatedBy, RowVersion)
- All entities should inherit from this

### BaseService<TEntity, TDto, TSearchRequest>
- Provides standard CRUD operations
- Handles mapping between entities and DTOs
- Includes error handling and validation
- Supports bulk operations

### BaseSpecification<TEntity>
- Common query patterns (search, pagination, sorting)
- Reusable across different entities
- Extensible for custom business logic

### Result Extensions
- Functional programming patterns for Result<T>
- Map, Bind, OnSuccess, OnFailure operations
- Reduces boilerplate code

### Custom Validation Attributes
- `ISBNAttribute`: Validates ISBN-10 and ISBN-13 formats
- `FutureDateAttribute`: Prevents future dates
- `PositiveNumberAttribute`: Ensures positive numbers

### Value Objects
- `ISBN`: Type-safe ISBN handling with validation
- `Money`: Type-safe monetary values with currency support

### Audit Interceptor
- Automatically sets audit fields
- Integrates with HTTP context for user tracking
- No manual audit field management needed

## 📝 Best Practices

### 1. Entity Design
- Use private setters for domain properties
- Include business logic methods in entities
- Use value objects for complex types
- Keep entities focused on business rules

### 2. Service Layer
- Inherit from BaseService for standard operations
- Add custom business logic methods
- Use Result<T> for all operations
- Keep services focused on orchestration

### 3. Specifications
- Inherit from BaseSpecification for common patterns
- Use PagedSpecification for search operations
- Override GetSearchableProperties() for search functionality
- Keep specifications focused on query logic

### 4. DTOs
- Keep DTOs simple and focused on data transfer
- Use validation attributes for input validation
- Separate DTOs for different operations if needed

### 5. Error Handling
- Always use Result<T> for operations that can fail
- Provide meaningful error messages
- Use Result extensions for chaining operations

## 🎯 Benefits

1. **Minimal Code**: Most CRUD operations require only a few lines
2. **Consistent Patterns**: All modules follow the same structure
3. **Type Safety**: Value objects and Result<T> prevent runtime errors
4. **Maintainable**: Clear separation of concerns and reusable components
5. **Testable**: Easy to unit test with dependency injection
6. **Scalable**: Infrastructure grows with your application

## 🔍 Example: Complete Module

The `Product` module demonstrates a complete implementation:
- Domain entity with business logic
- DTO with validation
- Search request with filters
- AutoMapper profile
- Search specification
- Service with custom operations
- Clean API controller
- All following the established patterns

This infrastructure allows you to create new modules quickly while maintaining consistency and following best practices.