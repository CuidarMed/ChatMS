using Application.Interfaces;
using Application.Services;
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

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100 KB
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ChatMSPolicy", policy =>
    {
        policy.WithOrigins(
            builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ??
            new[] { "http://localhost:3000", "http://localhost:5173" })
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

app.UseCors("ChatMSPolicy");

app.UseRouting();

//app.MapHub<ChatHub>("/chatHub");

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

app.UseHttpsRedirection(); //posible para sacar

app.UseAuthorization(); // posible para sacar

app.MapControllers();

app.Run();
