using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MoviesAPI.Filters;
using MoviesAPI.Services;

namespace MoviesAPI
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
            services.AddDbContext<MoviesDbContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("MoviesAPIDb"));
            });

            services.AddDataProtection();

            services.AddCors(options=> {
                options.AddPolicy("AllowAPIRequestIO", builder =>
                {
                    builder.WithOrigins("http://apirequest.io").WithMethods("GET", "POST").AllowAnyHeader();
                });
            });

            services.AddAutoMapper(typeof(Startup));

            services.AddTransient<HashService>();

            //services.AddTransient<IFileStorageService, AzureStorageService>();
            services.AddTransient<IFileStorageService, InAppStorageService>();
            services.AddTransient<IHostedService, MoviesInTheatersService>();
            services.AddHttpContextAccessor();

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<MoviesDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(MyExceptionFilter));
            })
                .AddNewtonsoftJson()
                .AddXmlDataContractSerializerFormatters();

            services.AddSwaggerGen(config => {
                config.SwaggerDoc("v1", new OpenApiInfo { 
                    Version = "v1", 
                    Title = "MoviesAPI",
                    Description = "This is the Web API for Movies operatoins by Pankaj Rayal",
                    TermsOfService = new Uri("https://pankajrayal.wordpress.com"),
                    License = new OpenApiLicense() { 
                        Name = "MIT"
                    },
                    Contact = new OpenApiContact()
                    {
                        Name = "Pankaj Rayal",
                        Email = "pankajrayal@abc.com",
                        Url = new Uri("https://www.pankajrayal.wordpress.com")
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                config.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            app.UseSwagger();
            app.UseSwaggerUI(config=> {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "MoviesAPI");
            });
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseCors(builder=> {
            //    builder.WithOrigins("http://apirequest.io")
            //        .WithMethods("GET", "POST")
            //        .AllowAnyHeader();
            //});
            app.UseCors();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
