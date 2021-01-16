using System;

namespace MrMeeseeks.NonogramSolver.Model.Game.Solving
{
    public enum CellState { Undecided, Excluded, Marked }
    
    public interface ICell : IModelLayerBase
    {
        int X { get; }
        int Y { get; }
        ISegment? VerticalAssignment { get; }
        ISegment? HorizontalAssignment { get; }
        ICell? Up { get; }
        ICell? Down { get; }
        ICell? Left { get; }
        ICell? Right { get; }
        CellState State { get; }
        void MarkVertical(ISegment segment);
        void MarkHorizontal(ISegment segment);
        void Exclude();
    }

    internal class Cell : ModelLayerBase, ICell
    {
        private CellState _state = CellState.Undecided;
        private ISegment? _verticalAssignment;
        private ISegment? _horizontalAssignment;

        public Cell((int, int) xAndY) => (X, Y) = xAndY;

        public int X { get; }
        public int Y { get; }

        public ISegment? VerticalAssignment
        {
            get => _verticalAssignment;
            private set => SetIfChangedAndRaise(ref _verticalAssignment, value);
        }

        public ISegment? HorizontalAssignment
        {
            get => _horizontalAssignment;
            private set => SetIfChangedAndRaise(ref _horizontalAssignment, value);
        }

        public ICell? Up { get; set; }
        
        public ICell? Down { get; set; }
        
        public ICell? Left { get; set; }
        
        public ICell? Right { get; set; }

        public CellState State
        {
            get => _state;
            private set => SetIfChangedAndRaise(ref _state, value);
        }

        private void Mark()
        {
            if (State == CellState.Excluded) throw new Exception();
            State = CellState.Marked;
        }

        public void MarkVertical(ISegment segment)
        {
            Mark();
            VerticalAssignment = segment;
        }

        public void MarkHorizontal(ISegment segment)
        {
            Mark();
            HorizontalAssignment = segment;
        }

        public void Exclude() => State = State != CellState.Undecided 
            ? throw new Exception() 
            : CellState.Excluded;
    }
}