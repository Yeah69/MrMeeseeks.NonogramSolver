using MrMeeseeks.NonogramSolver.Model.Settings;
using MrMeeseeks.NonogramSolver.ViewModel.Game.Editing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface IMainWindowViewModel : IViewModelLayerBase
    {
        string Title { get; }
        IGameProjectViewModelBase Content { get; }
        Task FileMenuNew();
        Task FileMenuOpen();
    }

    internal class MainWindowViewModel : ViewModelLayerBase, IMainWindowViewModel
    {
        private IGameProjectViewModelBase _content;
        private readonly ICurrentSettings _currentSettings;
        private readonly IMainWindowSaveFileDialog _mainWindowSaveFileDialog;
        private readonly IMainWindowOpenFileDialog _mainWindowOpenFileDialog;
        private readonly Func<string, IGameProjectViewModel> _gameProjectViewModelFactory;
        private readonly Func<IFromImageGameEditorViewModel> _imageGameExtractionViewModelFactory;

        public MainWindowViewModel(
            IEmptyGameProjectViewModel emptyGameProjectViewModel,
            ICurrentSettings currentSettings,
            IMainWindowSaveFileDialog mainWindowSaveFileDialog,
            IMainWindowOpenFileDialog mainWindowOpenFileDialog,
            Func<string, IGameProjectViewModel> gameProjectViewModelFactory,
            Func<IFromImageGameEditorViewModel> imageGameExtractionViewModelFactory)
        {
            _content = emptyGameProjectViewModel;
            _currentSettings = currentSettings;
            _mainWindowSaveFileDialog = mainWindowSaveFileDialog;
            _mainWindowOpenFileDialog = mainWindowOpenFileDialog;
            _gameProjectViewModelFactory = gameProjectViewModelFactory;
            _imageGameExtractionViewModelFactory = imageGameExtractionViewModelFactory;

            if(File.Exists(currentSettings.Value.LastOpenFileName))
                Content = _gameProjectViewModelFactory(currentSettings.Value.LastOpenFileName ?? "");
        }

        public string Title =>
            $"Mr. Meeseeks - Nonogram Solver{(this.Content is IGameProjectViewModel {Name: { } name} ? $" - {name}" : "")}";

        public IGameProjectViewModelBase Content
        {
            get => _content;
            private set
            {
                SetIfChangedAndRaise(ref _content, value);
                OnPropertyChanged(nameof(Title));
            }
        }

        public async Task FileMenuNew()
        {
            var initialFileName = "game.db";
            var directory = Environment.CurrentDirectory;
            if (_currentSettings.Value.LastOpenFileName is { } fileName)
            {
                FileInfo fileInfo = new(fileName);
                if (fileInfo.Exists)
                {
                    initialFileName = fileInfo.Name;
                    directory = fileInfo.DirectoryName ?? "";
                }
            }
            var path = await _mainWindowSaveFileDialog.Show(
                "Create new game database",
                directory,
                initialFileName,
                "db",
                ("LiteDb", new List<string> {"db"}));
            if (path is null) return;
            Content = _gameProjectViewModelFactory(path);
            _currentSettings.Value.LastOpenFileName = path;
        }

        public async Task FileMenuOpen()
        {
            var initialFileName = "game.db";
            var directory = Environment.CurrentDirectory;
            if (_currentSettings.Value.LastOpenFileName is { } fileName)
            {
                FileInfo fileInfo = new(fileName);
                if (fileInfo.Exists)
                {
                    initialFileName = fileInfo.Name;
                    directory = fileInfo.DirectoryName ?? "";
                }
            }
            var paths = await _mainWindowOpenFileDialog.Show(
                "Load game database",
                directory,
                initialFileName,
                false,
                ("LiteDb", new List<string> {"db"}));
            if (paths?.Length == 1)
            {
                var chosenFileName = paths.First();
                Content = _gameProjectViewModelFactory(chosenFileName);
                _currentSettings.Value.LastOpenFileName = chosenFileName;
            }
        }
    }
}