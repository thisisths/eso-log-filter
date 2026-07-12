namespace EsoLogFilter.Infrastructure.File
{
    using EsoLogFilter.Core.Services;
    using EsoLogFilter.Infrastructure.File.Services;

    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureFile(this IServiceCollection @this)
        {
            return @this
                .AddScoped<IFileHandler, FileHandler>()
                .AddScoped<IFileSummarizer, FileSummarizer>();
        }
    }
}
