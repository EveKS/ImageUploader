using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using ImageUploader.Datasets;
using ImageUploader.Models;
using ImageUploader.Services.Classes;
using ImageUploader.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace ImageUploader
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            this.Configuration = configuration;

            this.Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration["dbConnect:DefaultConnection"];

            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connectionString));

            #region Identity
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User = new UserOptions
                {
                    RequireUniqueEmail = true
                };

                options.Password = new PasswordOptions
                {
                    RequireDigit = true,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false,
                    RequireLowercase = true,
                    RequiredLength = 5,
                };
            }).AddEntityFrameworkStores<ApplicationContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(cfg =>
                {
                    cfg.SaveToken = true;

                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = this.Configuration["jwt:ValidIssuer"],

                        ValidateAudience = true,
                        ValidAudience = this.Configuration["jwt:ValidAudience"],

                        ValidateLifetime = true,

                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["jwt:IssuerSigningKey"]))
                    };
                });
            #endregion

            #region Compression
            services.Configure<GzipCompressionProviderOptions>(options =>
                options.Level = CompressionLevel.Optimal);

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;

                options.MimeTypes = new[]
                      {
                    // General
                    "text/plain",
                    // Static files
                    "text/css",
                    "application/javascript",
                    // MVC
                    "text/html",
                    "application/xml",
                    "text/xml",
                    "application/json",
                    "text/json",
                    // Custom
                    "image/svg+xml"
          };

                options.Providers.Add<GzipCompressionProvider>();
            });
            #endregion

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot/dist";
            });

            services.AddSingleton<IFileService, FileService>(option => new FileService(this.Environment));

            services.AddSingleton<IConfiguration>(this.Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseResponseCompression();

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            Directory.CreateDirectory(Path.Combine(env.WebRootPath, "files"));

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = content =>
                {
                    var time = 7 * 24 * 60 * 60;

                    content.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={time}";
                    content.Context.Response.Headers[HeaderNames.Expires] = DateTime.UtcNow.AddDays(7).ToString("R"); // Format RFC1123
                },

                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.WebRootPath, "files")),
                RequestPath = "/files"
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (this.Environment.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
