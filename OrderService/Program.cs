using AuthorizationPolicy.AdminOrSelfUserId;
using CommonDto.HandleErrorResult;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderService.Application.UnitOfWork;
using OrderService.Application.Usecases;
using OrderService.Domain.Interface.UnitOfWork;
using OrderService.Infrastructure.Data.DBContext;
using OrderService.Infrastructure.Handler;
using OrderService.Infrastructure.Service;
using OrderService.Interface_Adapters;
using OrderService.Interface_Adapters.API;
using System.Text;

DotNetEnv.Env.Load();
var MyConnectionString = Environment.GetEnvironmentVariable("MyConnectionString");
var JwtKey = Environment.GetEnvironmentVariable("Jwt__Key");
var JwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
var JwtAudiences = Environment.GetEnvironmentVariable("Jwt__Audience")?.Split(',');
var accessTokenExpirationMinutes = Environment.GetEnvironmentVariable("Jwt__AccessTokenExpirationMinutes");


var key = Encoding.UTF8.GetBytes(JwtKey);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderContext>(options =>
    options.UseSqlServer(MyConnectionString));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddHttpClient<ProductService>()
    .ConfigureHttpClient(httpClient =>
    {
        httpClient.BaseAddress = new Uri("http://product-service:8080/");
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

builder.Services.AddScoped<IUnitOfWork, OrderUnitOfWork>();
builder.Services.AddScoped<CreateOrderUC>();
builder.Services.AddScoped<GetOrderUC>();
builder.Services.AddScoped<UpdateOrderUC>();

builder.Services.AddSingleton<HandleResultApi>();
builder.Services.AddSingleton<HandleServiceError>();
builder.Services.AddSingleton<HandleResultApi>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null; // Giữ nguyên tên thuộc tính (PascalCase) cho SignalR
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin", // Tên của chính sách CORS
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order APIs", Version = "v1" });

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
            }
        };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyAdmin", policy =>
    {
        policy.RequireRole("Admin");
    });

    options.AddPolicy("OnlyCustomer", policy =>
    {
        policy.RequireRole("Customer");
    });

    options.AddPolicy("AdminOrSelfAccountId", policy =>
    {
        policy.Requirements.Add(new AdminOrSelfAccountIDReq());
    });
});


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OrderService.Infrastructure.Data.DBContext.OrderContext>();
        // Đây là dòng quan trọng để áp dụng tất cả các migrations đang chờ xử lý
        // và tạo database nếu nó chưa tồn tại.
        context.Database.Migrate();
        Console.WriteLine("[DEBUG] Database migrations applied successfully for OrderService DB.");
    }
    catch (Exception ex)
    {
        // Ghi log lỗi nếu có vấn đề trong quá trình migrations
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "[DEBUG] An error occurred while applying migrations to OrderService DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "ORDER APIS";
    });
}
app.UseCors("AllowSpecificOrigin");
app.MapOrderEndpoints();
app.Run();
