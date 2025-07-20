using APIGateway.Application.Usecases;
using APIGateway.Handler;
using APIGateway.Infrastructure.Service;
using APIGateway.Interface_Adapters.APIs;
using CommonDto.HandleErrorResult;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;


DotNetEnv.Env.Load();
var MyConnectionString = Environment.GetEnvironmentVariable("MyConnectionString");
var JwtKey = Environment.GetEnvironmentVariable("Jwt__Key");
var JwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
var JwtAudiences = Environment.GetEnvironmentVariable("Jwt__Audience")?.Split(',');

var key = Encoding.UTF8.GetBytes(JwtKey);

var builder = WebApplication.CreateBuilder(args);


builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddOcelot("Ocelot file", builder.Environment)
    .AddEnvironmentVariables();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddScoped<GWCreateUC>();

builder.Services.AddSingleton<HandleResultApi>();
builder.Services.AddSingleton<HandleServiceError>();

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

builder.Services.AddHttpClient<ContentManagementService>()
    .ConfigureHttpClient(httpClient =>
    {
        httpClient.BaseAddress = new Uri("http://localhost:5271/");
    })
    // Thêm AuthHeaderHandler vào pipeline của HttpClient này
    .AddHttpMessageHandler<AuthHeaderHandler>()
    // Cấu hình PrimaryHttpMessageHandler (ví dụ: để bỏ qua xác thực SSL có thể bỏ qua phần này nếu http)
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            // Tùy chọn: Bỏ qua xác thực SSL nếu đang phát triển cục bộ với chứng chỉ tự ký
            // CỰC KỲ KHÔNG NÊN DÙNG TRONG MÔI TRƯỜNG PRODUCTION VÌ RỦI RO BẢO MẬT!
            // ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });

builder.Services.AddEndpointsApiExplorer();
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

builder.Services.AddOcelot(builder.Configuration);

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
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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

                //var accessToken = context.Request.Query["access_token"];
                //var path = context.HttpContext.Request.Path;
                //if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/productPropertyHub"))
                //{
                //    context.Token = accessToken;
                //}
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "GATEWAY APIS";
    });
}

app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

//app.MapGatewayEndpoints(); Code cũ triển khai kết hợp các service trên gateway


await app.UseOcelot();


app.Run();


