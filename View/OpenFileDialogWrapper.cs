using Avalonia.Controls;
using MrMeeseeks.NonogramSolver.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.View
{
    internal class MainWindowOpenFileDialogWrapper : OpenFileDialogWrapper, IMainWindowOpenFileDialog
    {
        public MainWindowOpenFileDialogWrapper(Lazy<MainWindow> lazyMainWindow) : base(new Lazy<Window>(() => lazyMainWindow.Value)) {}
    }

    internal abstract class OpenFileDialogWrapper : IOpenFileDialog
    {
        private readonly Lazy<Window> _lazyWindow;

        protected OpenFileDialogWrapper(Lazy<Window> lazyWindow)
        {
            _lazyWindow = lazyWindow;
        }

        public Task<string[]?> Show(
            string title, 
            string directory, 
            string initialFileName, 
            bool allowMultiple,
            params (string Name, IReadOnlyList<string> Extensions)[] filters) =>
            new OpenFileDialog
            {
                Title = title,
                Directory = directory,
                InitialFileName = initialFileName,
                AllowMultiple = allowMultiple,
                Filters = filters
                    .Select(t => 
                        new FileDialogFilter
                        {
                            Name = t.Name, 
                            Extensions = t.Extensions.ToList()
                        })
                    .ToList()
            }.ShowAsync(_lazyWindow.Value);
    }
}