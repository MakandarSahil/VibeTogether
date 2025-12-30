using Microsoft.AspNetCore.SignalR;
using VibeTogether.Server.Configuration;
using VibeTogether.Server.Data;
using VibeTogether.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings")
);
builder.Services.AddSingleton<MongoContext>();

builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();


app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<RoomHub>("/hubs/room");

app.Run();
