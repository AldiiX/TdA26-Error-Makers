using dotenv.net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Minio;
using Minio.AspNetCore;
using MySqlConnector;
using StackExchange.Redis;
using tda26.Server.Controllers;
using tda26.Server.Data;
using tda26.Server.DTOs.Converters;
using tda26.Server.Infrastructure;
using tda26.Server.Options;
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
        
        
        // Disable detailed logging
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        builder.Logging.AddFilter("System.Net.Http.HttpClient.ServiceChecker.LogicalHandler", LogLevel.Warning);
        builder.Logging.AddFilter("System.Net.Http.HttpClient.ServiceChecker.ClientHandler", LogLevel.Warning);





        // ----------------------
        // REDIS (SESSION)
        // ----------------------

        // pripojeni k redisu
        var rhost = ENV.GetValueOrNull("REDIS_IP") ?? ENV.GetValueOrNull("DATABASE_IP") ?? "127.0.0.1";
        var rport = ENV.GetValueOrNull("REDIS_PORT") ?? "6379";
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





        // ----------------------
        // MYSQL DATABAZE
        // ----------------------

        // Primary Connection Configuration
        var primaryConnectionStringBuilder = new MySqlConnectionStringBuilder {
            Server = ENV.GetValueOrNull("DATABASE_IP") ?? "localhost",
            UserID = ENV.GetValueOrNull("DATABASE_USER") ?? "tda26",
            Password = ENV.GetValueOrNull("DATABASE_PASSWORD") ?? "tda26",
            Database = ENV.GetValueOrNull("DATABASE_DBNAME") ?? "tda26",
            Pooling = true,
            Port = 3306,
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
            Port = 3306,
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
                ? string.Concat(cs.AsSpan(0, cs.IndexOf("Password=", StringComparison.Ordinal) + 9), "****")
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
        builder.Services.AddControllers().AddJsonOptions(options => {
            options.JsonSerializerOptions.Converters.Add(new QuestionRequestConverter());
        });
        






        // ----------------------
        // MINIO STORAGE
        // ----------------------
        
        // Minio with fallback support
        string? minioEndpoint = null;
        string? minioAccessKey = null;
        string? minioSecretKey = null;
        string? minioConnectionName = null;
        bool minioUseSSL = false;

        // Try primary MinIO from environment variables
        var primaryEndpoint = ENV.GetValueOrNull("MINIO_ENDPOINT");
        var primaryAccessKey = ENV.GetValueOrNull("MINIO_ACCESS_KEY");
        var primarySecretKey = ENV.GetValueOrNull("MINIO_SECRET_KEY");
        var primaryUseSSL = ENV.GetValueOrNull("MINIO_USE_SSL")?.ToLower() == "true";

        if (!string.IsNullOrWhiteSpace(primaryEndpoint) && 
            !string.IsNullOrWhiteSpace(primaryAccessKey) && 
            !string.IsNullOrWhiteSpace(primarySecretKey))
        {
            try {
                Console.WriteLine($"Attempting connection to Primary MinIO: {primaryEndpoint}");
                
                // Test connection
                var testClient = new MinioClient()
                    .WithEndpoint(primaryEndpoint)
                    .WithCredentials(primaryAccessKey, primarySecretKey)
                    .WithSSL(primaryUseSSL)
                    .Build();

                // Simple health check - list buckets with timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var bucketsTask = testClient.ListBucketsAsync(cts.Token);
                bucketsTask.GetAwaiter().GetResult();

                minioEndpoint = primaryEndpoint;
                minioAccessKey = primaryAccessKey;
                minioSecretKey = primarySecretKey;
                minioUseSSL = primaryUseSSL;
                minioConnectionName = "Primary";
                Console.WriteLine($"Successfully connected to Primary MinIO: {primaryEndpoint}");
            } catch (Exception ex) {
                Console.WriteLine($"Failed to connect to Primary MinIO ({primaryEndpoint}). Error: {ex.Message}");
            }
        } else {
            Console.WriteLine("Primary MinIO configuration incomplete, checking environment variables...");
        }

        // Fallback to local MinIO
        if (minioEndpoint == null) {
            const string fallbackEndpoint = "127.0.0.1:9000";
            const string fallbackAccessKey = "admin";
            const string fallbackSecretKey = "adminadmin";

            try {
                Console.WriteLine($"Attempting connection to Fallback MinIO: {fallbackEndpoint}");
                
                // Test connection without SSL for local
                var testClient = new MinioClient()
                    .WithEndpoint(fallbackEndpoint)
                    .WithCredentials(fallbackAccessKey, fallbackSecretKey)
                    .WithSSL(false)
                    .Build();

                // Simple health check - list buckets with timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var bucketsTask = testClient.ListBucketsAsync(cts.Token);
                bucketsTask.GetAwaiter().GetResult();

                minioEndpoint = fallbackEndpoint;
                minioAccessKey = fallbackAccessKey;
                minioSecretKey = fallbackSecretKey;
                minioUseSSL = false;
                minioConnectionName = "Fallback";
                Console.WriteLine($"Successfully connected to Fallback MinIO: {fallbackEndpoint}");
            } catch (Exception ex) {
                Console.WriteLine($"Failed to connect to Fallback MinIO ({fallbackEndpoint}). Error: {ex.Message}");
            }
        }

        if (minioEndpoint == null) {
            Console.WriteLine("---------------------------------------------");
            throw new InvalidOperationException("CRITICAL: Failed to connect to both primary and fallback MinIO. Application startup aborted.");
        }

        Console.WriteLine($"--- Configuration using the {minioConnectionName} MinIO. ---");

        var finalEndpoint = minioEndpoint!;
        var finalAccessKey = minioAccessKey!;
        var finalSecretKey = minioSecretKey!;
        var finalUseSSL = minioUseSSL;

        builder.Services.AddMinio(options => {
            options.Endpoint = finalEndpoint;
            options.AccessKey = finalAccessKey;
            options.SecretKey = finalSecretKey;

            if (finalUseSSL) {
                options.ConfigureClient(client => {
                    client.WithSSL();
                });
            }
        });





        // repozitare a service
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IMaterialAccessService, MaterialAccessService>();
        builder.Services.AddSingleton<IFeedStreamBroker, InMemoryFeedStreamBroker>();
        
        // Nastaveni
        builder.Services.Configure<CustomMinioOptions>(options => {
            options.BucketName = ENV.GetValueOrNull("MINIO_BUCKET_NAME") ?? "tda26";
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddOpenApi();
        builder.Services.AddHttpClient();

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