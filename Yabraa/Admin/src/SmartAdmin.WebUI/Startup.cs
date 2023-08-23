using System;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
//using SmartAdmin.WebUI.Data;
using SmartAdmin.WebUI.Models;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF;
using YabraaEF.Models;

namespace SmartAdmin.WebUI
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
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddSession();
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;
            });
            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("ar")                 
                };

                options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.Configure<SmartSettings>(Configuration.GetSection(SmartSettings.SectionName));

            // Note: This line is for demonstration purposes only, I would not recommend using this as a shorthand approach for accessing settings
            // While having to type '.Value' everywhere is driving me nuts (>_<), using this method means reloaded appSettings.json from disk will not work
            services.AddSingleton(s => s.GetRequiredService<IOptions<SmartSettings>>().Value);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders(); 

       
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<SmartAdmin.WebUI.Services.Service, SmartAdmin.WebUI.Services.Service>();
            services.AddTransient<SmartAdmin.WebUI.Services.CategoryService, SmartAdmin.WebUI.Services.CategoryService>();
            services.AddTransient<SmartAdmin.WebUI.Services.PackageService, SmartAdmin.WebUI.Services.PackageService>();
            services.AddTransient<SmartAdmin.WebUI.Services.StartPagesService, SmartAdmin.WebUI.Services.StartPagesService>();
            services.AddTransient<SmartAdmin.WebUI.Services.UserService, SmartAdmin.WebUI.Services.UserService>();
            services.AddTransient<SmartAdmin.WebUI.Services.VisitsService, SmartAdmin.WebUI.Services.VisitsService>();
            services
                .AddControllersWithViews();

            services.AddRazorPages();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });
            
            //services.ConfigureApplicationCookie(options =>
            //{
            //    options.LoginPath = "/Account/login";
            //    options.LogoutPath = "/Account/logout";
            //    options.AccessDeniedPath = "/Account/accessDenied";
            //    //  options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
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

            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=AspNetCore}/{action=Welcome}");
                endpoints.MapRazorPages();
            });
        }
    }
}
