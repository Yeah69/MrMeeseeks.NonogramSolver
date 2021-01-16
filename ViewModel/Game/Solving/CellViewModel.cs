using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Reactive.Disposables;

namespace MrMeeseeks.NonogramSolver.ViewModel.Game.Solving
{
    public interface ICellViewModel : IViewModelLayerBase
    {
        bool IsMarked { get; }
        bool IsExcluded { get; }
    }

    internal class CellViewModel : ViewModelLayerBase, ICellViewModel
    {
        private readonly ICell _model;

        public CellViewModel(
            // parameters
            ICell model,
            
            // dependencies
            CompositeDisposable compositeDisposable)
        {
            _model = model;

            model
                .ObservePropertyChanged(nameof(model.State))
                .Subscribe(_ => OnPropertyChanged(nameof(IsMarked), nameof(IsExcluded)))
                .CompositeDisposalWith(compositeDisposable);
        }

        public bool IsMarked => _model.State == CellState.Marked;
        public bool IsExcluded => _model.State == CellState.Excluded;
    }
}