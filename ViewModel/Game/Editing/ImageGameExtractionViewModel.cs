using MrMeeseeks.NonogramSolver.Model;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.ViewModel.Game.Editing
{
    public interface IFromImageGameEditorViewModel : IGameEditorViewModelBase
    {
        string ImagePath { get; }
        Task Browse();
    }

    internal class FromImageGameEditorViewModel : ViewModelLayerBase, IFromImageGameEditorViewModel
    {
        private readonly IFromImageGameEditor _model;
        private readonly IMainWindowOpenFileDialog _mainWindowOpenFileDialog;
        private readonly TaskCompletionSource<IGame> _resultSource;
        private string _name = "Untitled";
        private string _imagePath = "";

        public FromImageGameEditorViewModel(
            IFromImageGameEditor model,
            IMainWindowOpenFileDialog mainWindowOpenFileDialog)
        {
            _model = model;
            _mainWindowOpenFileDialog = mainWindowOpenFileDialog;
            _resultSource = new TaskCompletionSource<IGame>();
        }

        public string Name
        {
            get => _name;
            set => this.SetIfChangedAndRaise(ref _name, value);
        }

        public string ImagePath
        {
            get => _imagePath; 
            private set => this.SetIfChangedAndRaise(ref _imagePath, value);
        }

        public async Task Browse()
        {
            var directory = Environment.CurrentDirectory;
            var paths = await _mainWindowOpenFileDialog.Show(
                "Load game database",
                directory,
                "",
                false,
                ("Image", new List<string> {"png"}));
            if (paths?.Length == 1 && paths.FirstOrDefault() is { } path)
            {
                ImagePath = path;
                await _model.OCR(path)
                    .ConfigureAwait(false);
            }
        }

        public Task<IGame> Result => _resultSource.Task;
        public void Okay()
        {
        }

        public void Cancel() => _resultSource.SetCanceled();
    }
}