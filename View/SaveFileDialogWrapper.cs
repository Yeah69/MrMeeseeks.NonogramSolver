using Avalonia.Controls;
using MrMeeseeks.NonogramSolver.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.View
{
    internal class MainWindowSaveFileDialogWrapper : SaveFileDialogWrapper, IMainWindowSaveFileDialog
    {
        public MainWindowSaveFileDialogWrapper(Lazy<MainWindow> lazyMainWindow) : base(new Lazy<Window>(() => lazyMainWindow.Value)) {}
    }

    internal abstract class SaveFileDialogWrapper : ISaveFileDialog
    {
        private readonly Lazy<Window> _lazyWindow;

        protected SaveFileDialogWrapper(Lazy<Window> lazyWindow)
        {
            _lazyWindow = lazyWindow;
        }

        public Task<string> Show(
            string title, 
            string directory, 
            string initialFileName, 
            string defaultExtension,
            params (string Name, IReadOnlyList<string> Extensions)[] filters) =>
            new SaveFileDialog
            {
                Title = title,
                Directory = directory,
                InitialFileName = initialFileName,
                DefaultExtension = defaultExtension,
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