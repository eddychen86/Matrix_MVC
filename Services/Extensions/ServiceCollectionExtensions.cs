using Matrix.Models;
using Matrix.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Matrix.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailSender, EmailSender>();
            return services;
        }
    }
}
