using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System;
using System.Reflection;
using API.Data;
using API.Services;
using API.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IConfiguration Configuration = builder.Configuration;

        string connectionString = Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") ?? "";

        builder.Services.AddDbContext<AppDBContext>(options =>
                options.UseNpgsql(connectionString));

        // Registrer JWT Service
        builder.Services.AddScoped<JwtService>();
        
        // Registrer Login Attempt Service
        builder.Services.AddScoped<LoginAttemptService>();
        
        // Registrer Data Seeder Service
        builder.Services.AddScoped<DataSeederService>();
        
        // Registrer Active Directory Service
        builder.Services.AddScoped<ActiveDirectoryService>();
        
        // Registrer Mail Service
        builder.Services.AddScoped<MailService>();
        
        // Registrer Ticket Service
        builder.Services.AddScoped<TicketService>();
        
        // Registrer Password Hashing Service (Argon2id)
        builder.Services.AddScoped<PasswordHashingService>();

        // Konfigurer JWT Authentication
        var jwtSecretKey = Configuration["Jwt:SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        var jwtIssuer = Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER");
        var jwtAudience = Configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey ?? "")),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization();

        // Add services to the container.
        builder.Services.AddControllers();
        
        // Add SignalR med CORS support
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });
        
        // Add Memory Cache for login attempt tracking
        builder.Services.AddMemoryCache();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddSwaggerGen(c =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Tilføj JWT Bearer support til Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });

        // Tilføj CORS for specifikke Blazor WASM domæner
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowSpecificOrigins",
                builder =>
                {
                    builder
                        .WithOrigins(
                            "http://localhost:5085",
                            "http://localhost:8052",
                            "https://localhost:7285",
                            "https://h2.mercantec.tech",
                            "http://localhost:5173",
                            "https://h2-mags-admin.mercantec.tech"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Content-Disposition")
                        .AllowCredentials(); // Nødvendigt for SignalR
                }
            );
        });

        // Tilføj basic health checks
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), new[] { "live" });

        var app = builder.Build();

        // Brug CORS - skal være før anden middleware
        app.UseCors("AllowSpecificOrigins");

        // Map health checks
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        // Test CORS endpoint
        app.MapGet("/test-cors", () => "CORS working!").RequireCors("AllowSpecificOrigins");

        app.MapOpenApi();

        // Scalar Middleware for OpenAPI
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("MAGSLearn")
                .WithTheme(ScalarTheme.Mars)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        // Map the Swagger UI
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        // Map SignalR Hub with CORS support
        app.MapHub<TicketHub>("/tickethub")
            .RequireCors("AllowSpecificOrigins");

        // Set default port if not specified
        var port = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(':').LastOrDefault() ?? "8045";
        app.Run($"http://+:{port}");
    }
}
