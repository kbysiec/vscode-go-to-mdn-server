using Coravel;
using Coravel.Pro;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApp.Core;
using WebApp.Scheduler;
using WebApp.Utils;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages().AddNewtonsoftJson();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanAccessPolicy", policy =>
                    policy.RequireClaim("CanAccess", "true"));
            });

            services.AddCoravelPro(typeof(ApplicationDbContext));
            services.AddPermission<DashboardAccessValidator>();
            services.AddQueue();

            RegisterDependencies(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
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
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseCoravelPro();
        }

        private void RegisterDependencies(IServiceCollection services)
        {
            services.AddScoped<Logger>();
            services.AddScoped<ResourceDownloader>();
            services.AddScoped<Parser>();
            services.AddScoped<UrlNormalizer>();
            services.AddScoped<ApplicationDbContext>();
            services.AddScoped<ResourceDownloaderJob>();
            services.AddScoped<DataRepository>();
            services.AddScoped<DataService>();
            services.AddSingleton<JobProcessor>();
        }
    }
}
