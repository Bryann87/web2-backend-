using Microsoft.EntityFrameworkCore;
using academia.Data;
using academia.Services;
using academia.Middleware;
using academia.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;

// Configurar PostgreSQL para usar UTC
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

// Cargar variables de entorno desde .env
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno desde .env
DotNetEnv.Env.Load();

// Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agregar SignalR para WebSocket
builder.Services.AddSignalR();

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsProduction() && OperatingSystem.IsWindows())
{
    builder.Logging.AddEventLog();
}

// Configurar DbContext con PostgreSQL
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
                      ?? builder.Configuration.GetConnectionString("AcademiaDb");

builder.Services.AddDbContext<AcademiaContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention() // Convención snake_case
);

// Registrar servicios
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IMappingService, MappingService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
    
    // Política permisiva para desarrollo (archivos locales y cualquier origen)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ----- Configuración de JWT con validación robusta -----
// Deshabilitar el mapeo automático de claims para mantener los nombres originales del JWT
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Obtener la clave JWT desde variables de entorno o configuración
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                       ?? builder.Configuration["Jwt:SecretKey"];
        
        if (string.IsNullOrEmpty(secretKey))
            throw new Exception("Falta JWT_SECRET_KEY en variables de entorno o Jwt:SecretKey en configuración");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero // Eliminar tolerancia de tiempo por defecto
        };

        // Configurar SignalR para usar JWT desde query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
// -------------------------------------------------------

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configurar CORS - debe ir antes de UseHttpsRedirection para manejar preflight requests
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowFrontend");
}

app.UseHttpsRedirection();

// Agregar middleware de logging
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication(); // Importante, antes de UseAuthorization
app.UseAuthorization();

// Agregar middleware de autorización personalizado (después de UseAuthorization)
app.UseMiddleware<AuthorizationMiddleware>();

app.MapControllers();

// Mapear el Hub de SignalR
app.MapHub<NotificacionHub>("/hubs/notificaciones");

app.Run();
