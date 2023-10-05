var builder = WebApplication.CreateBuilder(args);

//// Register HttpClient services
//builder.Services.AddHttpClient();
//builder.Services.AddHttpClient("AddServiceClient", c =>
//{
//    c.BaseAddress = new Uri("http://add-service");
//    // Other configurations
//});
//builder.Services.AddHttpClient("SubServiceClient", c =>
//{
//    c.BaseAddress = new Uri("http://sub-service");
//    // Other configurations
//});

// Add services to the container.
builder.Services.AddControllersWithViews();



// Add controllers and views if not already added
//builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Calculator/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=Index}/{id?}");

app.Run();


