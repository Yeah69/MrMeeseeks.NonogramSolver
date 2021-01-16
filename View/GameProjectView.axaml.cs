using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MrMeeseeks.NonogramSolver.View
{
    public class GameProjectView : UserControl
    {
        public GameProjectView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
