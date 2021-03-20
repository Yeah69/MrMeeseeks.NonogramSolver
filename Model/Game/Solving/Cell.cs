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
            if (State == CellState.Marked) return;
            if (State == CellState.Excluded) throw new Exception();
            State = CellState.Marked;
        }

        public void MarkVertical(ISegment segment)
        {
            Mark();
            VerticalAssignment = segment;
            segment.AssignCell(this);

            if (HorizontalAssignment is null)
            {
                if (Left is {HorizontalAssignment: {} horizontalAssignmentDown})
                {
                    HorizontalAssignment = horizontalAssignmentDown;
                    horizontalAssignmentDown.AssignCell(this);
                }
                else if (Right is {HorizontalAssignment: {} horizontalAssignmentUp})
                {
                    HorizontalAssignment = horizontalAssignmentUp;
                    horizontalAssignmentUp.AssignCell(this);
                }
            }

            if (Up is {State: CellState.Marked, VerticalAssignment: null})
            {
                Up.MarkVertical(segment);
            }

            if (Down is {State: CellState.Marked, VerticalAssignment: null})
            {
                Down.MarkVertical(segment);
            }
        }

        public void MarkHorizontal(ISegment segment)
        {
            Mark();
            HorizontalAssignment = segment;
            segment.AssignCell(this);

            if (VerticalAssignment is null)
            {
                if (Down is {VerticalAssignment: {} verticalAssignmentDown})
                {
                    VerticalAssignment = verticalAssignmentDown;
                    verticalAssignmentDown.AssignCell(this);
                }
                else if (Up is {VerticalAssignment: {} verticalAssignmentUp})
                {
                    VerticalAssignment = verticalAssignmentUp;
                    verticalAssignmentUp.AssignCell(this);
                }
            }

            if (Left is {State: CellState.Marked, HorizontalAssignment: null})
            {
                Left.MarkHorizontal(segment);
            }

            if (Right is {State: CellState.Marked, HorizontalAssignment: null})
            {
                Right.MarkHorizontal(segment);
            }
        }

        public void Exclude()
        {
            if (State == CellState.Excluded) return;
            State = State != CellState.Undecided
                ? throw new Exception()
                : CellState.Excluded;
        }
    }
}