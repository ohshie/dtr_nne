using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Extensions;
using dtr_nne.Domain.IContext;
using dtr_nne.Infrastructure.Extensions;
using dtr_nne.Middleware;
using Microsoft.AspNetCore.Authentication;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) 
    => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddDomain(builder.Configuration);

builder.Services.Configure<Auth.AuthSettings>(builder.Configuration.GetSection("Authentication"));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddScheme<AuthenticationSchemeOptions, Auth>("Bearer", _ => { });

builder.Services.AddHttpClient();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<INneDbContext>();
    
    await dbContext.EnsureCreatedAsync();
}

await app.RunAsync();