using System.Text;
using API.DBContext;
using API.Services.Mapping.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Tilfj CORS-politik
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowSpecificOrigin",
        policy =>
            policy
                .WithOrigins(
                    "https://localhost:7026",
                    "http://localhost:5036",
                    "https://hotel.mercantec.tech",
                    "https://localhost:51806/"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

IConfiguration Configuration = builder.Configuration;

string connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<HotelContext>(options => options.UseNpgsql(connectionString));

// API.Services
builder.Services.AddScoped<SignupService>();
builder.Services.AddScoped<JWTService>();

// Bind ActiveDirectorySettings fra appsettings.json
builder.Services.Configure<ActiveDirectorySettings>(
    builder.Configuration.GetSection("ActiveDirectory")
);

// Registrer ActiveDirectoryService som en singleton eller scoped tjeneste
builder.Services.AddScoped<ActiveDirectoryService>();

// Tilføj EmailService
builder.Services.AddScoped<EmailService>();

// Configure JWT Authentication
builder
    .Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer =
                Environment.GetEnvironmentVariable("JWT_ISSUER")
                ?? Configuration["JwtSettings:Issuer"],
            ValidAudience =
                Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                ?? Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    Environment.GetEnvironmentVariable("JWT_KEY")
                        ?? Configuration["JwtSettings:Key"]
                )
            ),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel API", Version = "v1" });

    // Opdateret security definition
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
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
                new string[] { }
            }
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Anvend CORS-politikken
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
