using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MrMeeseeks.NonogramSolver.View
{
    public class FromImageGameEditorView : UserControl
    {
        public FromImageGameEditorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
