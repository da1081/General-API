using Data;
using Data.Entities.Identity;
using Data.TokenProviders;
using Hangfire;
using Hangfire.SqlServer;
using MailTemplate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.ConfigurationModels;
using Services.Interfaces;
using Services.Services;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure database connections.
string connectionString = builder.Configuration.GetConnectionString("DevelopmentSQL");
string connectionStringHangfire = builder.Configuration.GetConnectionString("DevelopmentHangfire");

/// Add services to the container.
 
// Add Controllers.
builder.Services.AddControllers();

// Add DbContexts.
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connectionString));

// Add Identity and configure store + token providers.
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<SecurityPinProvider>(SecurityPinProvider.TotpProvider)
    .AddTokenProvider<SecurityPinProvider>(SecurityPinProvider.EmailProvider)
    .AddTokenProvider<SecurityPinProvider>(SecurityPinProvider.PhoneProvider)
    .AddTokenProvider<SecurityPinProvider>(SecurityPinProvider.PasswordResetProvider);

// Identity options. 
builder.Services.Configure<IdentityOptions>(options =>
{
    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // SignIn settings.
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // User settings.
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+!?";
    options.User.RequireUniqueEmail = true;

    // Token provider settings.
    options.Tokens.EmailConfirmationTokenProvider = SecurityPinProvider.EmailProvider;
    options.Tokens.ChangePhoneNumberTokenProvider = SecurityPinProvider.PhoneProvider;
    options.Tokens.PasswordResetTokenProvider = SecurityPinProvider.PasswordResetProvider;
});

// Token timeout settings.
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
       options.TokenLifespan = TimeSpan.FromMinutes(3));

// Add JWT authentication scheme.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = true;  // TODO : Prepare : Set true in production.
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,  // TODO : Prepare : Set true in production.
        ValidateAudience = true,  // TODO : Prepare : Set true in production.
        ValidIssuer = builder.Configuration.GetValue<string>("AppSettings:ValidIssuer"),
        ValidAudience = builder.Configuration.GetValue<string>("AppSettings:ValidAudience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("AppSettings:Secret")))
    };
});

// Add UnitOfWork as scoped service.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IConfirmationService, ConfirmationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ISmsService, SmsService>();

// Add template render service.
builder.Services.AddScoped<ITemplateRenderService, TemplateRenderService>();
// builder.Services.AddSingleton<ITemplateRenderService, TemplateRenderService>();

// Add/configure IOptions
builder.Services.Configure<PopSettingsModel>(builder.Configuration.GetSection("PopSettings"));
builder.Services.Configure<SmtpSettingsModel>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<SmsSettingsModel>(builder.Configuration.GetSection("SmsServiceSettings"));
builder.Services.Configure<ApplicationIdentityModel>(builder.Configuration.GetSection("ApplicationIdentity"));

/// Add and configure Hangfire.

// Configure Hangfire service.
builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionStringHangfire, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

// Add Hangfire default queue server.
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:default";
    options.Queues = new[] { "default" };
});


/// Add and configure Swagger OpenAPI.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to accept a bearer JWT token when trying methods. 
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard authorization header using the Bearer -scheme. Input (\"bearer {insert-token-here}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});


var app = builder.Build();

/// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHsts();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

/// Insure database, create seed data, create admin and/or other initialization.
using (var scope = app.Services.CreateScope())
{
    IServiceProvider serviceProvider = scope.ServiceProvider;

    // Ensure database is created.
    ApplicationContext context = serviceProvider.GetRequiredService<ApplicationContext>();
    bool contextCreated = context.Database.EnsureCreated();

    // Add default roles.
    var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    string[] roles = new string[] { "Administrator", "Moderator" };
    foreach (var role in roles)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new ApplicationRole(role));

    // Add default admin.
    string mail = builder.Configuration.GetValue<string>("DefaultUser:Mail");
    UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    ApplicationUser? user = await userManager.FindByEmailAsync(mail);
    if (user is null)
    {
        IdentityResult createNewUser = await userManager.CreateAsync(
            new ApplicationUser() { UserName = builder.Configuration.GetValue<string>("DefaultUser:Name"), Email = mail, EmailConfirmed = true },
            builder.Configuration.GetValue<string>("DefaultUser:Pass"));

        if (createNewUser.Succeeded)
            user = await userManager.FindByEmailAsync(mail) ?? throw new Exception("New default user cannot be found. Roles cannot be applyed on default user.");
        await userManager.AddToRolesAsync(user!, roles);
    }
}
app.Run();
