using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;

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
        ILineCell? Previous { get; set; }
        ILineCell? Next { get; set; }
        CellState State { get; }
        ImmutableHashSet<ISegment> PossibleAssignments { get; }
        void Mark(ISegment segment);
        void Exclude();
        public void InitializePossibleAssignments(ImmutableHashSet<ISegment> set);
        public void ExcludePossibleAssignment(ISegment segment);
    }

    internal class Cell : ModelLayerBase, ICell
    {
        private ISegment? _horizontalAssignment;
        private CellState _state = CellState.Undecided;
        private ISegment? _verticalAssignment;

        public Cell((int, int) xAndY)
        {
            (X, Y) = xAndY;
            Vertical = new VerticalLineCell(this);
            Horizontal = new HorizontalLineCell(this);
        }

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

        public ILineCell? Up { private get; set; }

        public ILineCell? Down { private get; set; }

        public ILineCell? Left { private get; set; }

        public ILineCell? Right { private get; set; }

        public ILineCell Vertical { get; }
        public ILineCell Horizontal { get; }

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
        }

        private void MarkHorizontal(ISegment segment)
        {
            Mark();
            HorizontalAssignment = segment;
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

            protected LineCellBase(Cell parent)
            {
                Parent = parent;

                parent
                    .ObservePropertyChanged(nameof(parent.State))
                    .Where(_ => State == CellState.Marked && Assignment is null)
                    .Subscribe(_ =>
                    {
                        if (PossibleAssignments.Count == 1)
                            Mark(PossibleAssignments.First());
                        else if (Previous?.Assignment is { } previousAssignment)
                            Mark(previousAssignment);
                        else if (Next?.Assignment is { } nextAssignment)
                            Mark(nextAssignment);
                    });
            }

            public ImmutableHashSet<ISegment> PossibleAssignments
            {
                get => _possibleAssignments;
                private set
                {
                    SetIfChangedAndRaise(ref _possibleAssignments, value);
                    switch (PossibleAssignments.Count, State)
                    {
                        case (0, CellState.Undecided):
                            Exclude();
                            break;
                        case (1, CellState.Marked) when Assignment is null:
                            Mark(PossibleAssignments.First());
                            break;
                    }
                }
            }

            public abstract int Position { get; }
            public abstract ISegment? Assignment { get; protected set; }
            public abstract ILineCell? Previous { get; set; }
            public abstract ILineCell? Next { get; set; }

            public virtual void Mark(ISegment segment)
            {
                PossibleAssignments = ImmutableHashSet<ISegment>.Empty.Add(segment);
            }

            public CellState State => Parent.State;

            public void Exclude()
            {
                Parent.Exclude();
                PossibleAssignments = ImmutableHashSet<ISegment>.Empty;
            }

            public void InitializePossibleAssignments(ImmutableHashSet<ISegment> set)
            {
                PossibleAssignments = set;
                foreach (var segment in set)
                    segment.PossibleAssignCell(this);
            }

            public void ExcludePossibleAssignment(ISegment segment)
            {
                PossibleAssignments = PossibleAssignments.Except(segment.ToEnumerable());
            }

            protected void SubscribeToNeighborAssignment(ILineCell neighbor)
            {
                neighbor
                    .ObservePropertyChanged(nameof(neighbor.Assignment))
                    .Where(_ => neighbor.Assignment is not null)
                    .Subscribe(_ =>
                    {
                        if (State == CellState.Marked 
                         && Assignment is null 
                         && neighbor.Assignment is not null) 
                            Mark(neighbor.Assignment);
                    });
            }
        }

        private class VerticalLineCell : LineCellBase, IDisposable
        {
            private readonly IDisposable _subscription;

            public VerticalLineCell(Cell parent) : base(parent)
            {
                _subscription = EscalateNotifications(
                    parent,
                    (nameof(parent.Up), nameof(Previous)),
                    (nameof(parent.Down), nameof(Next)),
                    (nameof(parent.VerticalAssignment), nameof(Assignment)),
                    (nameof(parent.State), nameof(State)),
                    (nameof(parent.Y), nameof(Position)));
            }

            public override int Position => Parent.Y;

            public override ISegment? Assignment
            {
                get => Parent.VerticalAssignment;
                protected set => Parent.VerticalAssignment = value;
            }

            public override ILineCell? Previous
            {
                get => Parent.Up;
                set
                {
                    Parent.Up = value;
                    if (value is not null)
                        SubscribeToNeighborAssignment(value);
                }
            }

            public override ILineCell? Next
            {
                get => Parent.Down;
                set
                {
                    Parent.Down = value;
                    if (value is not null)
                        SubscribeToNeighborAssignment(value);
                }
            }

            public void Dispose()
            {
                _subscription.Dispose();
            }

            public override void Mark(ISegment segment)
            {
                Parent.MarkVertical(segment);
                base.Mark(segment);
            }
        }

        private class HorizontalLineCell : LineCellBase, IDisposable
        {
            private readonly IDisposable _subscription;

            public HorizontalLineCell(Cell parent) : base(parent)
            {
                _subscription = EscalateNotifications(
                    parent,
                    (nameof(parent.Left), nameof(Previous)),
                    (nameof(parent.Right), nameof(Next)),
                    (nameof(parent.HorizontalAssignment), nameof(Assignment)),
                    (nameof(parent.State), nameof(State)),
                    (nameof(parent.X), nameof(Position)));
            }

            public override int Position => Parent.X;

            public override ISegment? Assignment
            {
                get => Parent.HorizontalAssignment;
                protected set => Parent.HorizontalAssignment = value;
            }

            public override ILineCell? Previous
            {
                get => Parent.Left;
                set
                {
                    Parent.Left = value;
                    if (value is not null)
                        SubscribeToNeighborAssignment(value);
                }
            }

            public override ILineCell? Next
            {
                get => Parent.Right;
                set
                {
                    Parent.Right = value;
                    if (value is not null)
                        SubscribeToNeighborAssignment(value);
                }
            }

            public void Dispose()
            {
                _subscription.Dispose();
            }

            public override void Mark(ISegment segment)
            {
                Parent.MarkHorizontal(segment);
                base.Mark(segment);
            }
        }
    }
}