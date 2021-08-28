using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandleMyQueue.Models;
using HandleMyQueue.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;

namespace HandleMyQueue
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4000")
                            .WithHeaders("Origin", "Accept", "Content-Type", "ResponseType",
                                "Content-Length", "X-Access-Token", "X-Refresh-Token")
                        .AllowCredentials();
                    });
            });
            services.Configure<RouteOptions>(opt => opt.LowercaseUrls = true);
            services.Configure<QueuesDatabaseSettings>(
                Configuration.GetSection(nameof(QueuesDatabaseSettings)));

            services.AddSingleton<IQueuesDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<QueuesDatabaseSettings>>().Value);
            // services.AddSingleton<QueuesService>();
            services.AddSingleton<QueuesService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HandleMyQueue", Version = "v1" });
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.Authority = Configuration["Jwt:Authority"];
                o.Audience = Configuration["Jwt:Audience"];
                o.RequireHttpsMetadata = false;
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.StatusCode = 500;
                        c.Response.ContentType = "text/plain";
                        // if (env.IsDevelopment())
                        // {
                            return c.Response.WriteAsync(c.Exception.ToString());
                        // }
                        // return c.Response.WriteAsync("An error occured processing your authentication.");
                    },
                    OnMessageReceived = async context =>
                    {
                        if (context.Request.Cookies.ContainsKey("X-Access-Token"))
                        {
                            context.Token = context.Request.Cookies["X-Access-Token"];
                        }
                        else if (context.Request.Cookies.ContainsKey("X-Refresh-Token"))
                        {
                            var refreshToken = context.Request.Cookies["X-Refresh-Token"];
                            AuthenticationService authenticationService = new AuthenticationService();
                            //Todo: Handle Unauthorized exception.

                            var newAccessToken =
                                await authenticationService.RefreshToken(refreshToken, context.Request,
                                    context.Response);
                            context.Token = newAccessToken.Value;

                        }

                        await Task.CompletedTask;
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HandleMyQueue v1"));
            }

            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}