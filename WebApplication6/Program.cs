using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantProvider, RequestHeaderTenantProvider>();
builder.Services.AddScoped<ITenantResolver, DefaultTenantResolver>();

builder.Services.AddScoped<IAmazonDynamoDB>(
    provider =>
    {
        var tenantResolver = provider.GetRequiredService<ITenantResolver>();

        var connectionString = tenantResolver.GetTenant() switch
        {
            "canada" => "http://localhost:8001",
            _ => "http://localhost:8000"
        };

        // Fetch anything that might be needed (access keys, connection strings and what not) from a tenant configuration file (appsettings?)
        var credentials = new BasicAWSCredentials("hgc0wr", "v9hunh");
        
        var dynamoDbConfig = new AmazonDynamoDBConfig
        {
            ServiceURL = connectionString
        };

        return new AmazonDynamoDBClient(credentials, dynamoDbConfig);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

interface ITenantResolver
{
    string? GetTenant();
}

class DefaultTenantResolver : ITenantResolver
{
    private readonly HttpContext _httpContext;
    private readonly IEnumerable<ITenantProvider> _providers;

    public DefaultTenantResolver(IHttpContextAccessor httpContextAccessor, IEnumerable<ITenantProvider> providers)
    {
        _httpContext = httpContextAccessor.HttpContext;
        _providers = providers;
    }

    public string? GetTenant()
    {
        foreach (var provider in _providers)
        {
            var tenantId = provider.GetTenantId(_httpContext);
            if (tenantId is not null)
            {
                return tenantId;
            }
        }

        return null;
    }
}

interface ITenantProvider
{
    string? GetTenantId(HttpContext httpContext);
}

class RequestHeaderTenantProvider : ITenantProvider
{
    public string? GetTenantId(HttpContext httpContext)
    {
        var tenantId = httpContext.Request.Headers["X-Tenant"];
        if (tenantId == StringValues.Empty)
        {
            return null;
        }

        return tenantId;
    }
}