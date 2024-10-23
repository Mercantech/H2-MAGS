using System.Text;
using API.DBContext;
using API.Services.Mapping.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Tilføj CORS-politik
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.WithOrigins("https://localhost:7026", "http://localhost:5036")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

IConfiguration Configuration = builder.Configuration;

string connectionString = Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<HotelContext>(options =>
    options.UseNpgsql(connectionString));

// API.Services
builder.Services.AddScoped<SignupService>();
builder.Services.AddScoped<JWTService>();

// Bind ActiveDirectorySettings fra appsettings.json
builder.Services.Configure<ActiveDirectorySettings>(
    builder.Configuration.GetSection("ActiveDirectory"));

// Registrer ActiveDirectoryService som en singleton eller scoped tjeneste
builder.Services.AddScoped<ActiveDirectoryService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = Configuration["JwtSettings:Issuer"],
        ValidAudience = Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
        (
            Encoding.UTF8.GetBytes(Configuration["JwtSettings:Key"])
        ),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Anvend CORS-politikken
app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();