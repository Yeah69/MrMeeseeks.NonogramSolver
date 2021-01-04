using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MrMeeseeks.NonogramSolver.ViewModel;

namespace MrMeeseeks.NonogramSolver.View
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        
        public IMainWindowViewModel ViewModel { init => DataContext = value; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
