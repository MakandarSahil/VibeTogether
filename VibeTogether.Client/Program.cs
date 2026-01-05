using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using VibeTogether.Client.Auth;
using VibeTogether.Client.Pages.Auth;
using VibeTogether.Client.Pages.Rooms;
using VibeTogether.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Data Protection to persist keys
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("VibeTogether");

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthStateProvider>());

builder.Services.AddScoped<LoginViewModel>();
builder.Services.AddScoped<RegisterViewModel>();

builder.Services.AddHttpClient("Api", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://api:8080";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<RoomState>();
builder.Services.AddScoped<CreateRoomViewModel>();
builder.Services.AddScoped<JoinRoomViewModel>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();