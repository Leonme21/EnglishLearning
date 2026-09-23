using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Infrastructure.Persistence;
using EnglishLearning.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Inyección de Dependencias (DI)
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o => o.SuppressModelStateInvalidFilter = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Base de Datos
builder.Services.AddDbContext<EnglishLearningDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios (Scoped)
builder.Services.AddScoped<ILeccionRepositorio, LeccionRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IAssessmentRepositorio, AssessmentRepositorio>();

// Servicios de Aplicación (Scoped)
builder.Services.AddScoped<ILeccionServicio, LeccionServicio>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();

// CORS para Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Seed Assessment preguntas si DB disponible (no bloquea si no hay conexión)
try
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<EnglishLearning.Infrastructure.Persistence.EnglishLearningDbContext>();
    // EnsureCreated para SQLite/dev sin migraciones; en SqlServer usar Migrate() si existe
    if (ctx.Database.CanConnect()) await EnglishLearning.Infrastructure.Seed.AssessmentSeed.SeedAsync(ctx);
}
catch { /* seed opcional: no falla startup si DB no configurada */ }

// CorrelationId + security headers (observability + security-and-hardening:53)
app.Use(async (ctx, next) =>
{
    var correlationId = ctx.Request.Headers["X-Request-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    ctx.Response.Headers["X-Request-Id"] = correlationId;
    ctx.Items["CorrelationId"] = correlationId;
    // Security headers
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseHttpsRedirection();
app.UseCors("AngularPolicy");
app.UseAuthorization();
app.MapControllers();

app.Run();
