using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using R2Model.Context;
using R2Model.Entities;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string"
        + "'DefaultConnection' not found.");

builder.Services.AddDbContext<R2DbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
 .AddIdentityCookies();
builder.Services.AddAuthorizationBuilder();

builder.Services.AddIdentityCore<User>()
 .AddEntityFrameworkStores<R2DbContext>()
 .AddApiEndpoints();


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
