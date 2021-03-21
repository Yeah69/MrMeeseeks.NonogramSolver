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
        ICellToolTipViewModel ToolTip { get; }
    }

    internal class CellViewModel : ViewModelLayerBase, ICellViewModel
    {
        private readonly ICell _model;
        private readonly (ILine Column, ILine Row) _lines;
        private readonly Func<ICell, ICellToolTipViewModel> _cellToolTipViewModelFactory;

        public CellViewModel(
            // parameters
            ICell model,
            (ILine Column, ILine Row) lines,

            // dependencies
            Func<ICell, ICellToolTipViewModel> cellToolTipViewModelFactory,
            CompositeDisposable compositeDisposable)
        {
            _model = model;
            _lines = lines;
            _cellToolTipViewModelFactory = cellToolTipViewModelFactory;

            model
                .ObservePropertyChanged(nameof(model.State))
                .Subscribe(_ => OnPropertyChanged(nameof(IsMarked), nameof(IsExcluded)))
                .CompositeDisposalWith(compositeDisposable);
        }

        public bool IsMarked => _model.State == CellState.Marked;
        public bool IsExcluded => _model.State == CellState.Excluded;
        public ICellToolTipViewModel ToolTip => _cellToolTipViewModelFactory(_model);
    }
}