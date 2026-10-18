using PresentationMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".EDW.Session";
});

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/CustomerLogin";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/Error";
        options.Cookie.Name = "EDW_AuthToken";
    });

Action<HttpClient> configureApiClient = client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7278";
    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
};

builder.Services.AddHttpClient<CustomerOrderApiClient>(configureApiClient);
builder.Services.AddHttpClient<CustomerCartApiClient>(configureApiClient);
builder.Services.AddHttpClient<OrderApiClient>(configureApiClient);
builder.Services.AddHttpClient<PromotionApiClient>(configureApiClient);
builder.Services.AddHttpClient<AuthApiClient>(configureApiClient);
builder.Services.AddHttpClient<CategoryApiClient>(configureApiClient);
builder.Services.AddHttpClient<ProfileApiClient>(configureApiClient);
builder.Services.AddHttpClient<ProductVariantApiClient>(configureApiClient);
builder.Services.AddHttpClient<ProductApiClient>(configureApiClient);
builder.Services.AddHttpClient<StaffApiClient>(configureApiClient);
builder.Services.AddHttpClient<CustomerApiClient>(configureApiClient);
builder.Services.AddHttpClient<ReviewApiClient>(configureApiClient);

builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
