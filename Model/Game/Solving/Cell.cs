using System;

namespace MrMeeseeks.NonogramSolver.Model.Game.Solving
{
    public enum CellState { Undecided, Excluded, Marked }
    
    public interface ICell : IModelLayerBase
    {
        ILineCell Vertical { get; }
        ILineCell Horizontal { get; }
        CellState State { get; }
    }
    
    public interface ILineCell : IModelLayerBase
    {
        int Position { get; }
        ISegment? Assignment { get; }
        ILineCell? Previous { get; }
        ILineCell? Next { get; }
        CellState State { get; }
        void Mark(ISegment segment);
        void Exclude();
    }

    internal class Cell : ModelLayerBase, ICell
    {
        private CellState _state = CellState.Undecided;
        private ISegment? _verticalAssignment;
        private ISegment? _horizontalAssignment;

        public Cell((int, int) xAndY)
        {
            (X, Y) = xAndY;
            Vertical = new VerticalLineCell(this);
            Horizontal = new HorizontalLineCell(this);
        }

        public ILineCell Vertical { get; }
        public ILineCell Horizontal { get; }
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

        public Cell? Up { get; set; }
        
        public Cell? Down { get; set; }
        
        public Cell? Left { get; set; }
        
        public Cell? Right { get; set; }

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
            segment.AssignCell(this.Vertical);

            if (HorizontalAssignment is null)
            {
                if (Left is {HorizontalAssignment: {} horizontalAssignmentDown})
                {
                    HorizontalAssignment = horizontalAssignmentDown;
                    horizontalAssignmentDown.AssignCell(this.Horizontal);
                }
                else if (Right is {HorizontalAssignment: {} horizontalAssignmentUp})
                {
                    HorizontalAssignment = horizontalAssignmentUp;
                    horizontalAssignmentUp.AssignCell(this.Horizontal);
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
            segment.AssignCell(this.Horizontal);

            if (VerticalAssignment is null)
            {
                if (Down is {VerticalAssignment: {} verticalAssignmentDown})
                {
                    VerticalAssignment = verticalAssignmentDown;
                    verticalAssignmentDown.AssignCell(this.Vertical);
                }
                else if (Up is {VerticalAssignment: {} verticalAssignmentUp})
                {
                    VerticalAssignment = verticalAssignmentUp;
                    verticalAssignmentUp.AssignCell(this.Vertical);
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
        
        private abstract class LineCellBase : ModelLayerBase, ILineCell
        {
            protected readonly Cell Parent;

            public LineCellBase(Cell parent)
            {
                Parent = parent;
            }

            public abstract int Position { get; }
            public abstract ISegment? Assignment { get; }
            public abstract ILineCell? Previous { get; }
            public abstract ILineCell? Next { get; }
            
            public abstract void Mark(ISegment segment);

            public CellState State => Parent.State;

            public void Exclude() => Parent.Exclude();
        }
        
        private class VerticalLineCell : LineCellBase
        {

            public VerticalLineCell(Cell parent) : base(parent)
            {
            }

            public override int Position => Parent.Y;
            public override ISegment? Assignment => Parent.VerticalAssignment;
            public override ILineCell? Previous => Parent.Up?.Vertical;
            public override ILineCell? Next => Parent.Down?.Vertical;
            public override void Mark(ISegment segment) => Parent.MarkVertical(segment);
        }
        
        private class HorizontalLineCell : LineCellBase
        {

            public HorizontalLineCell(Cell parent) : base(parent)
            {
            }

            public override int Position => Parent.X;
            public override ISegment? Assignment => Parent.HorizontalAssignment;
            public override ILineCell? Previous => Parent.Left?.Horizontal;
            public override ILineCell? Next => Parent.Right?.Horizontal;
            public override void Mark(ISegment segment) => Parent.MarkHorizontal(segment);
        }
    }
}