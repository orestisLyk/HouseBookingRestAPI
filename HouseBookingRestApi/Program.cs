using HouseBookingRestApi.Data;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Security;
using Microsoft.EntityFrameworkCore;
using Serilog;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection;
using Microsoft.OpenApi.Models;
using HouseBookingRestApi.Helpers;
using HouseBookingRestApi.Configuration;
using AutoMapper;

namespace HouseBookingRestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Setup for Cloudinary
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;

            var builder = WebApplication.CreateBuilder(args);

            var connString = builder.Configuration.GetConnectionString("DefaultConnection");

            //JWT Configuration
            var jwtSettings = builder.Configuration.GetSection("Jwt");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = builder.Environment.IsDevelopment();
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
                };
            });

            //CORS Configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowClient",
                    b => b.WithOrigins(builder.Configuration["Cors:Origin"]!)
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            //JSON Serialization Configuration
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            //Swagger Configuration
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "House Booking API", Version = "v1" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "JWT Authorization header using the Bearer scheme."
                    });
                options.OperationFilter<AuthorizeOperationFilter>();

            });

            //Add authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("OwnerOnly", policy =>
                    policy.RequireRole("Owner"));
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
            });

            // Add services to the container.

            builder.Services.AddDbContext<HouseBookingRestApiContext>(options =>
                options.UseSqlServer(connString));

            builder.Services.AddSingleton<IEncryptionUtil, EncryptionUtil>();
            builder.Services.AddRepositories();

            builder.Services.AddSingleton(cloudinary);

            builder.Services.AddAutoMapper(cfg => { }, typeof(MapperConfig).Assembly);

            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            builder.Services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();
            builder.Services.AddScoped<IHouseImageService, HouseImageService>();
            builder.Services.AddScoped<IHouseService, HouseService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddSingleton(cloudinary);

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();


            var app = builder.Build();

            app.UseExceptionHandler();

            //Swagger UI
            if(app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseCors("AllowClient");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
