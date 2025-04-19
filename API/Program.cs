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
        "AllowAll",
        policy =>
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

IConfiguration Configuration = builder.Configuration;

string connectionString;
try
{
    Console.WriteLine("Prøver at åbne DefaultConnection til Mercantecs Datacenter...");
    var defaultConnection = Configuration.GetConnectionString("DefaultConnection");
    using var connection = new Npgsql.NpgsqlConnection(defaultConnection);
    connection.Open(); 
    connectionString = defaultConnection; 
    connection.Close();
}
catch
{
    Console.WriteLine("Fejl ved at åbne DefaultConnection. Prøver NeonConnection...");
    connectionString = Configuration.GetConnectionString("NeonConnection");
}

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

// Log Google Authentication configuration
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (string.IsNullOrEmpty(googleClientId) || string.IsNullOrEmpty(googleClientSecret))
{
    Console.WriteLine("Google Authentication is not configured correctly.");
    Console.WriteLine("Please check the 'Authentication:Google:ClientId' and 'Authentication:Google:ClientSecret' values in appsettings.json.");
}
else
{
    Console.WriteLine("Google Authentication is configured:");
    Console.WriteLine($"ClientId: {googleClientId}");
    Console.WriteLine($"ClientSecret: {new string('*', googleClientSecret.Length)}"); 
}

// Configure JWT Authentication
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "accounts.google.com",
            ValidateAudience = true,
            ValidAudience = Configuration["Authentication:Google:ClientId"],
            ValidateLifetime = true
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
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
app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
