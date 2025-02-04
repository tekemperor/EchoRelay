using EchoRelay.Core.Server;
using Serilog;

namespace EchoRelay.API
{
    public class ApiServer
    {
        public static ApiServer? Instance;

        public Server RelayServer { get; private set; }

        public delegate void ApiSettingsUpdated();
        public event ApiSettingsUpdated? OnApiSettingsUpdated;
        public ApiSettings ApiSettings { get; private set; }

        public ApiServer(Server relayServer, ApiSettings apiSettings)
        {
            Instance = this;

            RelayServer = relayServer;
            ApiSettings = apiSettings;

            var builder = WebApplication.CreateBuilder();
            builder.Services.AddCors(options =>
                options.AddPolicy("AllowAll", builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                )
            );
            builder.Services.AddControllers().AddApplicationPart(typeof(ApiServer).Assembly);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Host.UseSerilog();

            var app = builder.Build();
            app.UseCors("AllowAll");

            // Reduce logging noise
            app.UseSerilogRequestLogging();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiddleware<ApiAuthentication>();
            app.UseAuthorization();
            app.MapControllers();

            app.RunAsync("http://0.0.0.0:8080");
        }

        public void UpdateApiSettings(ApiSettings newSettings)
        {
            ApiSettings = newSettings;
            OnApiSettingsUpdated?.Invoke();
        }
    }
}
