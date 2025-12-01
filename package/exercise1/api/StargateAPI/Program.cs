using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dataDir);

// TODO: Fix this hack to specify the SQLite database path
// Update configuration to use absolute path for database
var dbPath = Path.Combine(dataDir, "starbase.db");
builder.Configuration["ConnectionStrings:StarbaseApiDatabase"] = $"Data Source={dbPath}";
builder.Configuration["Serilog:WriteTo:1:Args:sqliteDbPath"] = dbPath;

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<StargateContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Seed development data
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<StargateContext>();
    DatabaseSeeder.SeedDevelopmentData(context);
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Starting Stargate API application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


