using dotenv.net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using MySqlConnector;
using StackExchange.Redis;
using tda26.Server.Classes;
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

        builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        // openapi generator (vestaveny v asp.net core)
        builder.Services.AddOpenApi();
        builder.Services.AddHttpClient();


        // databaze source service
        builder.Services.AddKeyedSingleton<MySqlDataSource>("main", (sp, key) => {
            // emsio data source
            var cs = $"server={ENV["DATABASE_IP"]};user id={ENV["DATABASE_DBNAME"]};password={ENV["DATABASE_PASSWORD"]};database={ENV["DATABASE_DBNAME"]};pooling=true;Max Pool Size=300;";
            var dsb = new MySqlDataSourceBuilder(cs);
            return dsb.Build();
        });

        builder.Services.AddKeyedSingleton<MySqlDataSource>("fallback", (sp, key) => {
            // fallback data source (container db)
            const string cs = $"server=localhost:3306;user id=tda26;password=tda26;database=tda26;pooling=true;Max Pool Size=300;";
            var dsb = new MySqlDataSourceBuilder(cs);
            return dsb.Build();
        });

        // repozitare a service
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
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