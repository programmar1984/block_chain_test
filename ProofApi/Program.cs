var builder = WebApplication.CreateBuilder(args);

// Add services for controllers
builder.Services.AddControllers();

var app = builder.Build();

// Enable controller-based routing
app.MapControllers();

app.Run();