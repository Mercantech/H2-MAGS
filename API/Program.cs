using API.DBContext;
using Microsoft.EntityFrameworkCore;

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

// Bind ActiveDirectorySettings fra appsettings.json
builder.Services.Configure<ActiveDirectorySettings>(
    builder.Configuration.GetSection("ActiveDirectory"));

// Registrer ActiveDirectoryService som en singleton eller scoped tjeneste
builder.Services.AddScoped<ActiveDirectoryService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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