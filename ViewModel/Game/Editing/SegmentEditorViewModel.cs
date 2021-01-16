using MrMeeseeks.NonogramSolver.Model.Game.Editing;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Reactive.Disposables;

namespace MrMeeseeks.NonogramSolver.ViewModel.Game.Editing
{
    public interface ISegmentEditorViewModel : IViewModelLayerBase
    {
        int Length { get; set; }
        void RemoveFromParent();
    }

    internal class SegmentEditorViewModel : ViewModelLayerBase, ISegmentEditorViewModel
    {
        private readonly ISegmentEditor _model;
        private readonly ILineEditor _parent;

        public SegmentEditorViewModel(
            // parameters
            ISegmentEditor model,
            ILineEditor parent,
            
            // dependencies
            CompositeDisposable compositeDisposable)
        {
            _model = model;
            _parent = parent;
            model
                .ObservePropertyChanged(nameof(model.Length))
                .Subscribe(_ => OnPropertyChanged(nameof(Length)))
                .CompositeDisposalWith(compositeDisposable);
        }

        public int Length
        {
            get => _model.Length;
            set => _model.Length = value;
        }

        public void RemoveFromParent() => _parent.RemoveSegment(_model);
    }
}