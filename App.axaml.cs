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
            }
            base.OnFrameworkInitializationCompleted();
            CryptographicManager.InitializeManagers();
            KeyManager.InitializeManagers();
        }
    }
}