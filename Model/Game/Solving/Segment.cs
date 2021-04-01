using MoreLinq;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MrMeeseeks.NonogramSolver.Model.Game.Solving
{
    public interface ISegment : IModelLayerBase
    {
        int Length { get; }

        ILineCell? CurrentMinCell { get; }

        ILineCell? CurrentMaxCell { get; }

        ISegment? Previous { get; set; }

        ISegment? Next { get; set; }

        ImmutableHashSet<ILineCell> AssignedCells { get; }
        ImmutableHashSet<ILineCell> CurrentPossibleCells { get; }

        bool Cleared => Length == AssignedCells.Count;

        IObservable<ILineCell> ObserveExcludedCells { get; }

        void PossibleAssignCell(ILineCell cell);

        void InitializeTrivialAssignments();
    }

    internal class Segment : ModelLayerBase, ISegment
    {
        private readonly Subject<ILineCell> _observeExcludedCells = new();
        private ImmutableHashSet<ILineCell> _assignedCells = ImmutableHashSet<ILineCell>.Empty;
        private ILineCell? _currentMaxCell;
        private ILineCell? _currentMinCell;
        private ImmutableHashSet<ILineCell> _currentPossibleCells = ImmutableHashSet<ILineCell>.Empty;
        private bool _isInitialized = false;

        public Segment(int length)
        {
            Length = length > 0
                ? length
                : throw new ArgumentException("Segment length has to be greater than zero.");
        }

        private ILineCell? CurrentPossibleMaxCell { get; set; }

        private ILineCell? CurrentPossibleMinCell { get; set; }

        private bool Cleared => Length == AssignedCells.Count;

        private int CountLeftToAssign => Length - AssignedCells.Count;

        public ImmutableHashSet<ILineCell> CurrentPossibleCells
        {
            get => _currentPossibleCells;
            private set
            {
                ImmutableHashSet<ILineCell> temp = _currentPossibleCells.Except(value);
                _currentPossibleCells = value;
                foreach (var lineCell in temp) _observeExcludedCells.OnNext(lineCell);
                CurrentPossibleMaxCell = _currentPossibleCells.MaxBy(c => c.Position).FirstOrDefault();
                CurrentPossibleMinCell = _currentPossibleCells.MinBy(c => c.Position).FirstOrDefault();
                
                AdjustToPossibleAssignments();
            }
        }

        public IObservable<ILineCell> ObserveExcludedCells => _observeExcludedCells.AsObservable();

        public int Length { get; }

        public ILineCell? CurrentMinCell
        {
            get => _currentMinCell;
            private set => SetIfChangedAndRaise(ref _currentMinCell, value);
        }

        public ILineCell? CurrentMaxCell
        {
            get => _currentMaxCell;
            private set => SetIfChangedAndRaise(ref _currentMaxCell, value);
        }

        public ISegment? Previous { get; set; }
        public ISegment? Next { get; set; }

        public ImmutableHashSet<ILineCell> AssignedCells
        {
            get => _assignedCells;
            private set
            {
                _assignedCells = value;
                CurrentMaxCell = _assignedCells.MaxBy(c => c.Position).FirstOrDefault();
                CurrentMinCell = _assignedCells.MinBy(c => c.Position).FirstOrDefault();

                if (CurrentMinCell is not null && CurrentMaxCell is not null)
                {
                    // Mark all cells between min and max
                    ILineCell? iCell = CurrentMinCell;
                    while (iCell != CurrentMaxCell && iCell.Next is { } nextCell)
                    {
                        iCell = nextCell;
                        if (iCell.Assignment == this)
                            continue;
                        if ((iCell.Assignment is not null && iCell.Assignment != this) || iCell.State == CellState.Excluded)
                            throw new Exception();
                        iCell.Mark(this);
                    }
                    
                    // exclude all past possible borders
                    var minPossiblePosition = CurrentMinCell.Position - CountLeftToAssign;
                    var maxPossiblePosition = CurrentMaxCell.Position + CountLeftToAssign;
                    foreach (var lineCell in CurrentPossibleCells.Where(c => c.Position < minPossiblePosition || c.Position > maxPossiblePosition))
                    {
                        ExcludeCell(lineCell);
                    }
                }

                // as soon as cleared exclude neighbors
                if (Cleared)
                {
                    OnPropertyChanged(nameof(Cleared));
                    if (CurrentMinCell is {Previous: { } prevToMinCell})
                        prevToMinCell.Exclude();
                    if (CurrentMaxCell is {Next: { } nextToMaxCell})
                        nextToMaxCell.Exclude();
                }
                AdjustToPossibleAssignments();
            }
        }

        public void PossibleAssignCell(ILineCell cell) => CurrentPossibleCells = CurrentPossibleCells.Add(cell);

        public void InitializeTrivialAssignments()
        {
            _isInitialized = true;
            
            CurrentPossibleCells
                .Select(c => c
                    .ObservePropertyChanged(nameof(c.Assignment))
                    .Select(_ => c))
                .Merge()
                .Where(c => c.Assignment == this && AssignedCells.Contains(c).Not())
                .Subscribe(AssignCell);

            CurrentPossibleCells
                .Select(c => c
                    .ObservePropertyChanged(nameof(c.Assignment), nameof(c.State))
                    .Select(_ => c))
                .Merge()
                .Where(c => ((c.Assignment is not null && c.Assignment != this) || c.State == CellState.Excluded)
                         && CurrentPossibleCells.Contains(c))
                .Subscribe(ExcludeCell);

            Previous
                ?.ObservePropertyChanged(nameof(Previous.CurrentMaxCell))
                .Subscribe(_ =>
                {
                    if (Previous?.CurrentMaxCell?.Next is { } minExclude) TrimPossibleMin(minExclude);
                });

            Next
                ?.ObservePropertyChanged(nameof(Next.CurrentMinCell))
                .Subscribe(_ =>
                {
                    if (Next?.CurrentMinCell?.Previous is { } maxExclude) TrimPossibleMax(maxExclude);
                });

            foreach (var currentPossibleCell in CurrentPossibleCells)
                if (currentPossibleCell.Assignment == this)
                    AssignCell(currentPossibleCell);
                else if ((currentPossibleCell.Assignment is { } someSegment && someSegment != this) ||
                         currentPossibleCell.State == CellState.Excluded)
                    ExcludeCell(currentPossibleCell);

            if (CurrentPossibleMaxCell is null || CurrentPossibleMinCell is null) return;
            int span = CurrentPossibleMaxCell.Position - CurrentPossibleMinCell.Position + 1;
            int margin = span - Length;
            if (margin >= Length) return;
            int trivialSpan = Length - margin;
            ILineCell? iCell = CurrentPossibleMinCell;
            for (var i = 0; i < margin; i++)
                iCell = iCell?.Next;
            for (var i = 0; i < trivialSpan; i++)
            {
                iCell?.Mark(this);
                iCell = iCell?.Next;
            }
        }

        private void TrimPossibleMax(ILineCell cell)
        {
            CurrentPossibleCells =
                CurrentPossibleCells.Except(CurrentPossibleCells.Where(c => c.Position >= cell.Position));
            
        }

        private void TrimPossibleMin(ILineCell cell)
        {
            CurrentPossibleCells =
                CurrentPossibleCells.Except(CurrentPossibleCells.Where(c => c.Position <= cell.Position));
        }

        private void AssignCell(ILineCell cell)
        {
            if (AssignedCells.Contains(cell)) return;
            AssignedCells = AssignedCells.Add(cell);
        }

        private void AdjustToPossibleAssignments()
        {
            if (_isInitialized.Not()) return;
            
            if (CurrentMaxCell is null && CurrentMinCell is null)
            {
                var consecutivePossibleOpenGroups = CurrentPossibleCells
                    .Except(AssignedCells)
                    .OrderBy(c => c.Position)
                    .Aggregate(new List<List<ILineCell>>(), (listOfLists, cell) =>
                    {
                        if (listOfLists.Any() && listOfLists.Last().Last().Position + 1 == cell.Position)
                        {
                            listOfLists.Last().Add(cell);
                        }
                        else
                        {
                            listOfLists.Add(new List<ILineCell> {cell});
                        }

                        return listOfLists;
                    });
                foreach (var cell in consecutivePossibleOpenGroups
                    .Where(consecutivePossibleOpenGroup => consecutivePossibleOpenGroup.Count < Length)
                    .SelectMany(l => l))
                {
                    ExcludeCell(cell);
                }
                
                InitializeTrivialAssignments();
            }
            
            if (CurrentMaxCell is null || CurrentMinCell is null) return;

            int assignedSpan = CurrentMaxCell.Position - CurrentMinCell.Position + 1;
            int leftToAssign = Length - assignedSpan;
            if (CurrentPossibleMinCell?.Position < CurrentMinCell.Position - leftToAssign)
            {
                ILineCell? mCell = CurrentMinCell;
                for (var i = 0; i < leftToAssign; i++)
                    mCell = mCell?.Previous;
                CurrentPossibleMinCell = mCell;
            }

            if (CurrentPossibleMaxCell?.Position > CurrentMaxCell.Position + leftToAssign)
            {
                ILineCell? mCell = CurrentMaxCell;
                for (var i = 0; i < leftToAssign; i++)
                    mCell = mCell?.Next;
                CurrentPossibleMaxCell = mCell;
            }

            if (CurrentPossibleMinCell?.Position > CurrentMinCell.Position - leftToAssign)
            {
                int assignableSpan = CurrentPossibleMinCell?.Position - (CurrentMinCell.Position - leftToAssign) ?? 0;
                ILineCell? mCell = CurrentMaxCell;
                for (var i = 0; i < assignableSpan; i++) mCell = mCell?.Next;

                mCell?.Mark(this);
            }

            assignedSpan = CurrentMaxCell.Position - CurrentMinCell.Position + 1;
            leftToAssign = Length - assignedSpan;
            if (CurrentPossibleMaxCell?.Position < CurrentMaxCell.Position + leftToAssign)
            {
                int? assignableSpan = CurrentMaxCell?.Position + leftToAssign - CurrentPossibleMaxCell?.Position;
                ILineCell? mCell = CurrentMinCell;
                for (var i = 0; i < assignableSpan; i++) mCell = mCell?.Previous;

                mCell?.Mark(this);
            }
        }

        private void ExcludeCell(ILineCell cell)
        {
            if (CurrentMinCell is { } && CurrentMaxCell is { })
            {
                if (cell.Position >= CurrentMinCell.Position && cell.Position <= CurrentMaxCell.Position)
                    throw new Exception();
                if (cell.Position < CurrentMinCell.Position)
                    TrimPossibleMin(cell);
                else
                    TrimPossibleMax(cell);
            }
            else
                CurrentPossibleCells = CurrentPossibleCells.Except(cell.ToEnumerable());
        }
    }
}