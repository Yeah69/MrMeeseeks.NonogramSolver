using System;

namespace MrMeeseeks.NonogramSolver.Model
{
    public interface ISegmentEditor : IModelLayerBase
    {
        int Length { get; set; }
        ISegment Build();
    }

    internal class SegmentEditor : ModelLayerBase, ISegmentEditor
    {
        private readonly Func<int, ISegment> _segmentFactory;
        private int _length;

        public SegmentEditor(
            // parameter
            int length,
            
            // dependencies
            Func<int, ISegment> segmentFactory)
        {
            Length = length;
            _segmentFactory = segmentFactory;
        }

        public int Length
        {
            get => _length;
            set => SetIfChangedAndRaise(ref _length, value);
        }

        public ISegment Build() => _segmentFactory(Length);
    }
}