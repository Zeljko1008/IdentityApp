using IdentityApp.Data;
using IdentityApp.Models;
using IdentityApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<IdentityApp.Data.Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<JWTservice>(); // Register JWT service for token generation

builder.Services.AddIdentityCore<User>(opt=>
{
        opt.Password.RequiredLength = 6;
        opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireNonAlphanumeric = false;
    opt.SignIn.RequireConfirmedEmail=true;
})
    .AddRoles<IdentityRole>() // Add roles support
    .AddRoleManager<RoleManager<IdentityRole>>() // Register RoleManager for role management
    .AddEntityFrameworkStores<Context>() // Use the custom DbContext
    .AddSignInManager<SignInManager<User>>() // Register SignInManager for user sign-in operations
    .AddUserManager<UserManager<User>>() // Register UserManager for user management operations
    .AddDefaultTokenProviders(); // Add default token providers for password reset, email confirmation, etc.

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Validate the token's signature
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateAudience = false // Set to true if you want to validate audience
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Enable authentication middleware before authorization

app.UseAuthorization();

app.MapControllers();

app.Run();
