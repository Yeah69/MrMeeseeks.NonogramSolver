using MrMeeseeks.NonogramSolver.Model;
using System;
using System.Reactive.Disposables;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface IGameEditorViewModel : IViewModelLayerBase
    {
        ICollectionEditorViewModel<ILineEditorViewModel> Columns { get; }
        ICollectionEditorViewModel<ILineEditorViewModel> Rows { get; }
        IGameViewModel BuildGame();
    }

    public class GameEditorViewModel : ViewModelLayerBase, IGameEditorViewModel
    {
        private readonly IGameEditor _model;
        private readonly Func<IGame, IGameViewModel> _gameViewModelFactory;

        public GameEditorViewModel(
            // parameters
            IGameEditor model,
            
            // dependencies
            Func<ILineEditor, IGameEditor, ILineEditorViewModel> lineEditorViewModelFactory,
            Func<IGame, IGameViewModel> gameViewModelFactory,
            CompositeDisposable compositeDisposable)
        {
            _model = model;
            _gameViewModelFactory = gameViewModelFactory;
            
            Columns = new CollectionEditorViewModel<ILineEditor, ILineEditorViewModel>(
                model.Columns,
                c => lineEditorViewModelFactory(c, model),
                model.AddColumn,
                c => c.RemoveFromParentAsColumn(),
                compositeDisposable);
            
            Rows = new CollectionEditorViewModel<ILineEditor, ILineEditorViewModel>(
                model.Rows,
                c => lineEditorViewModelFactory(c, model),
                model.AddRow,
                c => c.RemoveFromParentAsRow(),
                compositeDisposable);
        }

        public ICollectionEditorViewModel<ILineEditorViewModel> Columns { get; }

        public ICollectionEditorViewModel<ILineEditorViewModel> Rows { get; }
        
        public IGameViewModel BuildGame() => _gameViewModelFactory(_model.Build());
    }
}