
using System.Net;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace DatingApp.API
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
      services.AddDbContext<DataContext>( x => x.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
      services.AddMvc();
      services.AddControllers().AddNewtonsoftJson(
        opt =>
        {
          opt.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        });
      services.AddCors();
      services.AddAutoMapper(cfg => { }, typeof(DatingRepository));
      services.AddTransient<Seed>();
      // scoped means services is created once per request , singleton for each request
      services.AddScoped<IUserRepository, UserRepository>();
      services.AddScoped<IDatingRepository, DatingRepository>();
      services.AddScoped<IMatchRepository, MatchRepository>();
      services.AddScoped<IPasswordHasher, PasswordHasher>();
      services.AddScoped<IAuthService, AuthService>();
      services.AddScoped<IUserService, UserService>();
      services.AddScoped<IMatchesService, MatchesService>();

      // add auth service
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
        options =>
        {
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
              new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
          };
        });

      services.AddSwaggerGen(options =>
      {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "DatingApp API", Version = "v1" });

        var bearerScheme = new OpenApiSecurityScheme
        {
          Name = "Authorization",
          Type = SecuritySchemeType.Http,
          Scheme = "bearer",
          BearerFormat = "JWT",
          In = ParameterLocation.Header,
          Description = "Paste the JWT returned from /api/auth/login (without the \"Bearer \" prefix).",
          Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        };
        options.AddSecurityDefinition("Bearer", bearerScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
          { bearerScheme, new string[] { } }
        });
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler(builder =>
        {
          builder.Run(async context =>
          {
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            var error = context.Features.Get <IExceptionHandlerFeature>() ;
            if (error != null)
            {
              context.Response.AddApplicationError(error.Error.Message);
              await context.Response.WriteAsync(error.Error.Message);
            }
          });
        });
      }

      // app.UseHttpsRedirection();

      app.UseSwagger();
      app.UseSwaggerUI(options =>
      {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DatingApp API v1");
      });

      app.UseRouting();

      var clientUrl = Configuration.GetValue<string>("ClientUrl");
      app.UseCors(x =>
      {
        if (!string.IsNullOrEmpty(clientUrl))
        {
          x.WithOrigins(clientUrl).AllowAnyMethod().AllowAnyHeader();
        }
        else
        {
          x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
      });
      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();

      });
    }
  }
}
