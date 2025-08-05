
using AuthorizationPolicy.AdminOrSelfUserId;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserService.Application.Service;
using UserService.Application.UnitOfWorks;
using UserService.Application.Usecases;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Infrastructure.Cache_Service;
using UserService.Infrastructure.Data.DBContext;
using UserService.Infrastructure.SendEmail;
using UserService.Infrastructure.Setting;
using UserService.Infrastructure.Verify_Email;
using UserService.Interface_Adapters;
using UserService.Interface_Adapters.APIs;


DotNetEnv.Env.Load();
var MyConnectionString = Environment.GetEnvironmentVariable("MyConnectionString");
var JwtKey = Environment.GetEnvironmentVariable("Jwt__Key");
var JwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
var JwtAudiences = Environment.GetEnvironmentVariable("Jwt__Audience")?.Split(',');
var accessTokenExpirationMinutes = Environment.GetEnvironmentVariable("Jwt__AccessTokenExpirationMinutes");

var jwtSettingsInstance = new JWTSetting
{
    Issuer = JwtIssuer,
    Audiences = JwtAudiences,
    Key = JwtKey,
    ExpirationMinutes = int.Parse(accessTokenExpirationMinutes)
};
var key = Encoding.UTF8.GetBytes(JwtKey);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlServer(MyConnectionString));

builder.Services.AddScoped<IUnitOfWork, UserUnitOfWork>();
builder.Services.AddScoped<CreateUserUC>();
builder.Services.AddScoped<GetUserUC>();
builder.Services.AddScoped<UpdateUserUC>();
builder.Services.AddScoped<LoginLogoutSignUpUC>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<HashService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailValidatorService>();

builder.Services.AddSingleton<IAuthorizationHandler, AdminOrSelfAccountIDHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SelfAccountIDHandler>();
builder.Services.AddSingleton(jwtSettingsInstance);

builder.Services.AddSingleton<HandleResultApi>();
builder.Services.AddMemoryCache(); // Đăng ký dịch vụ IMemoryCache
builder.Services.AddScoped<CacheVerifyCodeService>(); // Đăng ký CacheVerifyCodeService, nó sẽ nhận IMemoryCache
builder.Services.AddTransient<SendEmailService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
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

    }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "RefreshToken";
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

    options.AddPolicy("SelfAccountId", policy =>
    {
        policy.Requirements.Add(new SelfAccountIDReq());
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<UserService.Infrastructure.Data.DBContext.UserContext>();
        // Đây là dòng quan trọng để áp dụng tất cả các migrations đang chờ xử lý
        // và tạo database nếu nó chưa tồn tại.
        context.Database.Migrate();
        Console.WriteLine("[DEBUG] Database migrations applied successfully for UserService DB.");
    }
    catch (Exception ex)
    {
        // Ghi log lỗi nếu có vấn đề trong quá trình migrations
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "[DEBUG] An error occurred while applying migrations to UserService DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "USER APIS";
    });
}
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();
app.Run();
