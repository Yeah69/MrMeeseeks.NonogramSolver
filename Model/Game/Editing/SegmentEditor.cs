using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System;

namespace MrMeeseeks.NonogramSolver.Model.Game.Editing
{
    public interface ISegmentEditor : IModelLayerBase
    {
        int Length { get; set; }
        ISegment Build();
    }

    internal class SegmentEditor : ModelLayerBase, ISegmentEditor
    {
        private readonly ICellIterator _cellIterator;
        private readonly Func<int, ICellIterator, ISegment> _segmentFactory;
        private int _length;

        public SegmentEditor(
            // parameter
            int length,
            ICellIterator cellIterator,
            
            // dependencies
            Func<int, ICellIterator, ISegment> segmentFactory)
        {
            Length = length;
            _cellIterator = cellIterator;
            _segmentFactory = segmentFactory;
        }

        public int Length
        {
            get => _length;
            set => SetIfChangedAndRaise(ref _length, value);
        }

        public ISegment Build() => _segmentFactory(Length, _cellIterator);
    }
}