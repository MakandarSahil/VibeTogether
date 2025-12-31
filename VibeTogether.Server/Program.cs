using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VibeTogether.Server.Configuration;
using VibeTogether.Server.Data;
using VibeTogether.Server.Hubs;
using VibeTogether.Server.Infrastructure.Generators;
using VibeTogether.Server.Infrastructure.Security;
using VibeTogether.Server.Services.Interfaces;
using Microsoft.OpenApi.Models;
using VibeTogether.Server.Services.Auth;
using VibeTogether.Server.Services.RoomServices;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings")
);
builder.Services.AddSingleton<MongoContext>();

builder.Services.AddSingleton<PasswordHashService>();
builder.Services.AddScoped<IAuthService, AuthServices>();
builder.Services.AddScoped<IRoomService, RoomService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtConfig = builder.Configuration.GetSection("Jwt");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(jwtConfig["Key"]!)  
            ),

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddSingleton<PresenceService>();
builder.Services.AddScoped<RoomPermissionService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<MusicSyncService>();



builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description= "Enter 'Bearer' [space] and then you token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();


app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<RoomHub>("/hubs/room");

app.Run();
