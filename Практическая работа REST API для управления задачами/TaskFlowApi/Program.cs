using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TaskFlowApi.Data;
using TaskFlowApi.Exceptions;
using TaskFlowApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc().AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskFlow API", Version = "v1", Description = "Устаревший контракт v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "TaskFlow API", Version = "v2" });
    options.DocInclusionPredicate((name, api) => api.GroupName == name);
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml)) options.IncludeXmlComments(xml, includeControllerXmlComments: true);
});
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow v1 (deprecated)");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "TaskFlow v2");
});
app.UseRouting();
app.UseCors("Frontend");
app.UseMiddleware<IdempotencyMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
