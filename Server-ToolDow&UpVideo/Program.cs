using Microsoft.EntityFrameworkCore;
using Server_ToolDow_UpVideo.Models;
using Server_ToolDow_UpVideo.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<InformationMeetingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InformationMeetingContext")));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ZoomService>();
// client for YouTube API and Zoom API
builder.Services.AddHttpClient("Zoom", c =>
{
    c.BaseAddress = new Uri("https://zoom.us/");
});
builder.Services.AddHttpClient("ZoomDownload", client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});

builder.Services.AddHttpClient("YouTube", c =>
{
    c.BaseAddress = new Uri("https://www.googleapis.com/");
});

builder.Services.AddScoped<IZoomService, ZoomService>();
builder.Services.AddScoped<IYouTubeService, YouTubeUploadService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
