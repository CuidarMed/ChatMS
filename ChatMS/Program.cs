using Application.Interfaces;
using Application.Services;
using ChatMS.Hubs;
using Infrastructure.Command;
using Infrastructure.Persistence.Configuration;
using Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ChatMS API",
        Version = "v1",
        Description = "Microservicio de Chat en tiempo real con SignalR"
    });
});

//dbcontext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

// Memory Cache
builder.Services.AddMemoryCache();

// Cache Service
builder.Services.AddSingleton<IUserCacheService, UserCacheService>();

// Commands
builder.Services.AddScoped<ICreateChatRoomCommand, CreateChatRoomCommand>();
builder.Services.AddScoped<ISendMessageCommand, SendMessageCommand>();
builder.Services.AddScoped<IMarkMessagesAsReadCommand, MarkMessagesAsReadCommand>();

// Queries
builder.Services.AddScoped<IGetChatRoomByIdQuery, GetChatRoomByIdQuery>();
builder.Services.AddScoped<IGetUserChatRoomsQuery, GetUserChatRoomsQuery>();
builder.Services.AddScoped<IGetChatMessagesQuery, GetChatMessagesQuery>();

// Services
builder.Services.AddScoped<IGetChatRoomService, GetChatRoomService>();
builder.Services.AddScoped<ISendMessageService, SendMessageService>();
builder.Services.AddScoped<IMarkMessagesAsReadService, MarkMessagesAsReadService>();
builder.Services.AddScoped<ICreateRoomService, CreateRoomService>();
builder.Services.AddScoped<IGetUserChatRoomsService, GetUserChatRoomsService>();
builder.Services.AddScoped<IGetChatMessagesService, GetChatMessagesService>();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100 KB
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowChatClient", policy =>
    {
        policy.WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "http://127.0.0.1:5501",  // Por si Live Server usa otro puerto
                "http://localhost:5501")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Apply migrations with retry logic
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    const int maxRetries = 10;
    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Applying migrations... Attempt {Attempt} of {MaxRetries}", attempt, maxRetries);
            dbContext.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying migrations on attempt {Attempt} of {MaxRetries}", attempt, maxRetries);
            if (attempt == maxRetries)
            {
                logger.LogCritical("Max migration attempts reached. Exiting application.");
                throw;
            }
            await Task.Delay(TimeSpan.FromSeconds(3)); // Wait before retrying
        }
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowChatClient");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.MapGet("/", () => Results.Ok(new
{
    service = "ChatMS",
    version = "1.0.0",
    status = "running",
    endpoints = new
    {
        hub = "/chatHub",
        api = "/api/chat",
        health = "/api/chat/health"
    }
}));

app.Run();
