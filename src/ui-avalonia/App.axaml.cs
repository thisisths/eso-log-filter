namespace EsoLogFilter.Ui.Avalonia
{
    using global::Avalonia;
    using global::Avalonia.Controls.ApplicationLifetimes;
    using global::Avalonia.Markup.Xaml;
    using EsoLogFilter.Core;
    using EsoLogFilter.Infrastructure.File;

    using Microsoft.Extensions.DependencyInjection;

    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();
            services.AddCore()
                .AddInfrastructureFile()
                .AddLogging();
            services.AddSingleton<MainWindow>();
            serviceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = serviceProvider.GetRequiredService<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}