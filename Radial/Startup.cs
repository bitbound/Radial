using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Radial.Areas.Identity;
using Radial.Data;
using Radial.Data.Entities;
using Radial.Services;
using Radial.Services.Client;
using Radial.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlite(
                    Configuration.GetConnectionString("Sqlite")));

            services.AddDefaultIdentity<RadialUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<RadialUser>>();
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddScoped<IApplicationConfig, ApplicationConfig>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IEmailSenderEx, EmailSender>();
            services.AddSingleton<IWorld, World>();
            services.AddScoped<IJsInterop, JsInterop>();
            services.AddScoped<IClientConnection, ClientConnection>();
            services.AddScoped<IMessagePublisher, MessagePublisher>();
            services.AddScoped<IToastService, ToastService>();
            services.AddScoped<IModalService, ModalService>();

            services.AddScoped<IClientManager, ClientManager>();
            services.AddScoped<IInputDispatcher, InputDispatcher>();
            services.AddScoped<ILocationService, LocationService>();

            services.AddHostedService<GameEngine>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            ApplicationDbContext dbContext,
            ILoggerFactory loggerFactory,
            IWorld world)
        {
            app.UseForwardedHeaders();

            if (dbContext.Database.IsRelational())
            {
                dbContext.Database.Migrate();
            }


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            world.Load();

            loggerFactory.AddProvider(new DbLoggerProvider(env, app.ApplicationServices));
        }
    }
}
