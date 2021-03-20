using MrMeeseeks.Extensions;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.Model.Game.Editing
{
    public interface ILineEditor : IModelLayerBase
    {
        ReadOnlyObservableCollection<ISegmentEditor> Segments { get; }
        void AddSegment();
        void RemoveSegment(ISegmentEditor segmentEditor);
        ILine Build();
    }

    internal class LineEditor : ModelLayerBase, ILineEditor
    {
        private readonly Func<int, ISegmentEditor> _segmentEditorFactory;
        private readonly Func<IReadOnlyList<ISegment>, ILine> _lineFactory;
        private readonly ObservableCollection<ISegmentEditor> _segments = new();
        
        public LineEditor(
            // dependencies
            Func<int, ISegmentEditor> segmentEditorFactory,
            Func<IReadOnlyList<ISegment>, ILine> lineFactory)
        {
            _segmentEditorFactory = segmentEditorFactory;
            _lineFactory = lineFactory;
            Segments = new ReadOnlyObservableCollection<ISegmentEditor>(_segments);
        }
        
        public ReadOnlyObservableCollection<ISegmentEditor> Segments { get; }

        public void AddSegment() => _segments.Add(_segmentEditorFactory(1));

        public void RemoveSegment(ISegmentEditor segmentEditor) => _segments.Remove(segmentEditor);

        public ILine Build() => 
            _lineFactory(
                Segments.Select(s => s.Build()).ToReadOnlyList());
    }
}