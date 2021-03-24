using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MrMeeseeks.NonogramSolver.View
{
    public class TextualGameEditorView : UserControl
    {
        public TextualGameEditorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
