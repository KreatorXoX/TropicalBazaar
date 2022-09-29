using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TropicalBazaar.DataAccess;
using TropicalBazaar.DataAccess.Repository;
using TropicalBazaar.DataAccess.Repository.IRepository;
using TropicalBazaar.Utility;

var builder = WebApplication.CreateBuilder(args);


/*why the fuck this is added after identity scaffolded*/
//var connectionString = builder.Configuration.GetConnectionString("AppDbContextConnection");;

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(connectionString));;



// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("Default")
    ));

//populating utility class stripe settings with the data thats given in the appsettings.json

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));


builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

//adding session to our program.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//adding google authentication
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = "165910447008-1ihoe2iq1p7l15kmnanb0uso0u503bd2.apps.googleusercontent.com";
    options.ClientSecret = "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";
});


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

app.UseRouting();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings:SecretKey").Get<string>();

app.UseAuthentication();

app.UseAuthorization();
app.UseSession();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
