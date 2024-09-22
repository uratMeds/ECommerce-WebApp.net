using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using Bulky.DataAccess.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Telling our project to use E F Core and that the DbContext will be using SQL Server.
// This code registers ApplicationDbContext with the DI container,
// specifying that it should use a SQL Server database and providing the connection string.
builder.Services.AddDbContext<ApplicationDbContext>(options => 
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Since the props of Stripe in appSettings match those of the utility class it will auto. inject values.
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// We are configuring this so we can se these pages bcz initially the automatic path was wrong.
// We must add it after Identity.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});


builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "1018861963327484";
    option.AppSecret = "d8f4064d5ec6d8125788fcb8bd8a3774";
});


builder.Services.AddAuthentication().AddMicrosoftAccount(option =>
{
    option.ClientId = "66f5becd-729d-4800-9733-fa582a6543bd";
    option.ClientSecret = "hRg8Q~CqIiEfm~obTIM5eAGAgv1pgKX4CHYEIdvW";
});

// This is how we add a session.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Neccessary for the Login and Register to work.
builder.Services.AddRazorPages();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

// Now that we switched from ApplicationDbContext to ICategoryRepository we need to register the service.
// builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
// Now that we added IUnitOfWork we need to replace the previous line by:
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IEmailSender, EmailSender>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseRouting();
app.UseAuthentication(); // Checks if Usrnm or Pswd are valid, if so then comes Authoriz., that's why it comes after.
app.UseAuthorization(); // Devides access based on roles.

app.UseSession();

SeedDatabase();

// We added this line to be able access the Login and Register pages because they are Razor Pages.
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}