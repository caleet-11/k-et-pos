var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURACIÓN DE CORS (Permitir conexiones externas)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// 2. ACTIVAR CORS (Debe ir antes de MapControllers)
app.UseCors("PermitirTodo");

app.UseAuthorization();
app.MapControllers();

app.Run();