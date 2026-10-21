using ConveyorScada.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<LiveValuesCache>();
builder.Services.AddHostedService<MqttListener>();   // підписник: MQTT -> кеш -> сторінка

var app = builder.Build();

app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
