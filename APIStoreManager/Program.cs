// Program.cs
using APIStoreManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using APIStoreManager.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using APIStoreManager.Authorization;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddSingleton<RSA>(_ => {
    var privateKey = builder.Configuration["Jwt:RsaPrivateKey"];

    privateKey = privateKey.Replace("\\n", "\n");

    var rsa = RSA.Create();

    try
    {
        rsa.ImportFromPem(privateKey.ToCharArray());
        return rsa;
    }
    catch (Exception ex)
    {
        throw new Exception("Failed to load RSA key", ex);
    }
});


// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connect to SQL Server
builder.Services.AddDbContext<StoreManagerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Token service
builder.Services.AddScoped<TokenService>();


// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var rsa = builder.Services.BuildServiceProvider().GetRequiredService<RSA>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new RsaSecurityKey(rsa),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { message = "Phiên đăng nhập hết hạn hoặc không hợp lệ" });
                return context.Response.WriteAsync(result);
            }
        };
    });
// Authorize
builder.Services.AddSwaggerGen(options =>
    {

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "APIStoreManager", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token dạng: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// auth role handle
builder.Services.AddHttpContextAccessor(); 

builder.Services.AddScoped<IAuthorizationHandler, OwnerHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OwnerOnly", policy =>
        policy.Requirements.Add(new OwnerRequirement()));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


//CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});





var app = builder.Build();
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
