using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MrMeeseeks.NonogramSolver.DI;

namespace MrMeeseeks.NonogramSolver.Root
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = AutofacModule.Start();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
