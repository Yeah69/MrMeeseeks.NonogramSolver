using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MrMeeseeks.NonogramSolver.View
{
    public class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
