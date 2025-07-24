using CommonDto.HandleErrorResult;
using ContentManagementService.Application.UnitOfWork;
using ContentManagementService.Application.Usecases;
using ContentManagementService.Domain.Interface.UnitOfWork;
using ContentManagementService.Infrastructure.Data.DBContext;
using ContentManagementService.Infrastructure.Handler;
using ContentManagementService.Infrastructure.Service;
using ContentManagementService.Interface_Adapters;
using ContentManagementService.Interface_Adapters.APIs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

DotNetEnv.Env.Load();
var MyConnectionString = Environment.GetEnvironmentVariable("MyConnectionString");
var JwtKey = Environment.GetEnvironmentVariable("Jwt__Key");
var JwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
var JwtAudiences = Environment.GetEnvironmentVariable("Jwt__Audience")?.Split(',');

var key = Encoding.UTF8.GetBytes(JwtKey);


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ContentManagementContext>(options =>
      options.UseSqlServer(MyConnectionString));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddScoped<IUnitOfWork, ContentManagementUnitOfWork>();

builder.Services.AddHttpClient<ProductService>()
    .ConfigureHttpClient(httpClient =>
    {
        httpClient.BaseAddress = new Uri("http://localhost:5272/");
    })
    // Thêm AuthHeaderHandler vào pipeline của HttpClient này
    .AddHttpMessageHandler<AuthHeaderHandler>()
    // Cấu hình PrimaryHttpMessageHandler (ví dụ: để bỏ qua xác thực SSL)
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            // Tùy chọn: Bỏ qua xác thực SSL nếu đang phát triển cục bộ với chứng chỉ tự ký
            // CỰC KỲ KHÔNG NÊN DÙNG TRONG MÔI TRƯỜNG PRODUCTION VÌ RỦI RO BẢO MẬT!
            // ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });
builder.Services.AddScoped<CreateContentManagementUC>();
builder.Services.AddScoped<UpdateContentManagementUC>();
builder.Services.AddScoped<GetContentManagementUC>();
builder.Services.AddScoped<DeleteContentManagement>();

builder.Services.AddSingleton<HandleResultApi>();
builder.Services.AddSingleton<HandleServiceError>();
builder.Services.AddSingleton<HandleResultApi>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin", // Tên của chính sách CORS
                      policyBuilder =>
                      {
                          // Sử dụng các tham số riêng biệt cho WithOrigins
                          // hoặc một mảng các chuỗi nếu bạn có nhiều origins
                          policyBuilder.WithOrigins("http://localhost:4300", "http://localhost:4200") // SỬA LỖI TẠI ĐÂY
                                 .AllowAnyHeader()
                                 .AllowAnyMethod()
                                 .AllowCredentials(); // Bỏ comment nếu bạn cần hỗ trợ cookie/credentials

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


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API Name", Version = "v1" });

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyAdmin", policy =>
    {
        policy.RequireRole("Admin");
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "CONTENT MANAGEMENT APIS";
    });
}

app.UseCors("AllowSpecificOrigin");
app.MapContentManagementEndpoints();

app.Run();

