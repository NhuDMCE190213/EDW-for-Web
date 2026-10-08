using PresentationMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("ApiSettings:BaseUrl must be configured.");
    }
    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
};

builder.Services.AddHttpClient<PromotionApiClient>(configureApiClient);
builder.Services.AddHttpClient<AuthApiClient>(configureApiClient);
builder.Services.AddHttpClient<CategoryApiClient>(configureApiClient);
builder.Services.AddHttpClient<ProfileApiClient>(configureApiClient);
builder.Services.AddHttpClient<ProductVariantApiClient>(configureApiClient);
builder.Services.AddHttpClient<ProductApiClient>(configureApiClient);

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

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
