using MrMeeseeks.NonogramSolver.Model;
using System;
using System.Reactive.Disposables;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface ILineEditorViewModel : IViewModelLayerBase
    {
        ICollectionEditorViewModel<ISegmentEditorViewModel> Segments { get; }
        void RemoveFromParentAsColumn();
        void RemoveFromParentAsRow();
    }

    internal class LineEditorViewModel : ViewModelLayerBase, ILineEditorViewModel
    {
        private readonly ILineEditor _model;
        private readonly IGameEditor _parent;

        public LineEditorViewModel(
            // parameters
            ILineEditor model,
            IGameEditor parent,
            
            // dependencies
            Func<ISegmentEditor, ILineEditor, ISegmentEditorViewModel> segmentEditorViewModelFactory,
            CompositeDisposable compositeDisposable)
        {
            _model = model;
            _parent = parent;
            
            Segments = new CollectionEditorViewModel<ISegmentEditor, ISegmentEditorViewModel>(
                model.Segments,
                s => segmentEditorViewModelFactory(s, model),
                model.AddSegment,
                s => s.RemoveFromParent(),
                compositeDisposable);
        }

        public ICollectionEditorViewModel<ISegmentEditorViewModel> Segments { get; }

        public void RemoveFromParentAsColumn() => _parent.RemoveColumn(_model);
        
        public void RemoveFromParentAsRow() => _parent.RemoveRow(_model);
    }
}