using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProductService.Application.UnitOfWork;
using ProductService.Application.Usecases;
using ProductService.Domain.Interface.UnitOfWork;
using ProductService.Infrastructure.Data.DBContext;
using ProductService.Interface_Adapters;
using ProductService.Interface_Adapters.APIs;
using System.Text;

DotNetEnv.Env.Load();
var MyConnectionString = Environment.GetEnvironmentVariable("MyConnectionString");
var JwtKey = Environment.GetEnvironmentVariable("Jwt__Key");
var JwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
var JwtAudiences = Environment.GetEnvironmentVariable("Jwt__Audience")?.Split(',');

var key = Encoding.UTF8.GetBytes(JwtKey);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null; // Giữ nguyên tên thuộc tính (PascalCase) cho SignalR
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseSqlServer(MyConnectionString));

builder.Services.AddScoped<IUnitOfWork, ProductUnitOfWork>();

builder.Services.AddScoped<ManageProductImagesUC>(serviceProvider =>
{
    var s3Client = serviceProvider.GetRequiredService<IAmazonS3>();
    var logger = serviceProvider.GetRequiredService<ILogger<ManageProductImagesUC>>();
    // Truyền các giá trị string đã lấy từ cấu hình vào constructor
    return new ManageProductImagesUC(s3Client, logger, "electric-store"); // Thêm s3Region nếu constructor có
});
builder.Services.AddScoped<CreateProductUC>();
builder.Services.AddScoped<DeleteProductUC>();
builder.Services.AddScoped<GetProductUC>();
builder.Services.AddScoped<UpdateProductUC>();

builder.Services.AddSingleton<HandleResultApi>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin",
                      policyBuilder =>
                      {
                          policyBuilder.SetIsOriginAllowed(origin => true)
                          //policyBuilder.WithOrigins("http://localhost:4300", "http://localhost:4200") 
                                 .AllowAnyHeader()
                                 .AllowAnyMethod()
                                 .AllowCredentials(); 

                      });
});


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Prorduct APIs", Version = "v1" });

    // Cấu hình bảo mật Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // tham số trong này chính là scheme
    .AddJwtBearer(options => // AddJWTBearer là authentication handler
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtIssuer,
            ValidAudiences = JwtAudiences,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Đọc Access Token từ cookie
                if (context.Request.Cookies.ContainsKey("AccessToken"))
                {
                    context.Token = context.Request.Cookies["AccessToken"];
                }
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[OnAuthenticationFailed] Authentication failed. Request Path: {context.HttpContext.Request.Path}");
                Console.WriteLine($"[OnAuthenticationFailed] Exception Type: {context.Exception.GetType().Name}");
                Console.WriteLine($"[OnAuthenticationFailed] Exception Message: {context.Exception.Message}");
                // Chỉ in stack trace trong môi trường phát triển để tránh thông tin nhạy cảm trong production logs
                if (context.HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
                {
                    Console.WriteLine($"[OnAuthenticationFailed] Exception Details: {context.Exception}");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"[OnTokenValidated] Token successfully validated.");
                Console.WriteLine($"[OnTokenValidated] User Identity: {context.Principal?.Identity?.Name ?? "N/A"}");
                Console.WriteLine($"[OnTokenValidated] Authentication Scheme: {context.Scheme.Name}");

                // In ra các claims của người dùng
                if (context.Principal?.Claims != null)
                {
                    Console.WriteLine("[OnTokenValidated] User Claims:");
                    foreach (var claim in context.Principal.Claims)
                    {
                        Console.WriteLine($"  - {claim.Type}: {claim.Value}");
                    }
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var authHeaderValue = context.Request.Headers.Authorization.FirstOrDefault();
                Console.WriteLine($"[OnChallenge] Challenge issued. Request Path: {context.HttpContext.Request.Path}");
                Console.WriteLine($"[OnChallenge] Received Authorization Header: {authHeaderValue}");
                Console.WriteLine($"[OnChallenge] Auth Failure Message: {context.AuthenticateFailure?.Message ?? "No specific failure message from AuthenticateResult"}");
                Console.WriteLine($"[OnChallenge] Challenge Message: {context.AuthenticateFailure?.Message ?? "No failure message from AuthenticateFailure"}");

                // Ghi log headers response nếu cần
                // Console.WriteLine($"[OnChallenge] Response Headers: {string.Join(", ", context.Response.Headers.Select(h => $"{h.Key}={h.Value}"))}");

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyAdmin", policy =>
    {
        policy.RequireRole("Admin");
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ProductService.Infrastructure.Data.DBContext.ProductContext>();
        // Đây là dòng quan trọng để áp dụng tất cả các migrations đang chờ xử lý
        // và tạo database nếu nó chưa tồn tại.
        context.Database.Migrate();
        Console.WriteLine("[DEBUG] Database migrations applied successfully for ProductService DB.");
    }
    catch (Exception ex)
    {
        // Ghi log lỗi nếu có vấn đề trong quá trình migrations
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "[DEBUG] An error occurred while applying migrations to ProductService DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "PRODUCT APIS";
    });
}

app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapProductEndpoints();

app.Run();

