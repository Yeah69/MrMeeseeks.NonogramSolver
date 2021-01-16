using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MrMeeseeks.NonogramSolver.View
{
    public class GameEditorView : UserControl
    {
        public GameEditorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
