using MrMeeseeks.Extensions;
using System;
using System.Collections.Immutable;
using System.Linq;

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
        ImmutableHashSet<ISegment> PossibleAssignments { get; }
        void Mark(ISegment segment);
        void Exclude();
        public void InitializePossibleAssignments(ImmutableHashSet<ISegment> set);
        public void ExcludePossibleAssignment(ISegment segment);
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

        public Cell? Up { private get; set; }
        
        public Cell? Down { private get; set; }
        
        public Cell? Left { private get; set; }
        
        public Cell? Right { private get; set; }

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

        private void MarkVertical(ISegment segment)
        {
            Mark();
            VerticalAssignment = segment;
            segment.AssignCell(this.Vertical);

            if (HorizontalAssignment is null)
            {
                if (Horizontal.PossibleAssignments.Count == 1)
                    Horizontal.Mark(Horizontal.PossibleAssignments.First());
                else if (Left is {HorizontalAssignment: {} horizontalAssignmentDown})
                    Horizontal.Mark(horizontalAssignmentDown);
                else if (Right is {HorizontalAssignment: {} horizontalAssignmentUp})
                    Horizontal.Mark(horizontalAssignmentUp);
            }

            if (Up is {State: CellState.Marked, VerticalAssignment: null})
                Up.Vertical.Mark(segment);

            if (Down is {State: CellState.Marked, VerticalAssignment: null})
                Down.Vertical.Mark(segment);
        }

        private void MarkHorizontal(ISegment segment)
        {
            Mark();
            HorizontalAssignment = segment;
            segment.AssignCell(this.Horizontal);

            if (VerticalAssignment is null)
            {
                if (Vertical.PossibleAssignments.Count == 1)
                    Vertical.Mark(Vertical.PossibleAssignments.First());
                else if (Down is {VerticalAssignment: {} verticalAssignmentDown})
                    Vertical.Mark(verticalAssignmentDown);
                else if (Up is {VerticalAssignment: {} verticalAssignmentUp})
                    Vertical.Mark(verticalAssignmentUp);
            }

            if (Left is {State: CellState.Marked, HorizontalAssignment: null})
                Left.Horizontal.Mark(segment);

            if (Right is {State: CellState.Marked, HorizontalAssignment: null})
                Right.Horizontal.Mark(segment);
        }

        private void Exclude()
        {
            if (State == CellState.Excluded) return;
            State = State != CellState.Undecided
                ? throw new Exception()
                : CellState.Excluded;
        }
        
        private abstract class LineCellBase : ModelLayerBase, ILineCell
        {
            protected readonly Cell Parent;
            private ImmutableHashSet<ISegment> _possibleAssignments = ImmutableHashSet<ISegment>.Empty;

            public ImmutableHashSet<ISegment> PossibleAssignments
            {
                get => _possibleAssignments;
                private set
                {
                    SetIfChangedAndRaise(ref _possibleAssignments, value);
                    if (PossibleAssignments.Count == 0
                        && State == CellState.Undecided) 
                        Exclude();
                    if (PossibleAssignments.Count == 1 
                        && Assignment is null 
                        && State == CellState.Marked) 
                        Mark(PossibleAssignments.First());
                }
            }

            protected LineCellBase(Cell parent) => Parent = parent;

            public abstract int Position { get; }
            public abstract ISegment? Assignment { get; }
            public abstract ILineCell? Previous { get; }
            public abstract ILineCell? Next { get; }
            
            public virtual void Mark(ISegment segment) => 
                PossibleAssignments = ImmutableHashSet<ISegment>.Empty.Add(segment);

            public CellState State => Parent.State;

            public void Exclude()
            {
                Parent.Exclude();
                PossibleAssignments = ImmutableHashSet<ISegment>.Empty;
            }

            public void InitializePossibleAssignments(ImmutableHashSet<ISegment> set)
            {
                _possibleAssignments = set;
                foreach (var segment in set)
                    segment.PossibleAssignCell(this);
            }

            public void ExcludePossibleAssignment(ISegment segment)
            {
                PossibleAssignments = PossibleAssignments.Except(segment.ToEnumerable());
            }
        }
        
        private class VerticalLineCell : LineCellBase, IDisposable
        {
            private readonly IDisposable _subscription;

            public VerticalLineCell(Cell parent) : base(parent) =>
                _subscription = this.EscalateNotifications(
                    parent,
                    (nameof(parent.Up), nameof(Previous)),
                    (nameof(parent.Down), nameof(Next)),
                    (nameof(parent.VerticalAssignment), nameof(Assignment)),
                    (nameof(parent.State), nameof(State)),
                    (nameof(parent.Y), nameof(Position)));

            public override int Position => Parent.Y;
            public override ISegment? Assignment => Parent.VerticalAssignment;
            public override ILineCell? Previous => Parent.Up?.Vertical;
            public override ILineCell? Next => Parent.Down?.Vertical;
            public override void Mark(ISegment segment)
            {
                Parent.MarkVertical(segment);
                base.Mark(segment);
            }

            public void Dispose() => _subscription.Dispose();
        }
        
        private class HorizontalLineCell : LineCellBase, IDisposable
        {
            private readonly IDisposable _subscription;

            public HorizontalLineCell(Cell parent) : base(parent) =>
                _subscription = this.EscalateNotifications(
                    parent,
                    (nameof(parent.Left), nameof(Previous)),
                    (nameof(parent.Right), nameof(Next)),
                    (nameof(parent.HorizontalAssignment), nameof(Assignment)),
                    (nameof(parent.State), nameof(State)),
                    (nameof(parent.X), nameof(Position)));

            public override int Position => Parent.X;
            public override ISegment? Assignment => Parent.HorizontalAssignment;
            public override ILineCell? Previous => Parent.Left?.Horizontal;
            public override ILineCell? Next => Parent.Right?.Horizontal;
            public override void Mark(ISegment segment)
            {
                Parent.MarkHorizontal(segment);
                base.Mark(segment);
            }

            public void Dispose() => _subscription.Dispose();
        }
    }
}