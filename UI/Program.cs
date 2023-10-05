using Polly;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddHttpClient();


// Add services to the container.
builder.Services.AddControllersWithViews();

// Add HttpClient with Polly retry policy
builder.Services.AddHttpClient("MyClient")
    .AddTransientHttpErrorPolicy(p =>
        p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));

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


