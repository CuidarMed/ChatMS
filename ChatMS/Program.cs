using Application.Interfaces;
using Application.Services;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using ChatMS.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar serialización de fechas en formato ISO 8601 con UTC
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // Las fechas DateTime se serializarán en formato ISO 8601
        // Por defecto, ASP.NET Core serializa DateTime en formato ISO 8601
        options.JsonSerializerOptions.WriteIndented = false;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR();

// HttpClient para llamadas a otros microservicios
builder.Services.AddHttpClient();

// DbContext - debe configurarse antes de Build()
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));
}

// Registrar Commands y Queries
builder.Services.AddScoped<Application.Interfaces.ICreateChatRoomCommand, Infrastructure.Command.CreateChatRoomCommand>();
builder.Services.AddScoped<Application.Interfaces.IGetChatRoomByIdQuery, Infrastructure.Queries.GetChatRoomByIdQuery>();
builder.Services.AddScoped<Application.Interfaces.IGetUserChatRoomsQuery, Infrastructure.Queries.GetUserChatRoomsQuery>();
builder.Services.AddScoped<Application.Interfaces.IGetChatMessagesQuery, Infrastructure.Queries.GetChatMessagesQuery>();
builder.Services.AddScoped<Application.Interfaces.ISendMessageCommand, Infrastructure.Command.SendMessageCommand>();
builder.Services.AddScoped<Application.Interfaces.IMarkMessagesAsReadCommand, Infrastructure.Command.MarkMessagesAsReadCommand>();

// Registrar servicios de aplicación
builder.Services.AddScoped<ICreateRoomService, CreateRoomService>();
builder.Services.AddScoped<IGetUserChatRoomsService, GetUserChatRoomsService>();
builder.Services.AddScoped<IGetChatRoomService, GetChatRoomService>();
builder.Services.AddScoped<IGetChatMessagesService, GetChatMessagesService>();
builder.Services.AddScoped<ISendMessageService, SendMessageService>();
builder.Services.AddScoped<IMarkMessagesAsReadService, MarkMessagesAsReadService>();

// CORS - Configuración más permisiva para SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5500", 
                "http://127.0.0.1:5500", 
                "http://localhost:3000",
                "http://localhost:8000",
                "http://127.0.0.1:8000"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Necesario para SignalR
    });
});

var app = builder.Build();

// Aplicar migraciones si la base de datos está configurada
if (!string.IsNullOrEmpty(connectionString))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Log error pero continuar
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "No se pudieron aplicar migraciones. La base de datos puede no estar disponible.");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS debe estar ANTES de cualquier otro middleware
app.UseCors();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// SignalR Hub - debe estar después de UseRouting pero antes de MapControllers
app.MapHub<ChatHub>("/chathub");

app.MapControllers();

app.Run();
