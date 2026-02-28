using AppleWalletPassGenerator.IServices;
using AppleWalletPassGenerator.Models;
using AppleWalletPassGenerator.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PassSettings>(builder.Configuration.GetSection("PassSettings"));

builder.Services.AddDbContext<PassDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPassGeneratorService, PassGeneratorService>();
builder.Services.AddScoped<IPassDataService, PassDataService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();

builder.Services.AddHttpClient<IPushNotificationService, PushNotificationService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PassDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
