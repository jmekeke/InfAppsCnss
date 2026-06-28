using Cnss.Metier.CommunicationInterne.Application;
using Cnss.Metier.CommunicationInterne.Infrastructure;
using Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;
using Cnss.Metier.Shared.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// --- Serilog bootstrap
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    // --- Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/communication-interne-api-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

    // --- Application + Infrastructure
    builder.Services.AddCommunicationInterneApplication();
    builder.Services.AddCommunicationInterneInfrastructure(builder.Configuration);
    builder.Services.AddMDiatorValidation();

    // --- JWT
    var authority = builder.Configuration["RubacCore:Authority"]
                    ?? throw new InvalidOperationException("Configuration 'RubacCore:Authority' manquante.");
    var audience  = builder.Configuration["RubacCore:Audience"] ?? "cnss_metier_api";

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience  = audience;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidIssuer              = authority,
                ValidateAudience         = true,
                ValidAudience            = audience,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    Log.Warning(ctx.Exception, "JWT auth failed: {Message}", ctx.Exception.Message);
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();
    builder.Services.AddProblemDetails();

    // --- OpenAPI
    builder.Services.AddOpenApi(options =>
    {
        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type == typeof(Guid) || context.JsonTypeInfo.Type == typeof(Guid?))
            {
                schema.Type    = Microsoft.OpenApi.JsonSchemaType.String;
                schema.Format  = "uuid";
                schema.Default = null;
                schema.Example = null;
            }
            return Task.CompletedTask;
        });
    });

    // --- API versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader  = new Asp.Versioning.HeaderApiVersionReader("X-Api-Version");
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // --- MVC
    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
            opts.JsonSerializerOptions.ReferenceHandler =
                System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    // --- CORS
    builder.Services.AddCors(options =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:4200", "https://localhost:4200" };
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
    });

    // --- Health checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<CommunicationInterneDbContext>("db-communication-interne");

    var app = builder.Build();

    app.MapOpenApi();

    // --- Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"]  = "nosniff";
        context.Response.Headers["X-Frame-Options"]         = "DENY";
        context.Response.Headers["X-XSS-Protection"]        = "0";
        context.Response.Headers["Referrer-Policy"]         = "strict-origin-when-cross-origin";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
        await next();
    });

    app.UseCors();
    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging(options =>
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "anonymous"));

    app.UseMiddleware<ComInterne.Api.Middleware.ExceptionMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
