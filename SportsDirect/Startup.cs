﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsDirect.Models;

namespace SportsDirect
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAdB2C(options => Configuration.Bind("AzureAdB2C", options))
            .AddCookie();

            services.AddMvc();

            //Adding cross-origin resource sharing middleware
            services.AddCors();

            //Add CosmosDB configuration from appsettings.json
            CosmosDBClient.CosmosDBURI = Configuration.GetValue<string>("CosmosDB:URI");
            CosmosDBClient.CosmosDBKey = Configuration.GetValue<string>("CosmosDB:Key");
            CosmosDBClient.CosmosDBDatabaseName = Configuration.GetValue<string>("CosmosDB:DatabaseName");
            //CosmosDB initialiser
            CosmosDBClient<Orders>.Initialize();
            CosmosDBClient<Products>.Initialize();
            CosmosDBClient<Users>.Initialize();

            //Add Service Bus config
            ServiceBusClientCredentials.ConnectionString = Configuration.GetValue<string>("ServiceBus:ConnectionString");
            ServiceBusClientCredentials.Key = Configuration.GetValue<string>("ServiceBus:Key");
            ServiceBusClientCredentials.QueueName = Configuration.GetValue<string>("ServiceBus:QueueName");
            //Service Bus initialiser
            ServiceBusClient.Initialize();

            services.AddOptions();
            //Add other config
            services.Configure<OtherSettings>(Configuration.GetSection("OtherSettings"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseCors(builder =>
                builder.WithOrigins("https://login.microsoftonline.com"));

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
        }
    }
}
        
    

