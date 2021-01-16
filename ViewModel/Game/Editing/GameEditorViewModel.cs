using MrMeeseeks.NonogramSolver.Model.Game.Editing;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.ViewModel.Game.Editing
{
    public interface IGameEditorViewModel : IViewModelLayerBase
    {
        string Name { get; set; }
        ICollectionEditorViewModel<ILineEditorViewModel> Columns { get; }
        ICollectionEditorViewModel<ILineEditorViewModel> Rows { get; }
        Task<IGame> Result { get; }
        void Okay();
        void Cancel();
    }

    public class GameEditorViewModel : ViewModelLayerBase, IGameEditorViewModel
    {
        private readonly IGameEditor _model;
        private readonly TaskCompletionSource<IGame> _resultSource;

        public GameEditorViewModel(
            // parameters
            IGameEditor model,
            
            // dependencies
            Func<ILineEditor, IGameEditor, ILineEditorViewModel> lineEditorViewModelFactory,
            CompositeDisposable compositeDisposable)
        {
            _model = model;
            
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

            _resultSource = new TaskCompletionSource<IGame>();
            Result = _resultSource.Task;

            this.EscalateNotifications(model, (nameof(model.Name), nameof(Name)))
                .CompositeDisposalWith(compositeDisposable);
        }

        public string Name
        {
            get => _model.Name;
            set => _model.Name = value;
        }
        public ICollectionEditorViewModel<ILineEditorViewModel> Columns { get; }

        public ICollectionEditorViewModel<ILineEditorViewModel> Rows { get; }
        
        public Task<IGame> Result { get; }
        public void Okay() => _resultSource.SetResult(_model.Build());

        public void Cancel() => _resultSource.SetCanceled();
    }
}