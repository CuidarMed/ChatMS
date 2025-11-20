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
