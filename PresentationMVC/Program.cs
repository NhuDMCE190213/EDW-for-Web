using PresentationMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiAuthorizationHandler>();

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

builder.Services.AddHttpClient<CustomerOrderApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<CustomerCartApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<OrderApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<PromotionApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<AuthApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<CategoryApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<ProfileApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<ProductVariantApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<ProductApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<StaffApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<CustomerApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();
builder.Services.AddHttpClient<ReviewApiClient>(configureApiClient).AddHttpMessageHandler<ApiAuthorizationHandler>();

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
