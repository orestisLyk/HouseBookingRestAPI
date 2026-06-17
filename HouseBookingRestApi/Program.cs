
using HouseBookingRestApi.Data;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Security;
using Microsoft.EntityFrameworkCore;
using Serilog;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;

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

            // Add services to the container.
            builder.Services.AddDbContext<HouseBookingRestApiContext>(options =>
                options.UseSqlServer(connString));

            builder.Services.AddSingleton<IEncryptionUtil, EncryptionUtil>();
            builder.Services.AddRepositories();

            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
