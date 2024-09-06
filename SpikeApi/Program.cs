using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Spike.DataAccess;
using Spike.DataAccess.Entities;
using Spike.DataAccess.Implementation;
using Spike.DataAccess.Interface;
using Spike.Services.Implementation;
using Spike.Services.Interface;
using SpikeApi.Common;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<SpikeDbContext>(options =>
{
    options.UseInMemoryDatabase("SpikeDb");
});

var config = builder.Configuration;
var myCorsPolicy = new MyCorsPolicy();
builder.Services.AddCors(options =>
{
    options.AddPolicy("_mycorspolicy", policy =>
    {
        policy.WithOrigins(myCorsPolicy.AllowedOrgin)
              .WithHeaders(new string[] { "Authorization", "Content-Type" })
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddIdentity<SpikeUser, IdentityRole>(options =>
{
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = false;
    

}).AddEntityFrameworkStores<SpikeDbContext>()
  .AddDefaultTokenProviders();

//Registering the dependencies for Dependency injection
builder.Services.AddSingleton<ILoggerFactory, LoggerFactory>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenFactory, JwtTokenFactory>();



var secretKey = config["JwtSettings:SecretKey"];
var issuer = config["JwtSettings:Issuer"];
var audience = config["JwtSettings:Audience"];
var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

//Only for test purpose writing passwords here. In real-time, key-vault is preferred choice
Environment.SetEnvironmentVariable("SuperAdminPwd", "Tests1Up81");
Environment.SetEnvironmentVariable("AdminPwd", "a8dm1nsP72");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(config =>
{
    config.IncludeErrorDetails = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        RequireExpirationTime=true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        ValidateAudience = true,
        ValidateIssuer = true,
        IssuerSigningKey = secret,
        ValidateLifetime = true,   
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CheckRole", 
        policy => policy.RequireClaim(claimType: ClaimTypes.Role, ["SuperAdmin", "Admin"]));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Seeding data for SuperAdmin and Admin role.
using(var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] {"SuperAdmin", "Admin", "User" };
    
    foreach (var role in roles)
    {
        var identityRole = await roleManager.CreateAsync(new IdentityRole {   
                                                    Name = role, 
                                                    NormalizedName = role 
                                                });
    }
}

//Creating SuperAdmin and Admin user and seeding data.
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SpikeUser>>();
    
    

    byte[] salt = Encoding.UTF8.GetBytes(config["PasswordSalt"]);

    //2. Selecting the hashing algo and generate derivation key.
    byte[] keyDerivation = KeyDerivation.Pbkdf2(Environment.GetEnvironmentVariable("AdminPwd"), 
                                                            salt, 
                                                            KeyDerivationPrf.HMACSHA512, 
                                                            100000, 
                                                            numBytesRequested: 256 / 8);

    //3. Generate hashed password from derivation key
    string encrPassword = Convert.ToBase64String(keyDerivation);

    var superAdminUser = new SpikeUser
    {
        FirstName = "SuperAdministrator",
        LastName = "",
        UserName = "superadmin@gmail.com",
        Email = "superadmin@gmail.com",
        PasswordHash= encrPassword,
        EmailConfirmed = true
    };

    var adminUser = new SpikeUser
    {
        FirstName = "Administrator",
        LastName = "",
        UserName = "admin@gmail.com",
        Email = "admin@gmail.com",
        PasswordHash = encrPassword,
        EmailConfirmed = true
        
    };

    //Assigning roles to SuperAdmin and Admin users
    var adminUsers = new List<SpikeUser> { superAdminUser, adminUser};
    
    foreach (var user in adminUsers)
    {
        var userAdded = await userManager.CreateAsync(user);

        string roleToAdd = user.FirstName.Contains("Super", StringComparison.OrdinalIgnoreCase) ? "SuperAdmin": "Admin";

        var identityResult = await userManager.AddToRoleAsync(user, roleToAdd);
    }
}

//app.UseRouting();

app.UseCors("_mycorspolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
