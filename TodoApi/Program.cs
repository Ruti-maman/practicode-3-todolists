using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi;
using DotNetEnv;

// טעינת קובץ .env
if (File.Exists(".env"))
    Env.Load();

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// קריאת הגדרות מקובץ .env
// -------------------------
var connectionString = Environment.GetEnvironmentVariable("ToDoDB");
var jwtKey = Environment.GetEnvironmentVariable("Key");
var jwtIssuer = Environment.GetEnvironmentVariable("Issuer");
var jwtAudience = Environment.GetEnvironmentVariable("Audience");

// בדיקה שהמשתנים לא ריקים
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("ToDoDB environment variable is not set!");
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("Key environment variable is not set!");
if (string.IsNullOrEmpty(jwtIssuer))
    throw new InvalidOperationException("Issuer environment variable is not set!");
if (string.IsNullOrEmpty(jwtAudience))
    throw new InvalidOperationException("Audience environment variable is not set!");

// -------------------------
// הוספת DbContext לשירותים
// -------------------------
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// -------------------------
// הגדרת JWT Authentication
// -------------------------
var key = Encoding.UTF8.GetBytes(jwtKey!);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

// -------------------------
// הגדרת JSON options - שדות באותיות קטנות
// -------------------------
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// -------------------------
// הוספת Swagger
// -------------------------

builder.Services.AddSwaggerGen();

// -------------------------
// הגדרת CORS
// -------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("*");
    });
});
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();

// -------------------------
// הפעלת Swagger
// -------------------------
app.UseSwagger();
app.UseSwaggerUI();

// -------------------------
// הפעלת CORS
// -------------------------
app.UseCors("AllowAllPolicy");


// -------------------------
// Authentication & Authorization
// -------------------------
app.UseAuthentication();
app.UseAuthorization();

// -------------------------
// Route בסיסי לבדיקה
// -------------------------
app.MapGet("/", () => "API is running!");

// -------------------------
// CRUD Routes (Protected)
// -------------------------

// שליפת כל המשימות
app.MapGet("/tasks", async (ToDoDbContext db) => await db.MyTables.ToListAsync())
    .RequireAuthorization();

// שליפת משימה לפי ID
app.MapGet("/tasks/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.MyTables.FindAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
})
    .RequireAuthorization();

// הוספת משימה חדשה
app.MapPost("/tasks", async (MyTable newItem, ToDoDbContext db) =>
{
    db.MyTables.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{newItem.Id}", newItem);
})
    .RequireAuthorization();

// עדכון משימה
app.MapPut("/tasks/{id}", async (int id, MyTable updatedItem, ToDoDbContext db) =>
{
    var item = await db.MyTables.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
    .RequireAuthorization();

// מחיקת משימה
app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.MyTables.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.MyTables.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
    .RequireAuthorization();

// -------------------------
// JWT Auth Routes
// -------------------------

app.MapPost("/auth/register", async (ToDoDbContext db, UserDto userDto, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation($"Register attempt: {userDto.UserName}");

        if (string.IsNullOrWhiteSpace(userDto.UserName) || string.IsNullOrWhiteSpace(userDto.Password))
        {
            logger.LogWarning("Username or password empty");
            return Results.BadRequest("Username and password are required");
        }

        var exists = await db.Users.AnyAsync(u => u.UserName == userDto.UserName);
        if (exists)
        {
            logger.LogWarning($"User already exists: {userDto.UserName}");
            return Results.BadRequest("Username already exists");
        }

        var user = new User
        {
            UserName = userDto.UserName,
            PasswordHash = userDto.Password // לצורך הפשטות בלבד
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        logger.LogInformation($"User created: {userDto.UserName}");
        return Results.Ok("User created");
    }
    catch (Exception ex)
    {
        logger.LogError($"Register error: {ex.Message}");
        return Results.StatusCode(500);
    }
});

app.MapPost("/auth/login", async (ToDoDbContext db, UserDto loginDto) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == loginDto.UserName);

    if (user == null || user.PasswordHash != loginDto.Password)
    {
        return Results.Unauthorized();
    }

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim("userId", user.Id.ToString())
    };

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new { token = tokenString });
});

// -------------------------
// הפעלה
// -------------------------
app.Run();