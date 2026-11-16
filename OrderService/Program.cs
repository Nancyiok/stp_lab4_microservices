using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OrderService.Data;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));

// in docker this is overridden by Services__UserServiceUrl
builder.Services.AddHttpClient<UserClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:UserServiceUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderService", Version = "v1" });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "OrderService.xml"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService v1");
});

app.MapControllers();

app.Run();
