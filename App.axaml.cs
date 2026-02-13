using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JuliaCrypt.CryptographicManagers;
using JuliaCrypt.ViewModels;

namespace JuliaCrypt
{
    public partial class App : Application
    {
        public static MainWindowViewModel MWvmInstance { get; private set; } = new();
        public static MainWindow MWInstance { get; private set; } = new();
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MWInstance = new MainWindow()
                {
                    DataContext = MWvmInstance,
                };
                desktop.MainWindow = MWInstance;
                desktop.Startup += OnStartup;
                desktop.Exit += OnExit;
            }
            base.OnFrameworkInitializationCompleted();
        }

        private void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs args)
        {
            CryptographicManager.InitializeManagers();
            KeyManager.InitializeManagers();
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs args)
        {
            foreach(var manager in CryptographicManager.CryptographicManagers)
            {
                manager.Dispose();
            }

            foreach(var manager in KeyManager.KeyManagers)
            {
                manager.Dispose();
            }
        }
    }
}