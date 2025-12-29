using TemplatePrinting.Controllers;
using PrintingLibrary.Setup;
using PrintingLibrary.EmbeddedResourcesUtils;
using TemplatePrinting.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string Origins = "_myAllowSpecificOrigins";

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => {
  options.AddPolicy(name: Origins,
                    policy => {
                      policy
                                        .AllowAnyOrigin()
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                    });
});

builder.Services.AddSingleton<IPrintingSetup>(new PrintingSetup());
// builder.Services.AddSingleton<IJobCountStrategy, ManagedJobCountStrategy>();
builder.Services.AddSingleton<IJobCountStrategy, NativeJobCountStrategy>();

builder.Services.AddSingleton(new Resources<Assets>());

if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) {
  builder.Services.AddSingleton<PrinterBackgroundService>();
}

var app = builder.Build();

var utils = app.Services.GetRequiredService<IPrintingSetup>();
utils.Setup();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Removed to avoid potential redirect loops locally

app.UseCors(Origins);

// Middleware to handle case-insensitive static file requests on Linux
app.Use(async (context, next) => {
  var path = context.Request.Path.Value;
  if (!string.IsNullOrEmpty(path) && path != "/") {
    var webRoot = app.Environment.WebRootPath;
    var relativePath = path.TrimStart('/');
    var fullPath = Path.Combine(webRoot, relativePath);

    // If file/directory doesn't exist with exact casing, try case-insensitive match
    if (!File.Exists(fullPath) && !Directory.Exists(fullPath)) {
      var parts = relativePath.Split('/');
      var currentPath = webRoot;
      var foundMatch = true;

      foreach (var part in parts) {
        if (string.IsNullOrEmpty(part)) continue;
        var match = Directory.GetFileSystemEntries(currentPath)
            .FirstOrDefault(e => string.Equals(Path.GetFileName(e), part, StringComparison.OrdinalIgnoreCase));

        if (match != null) {
          currentPath = match;
        } else {
          foundMatch = false;
          break;
        }
      }

      if (foundMatch) {
        var newPath = "/" + Path.GetRelativePath(webRoot, currentPath).Replace("\\", "/");
        context.Request.Path = newPath;
      }
    }
  }
  await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
