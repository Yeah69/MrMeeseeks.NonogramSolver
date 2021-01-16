using DynamicData;
using DynamicData.Binding;
using MrMeeseeks.Extensions;
using MrMeeseeks.NonogramSolver.Model.Game;
using MrMeeseeks.NonogramSolver.Model.Game.Editing;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using MrMeeseeks.NonogramSolver.ViewModel.Game.Editing;
using MrMeeseeks.NonogramSolver.ViewModel.Game.Solving;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface IGameProjectViewModel : IGameProjectViewModelBase
    {
        string Name { get; }
        ReadOnlyObservableCollection<IGameViewModel> Games { get; }
        IGameViewModel? SelectedGame { get; set; }
        IGameEditorViewModel? Editor { get; }
        void CreateNewGame();
        void CopyAndEdit(IGameViewModel game);
    }

    internal class GameProjectViewModel : ViewModelLayerBase, IGameProjectViewModel
    {
        private readonly IGameProject _gameProject;
        private readonly Func<IGameEditor> _gameEditorFactory;
        private readonly Func<IGameEditor, IGameEditorViewModel> _gameEditorViewModelFactory;
        private IGameViewModel? _selectedGame;
        private IGameEditorViewModel? _editor;

        public GameProjectViewModel(
            // parameters
            IGameProject gameProject,
            
            // dependencies
            Func<IGame, IGameViewModel> gameViewModelFactory,
            Func<IGameEditor> gameEditorFactory,
            Func<IGameEditor, IGameEditorViewModel> gameEditorViewModelFactory,
            CompositeDisposable compositeDisposable)
        {
            _gameProject = gameProject;
            _gameEditorFactory = gameEditorFactory;
            _gameEditorViewModelFactory = gameEditorViewModelFactory;

            _gameProject
                .Games
                .ToObservableChangeSet()
                .Transform(gameViewModelFactory)
                .Bind(out var games)
                .Subscribe()
                .CompositeDisposalWith(compositeDisposable);
            Games = games;

            SelectedGame = Games.FirstOrDefault();
        }

        public string Name => _gameProject.Name;
        
        public ReadOnlyObservableCollection<IGameViewModel> Games { get; }

        public IGameViewModel? SelectedGame
        {
            get => _selectedGame;
            set => SetIfChangedAndRaise(ref _selectedGame, value);
        }

        public bool IsGamesVisible => IsEditorVisible.Not();

        public IGameEditorViewModel? Editor
        {
            get => _editor;
            private set
            {
                SetIfChangedAndRaise(ref _editor, value);
                OnPropertyChanged(nameof(IsEditorVisible), nameof(IsGamesVisible));
            }
        }

        public bool IsEditorVisible => Editor is not null;

        public void CreateNewGame()
        {
#pragma warning disable 4014 *** Fire and forget is intended
            TriggerEditing(_gameEditorViewModelFactory(_gameEditorFactory()));
#pragma warning restore 4014
        }

        public void CopyAndEdit(IGameViewModel game)
        {
#pragma warning disable 4014 *** Fire and forget is intended
            TriggerEditing(game.CreateEditableCopy());
#pragma warning restore 4014
        }

        public void Solve() => SelectedGame?.Solve();

        public void Build() => Editor?.Okay();

        private async Task TriggerEditing(IGameEditorViewModel editorViewModel)
        {
            Editor = editorViewModel;
            try
            {
                _gameProject.Add(await editorViewModel.Result);
            }
            catch (OperationCanceledException)
            {
                // Nothing to do
            }

            Editor = null;
        } 
    }
}