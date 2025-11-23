using dotenv.net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using MySqlConnector;
using StackExchange.Redis;
using tda26.Server.Data;
using tda26.Server.Infrastructure;
using tda26.Server.Repositories;
using tda26.Server.Services;

namespace tda26.Server;

public static class Program {

    public static ILogger Logger { get; private set; } = null!;
    public static WebApplication Application { get; private set; } = null!;
    public static IDictionary<string, string> ENV { get; private set; } = DotEnv.Read();

    #if DEBUG
        public static readonly bool DevelopmentMode = true;
    #else
        public static readonly bool DevelopmentMode = false;
    #endif



    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // pripojeni k redisu
        var rhost = ENV.GetValueOrNull("REDIS_IP") ?? ENV["DATABASE_IP"];
        var rport = ENV["REDIS_PORT"];
        var rpassword = ENV.GetValueOrNull("REDIS_PASSWORD");

        var redis = ConnectionMultiplexer.Connect(new ConfigurationOptions {
            EndPoints = { $"{rhost}:{rport}" },
            AbortOnConnectFail = false,
            Password = string.IsNullOrWhiteSpace(rpassword) ? null : rpassword,
        });

        builder.Services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
            .SetApplicationName("tda26");

        builder.Services.AddSingleton<IDistributedCache>(sp =>
            new RedisCache(new RedisCacheOptions {
                ConfigurationOptions = ConfigurationOptions.Parse(redis.Configuration),
                InstanceName = "tda26_session"
            })
        );

        builder.Services.AddSession(options => {
            options.IdleTimeout = TimeSpan.FromDays(365);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.MaxAge = TimeSpan.FromDays(365);
            options.Cookie.Name = "tda26_session";
        });
        
        // Primary Connection Configuration
        var primaryConnectionStringBuilder = new MySqlConnectionStringBuilder {
            Server = ENV["DATABASE_IP"],
            UserID = "tda26",
            Password = ENV["DATABASE_PASSWORD"],
            Database = ENV["DATABASE_DBNAME"],
            Pooling = true,
            MaximumPoolSize = 300,
            AllowUserVariables = true,
            UseAffectedRows = false,
            ConnectionTimeout = 10
        };

        // Fallback Connection Configuration
        var fallbackConnectionStringBuilder = new MySqlConnectionStringBuilder {
            Server = "localhost",
            UserID = "tda26",
            Password = "tda26",
            Database = "tda26",
            Pooling = true,
            MaximumPoolSize = 300,
            AllowUserVariables = true,
            UseAffectedRows = false,
            ConnectionTimeout = 10
        };

        List<(string Name, string ConnectionString)> potentialConnections = [
            ("Primary", primaryConnectionStringBuilder.ConnectionString),
            ("Fallback", fallbackConnectionStringBuilder.ConnectionString)
        ];

        string? workingConnectionString = null;
        string? workingConnectionName = null;

        foreach (var (name, cs) in potentialConnections) {
            // Mask password in logs
            var displayCs = cs.Contains("Password=") 
                ? cs.Substring(0, cs.IndexOf("Password=", StringComparison.Ordinal) + 9) + "****" 
                : cs;

            try {
                Console.WriteLine($"Attempting connection to {name} database...");
                var testDataSource = new MySqlDataSourceBuilder(cs).Build();
                
                using (var connection = testDataSource.CreateConnection()) {
                    connection.OpenAsync();
                }

                workingConnectionString = cs;
                workingConnectionName = name;
                Console.WriteLine($"Successfully connected to {name} database: {displayCs}");
                break;
            }

            catch (Exception ex) {
                Console.WriteLine($"Failed to connect to {name} database ({displayCs}). Error: {ex.Message}");
            }
        }

        if (workingConnectionString == null) {
            Console.WriteLine("---------------------------------------------");
            throw new InvalidOperationException("CRITICAL: Failed to connect to both primary and fallback databases. Application startup aborted.");
        }

        Console.WriteLine($"--- Configuration using the {workingConnectionName} database. ---");

        builder.Services.AddSingleton(sp => new MySqlDataSourceBuilder(workingConnectionString).Build());

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                workingConnectionString,
                ServerVersion.AutoDetect(workingConnectionString),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                )
            )
        );

        builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        // openapi generator (vestaveny v asp.net core)
        builder.Services.AddOpenApi();
        builder.Services.AddHttpClient();

        // repozitare a service
        builder.Services.AddScoped<ICourseRepository, CourseRepository>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<ILecturerRepository, LecturerRepository>();

        Application = builder.Build();

        Application.UseDefaultFiles();
        Application.MapStaticAssets();

        Application.MapOpenApi("/_openapi/{documentName}.json");
        Application.UseSession();
        Application.UseAuthorization();
        Application.UseCors();

        #if DEBUG
        Application.UseSwaggerUI(o => {
            o.RoutePrefix = "_swagger";
            o.SwaggerEndpoint("/_openapi/v1.json", "Think different Academy API");
        });
        #endif

        Application.MapControllers();

        // pridani X-Powered-By
        Application.Use(async (context, next) => {
            context.Response.Headers.Append("X-Powered-By", "ASP.NET");
            await next.Invoke();
        });

        Logger = Application.Logger;
        Application.Run();
    }
}