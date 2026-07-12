namespace EsoLogFilter.Core
{
    using EsoLogFilter.Core.Services;

    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection @this)
        {
            return @this
                .AddScoped<IFilterByUnitTypeService, FilterByUnitTypeService>()
                .AddScoped<ILogSummaryService, LogSummaryService>();
        }
    }
}
