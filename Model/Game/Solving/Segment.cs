using MoreLinq;
using MrMeeseeks.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.Model.Game.Solving
{
    public interface ISegment : IModelLayerBase
    {
        int Length { get; }

        ILineCell? CurrentMinCell { get; }

        ILineCell? CurrentMaxCell { get; }

        ISegment? Previous { set; }

        ISegment? Next { set; }

        ImmutableHashSet<ILineCell> AssignedCells { get; }
        ImmutableHashSet<ILineCell> CurrentPossibleCells { get; }

        bool Cleared => Length == AssignedCells.Count;
        
        void InitializePossibleAssignments(ImmutableHashSet<ILineCell> set);

        bool Check();
    }

    internal class Segment : ModelLayerBase, ISegment
    {
        private ImmutableHashSet<ILineCell> _assignedCells = ImmutableHashSet<ILineCell>.Empty;
        private ILineCell? _currentMaxCell;
        private ILineCell? _currentMinCell;
        private ImmutableHashSet<ILineCell> _currentPossibleCells = ImmutableHashSet<ILineCell>.Empty;

        public Segment(int length)
        {
            Length = length > 0
                ? length
                : throw new ArgumentException("Segment length has to be greater than zero.");
        }

        private ILineCell? CurrentPossibleMaxCell { get; set; }

        private ILineCell? CurrentPossibleMinCell { get; set; }

        private bool Cleared => Length == AssignedCells.Count;

        public ImmutableHashSet<ILineCell> CurrentPossibleCells
        {
            get => _currentPossibleCells;
            private set
            {
                _currentPossibleCells = value;
                CurrentPossibleMaxCell = _currentPossibleCells.MaxBy(c => c.Position).FirstOrDefault();
                CurrentPossibleMinCell = _currentPossibleCells.MinBy(c => c.Position).FirstOrDefault();
            }
        }

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

                // as soon as cleared exclude neighbors
                if (Cleared) OnPropertyChanged(nameof(Cleared));
            }
        }

        public void InitializePossibleAssignments(ImmutableHashSet<ILineCell> set) => CurrentPossibleCells = set;

        public bool Check()
        {
            var ret = false;

            // Adjust CurrentPossibleCells
            
            var minPossiblePosition = Previous?.CurrentMaxCell is {} prevMaxCell 
                ? prevMaxCell.Position + 2 
                : 0;
            var maxPossiblePosition = Next?.CurrentMinCell is {} nextMinCell 
                ? nextMinCell.Position - 2 
                : int.MaxValue;
            var newExcludesOfPossibleCells = CurrentPossibleCells.Where(c =>
                (c.State == CellState.Excluded || c.Assignment is not null && c.Assignment != this)
                || c.Position < minPossiblePosition 
                || c.Position > maxPossiblePosition)
                .ToList();

            if (newExcludesOfPossibleCells.Any())
            {
                ret = true;
                CurrentPossibleCells = CurrentPossibleCells.Except(newExcludesOfPossibleCells);
            }

            // Adjust AssignedCells

            var unknownAssignedCells = CurrentPossibleCells
                .Except(AssignedCells)
                .Where(c => c.Assignment == this)
                .ToList();
            
            if (unknownAssignedCells.Any())
            {
                ret = true;
                var newAssignedCells = ImmutableHashSet.Create(AssignedCells.Concat(unknownAssignedCells).ToArray());
                var minPosition = newAssignedCells.MinBy(c => c.Position).First().Position;
                var maxPosition = newAssignedCells.MaxBy(c => c.Position).First().Position;
                newAssignedCells = CurrentPossibleCells
                    .Where(c => newAssignedCells.Contains(c).Not() && c.Position >= minPosition &&
                                c.Position <= maxPosition)
                    .Aggregate(newAssignedCells, (cells, cell) => cells.Add(cell));
                AssignedCells = newAssignedCells;
            }

            // If possible exclude too small possible cell groups

            var excludeCellGroups = CurrentPossibleCells
                .OrderBy(c => c.Position)
                .Aggregate(new List<List<ILineCell>>(), (lol, c) =>
                {
                    if(lol.None() || lol.Last().Last().Position + 1 < c.Position)
                        lol.Add(new List<ILineCell> { c });
                    else
                        lol.Last().Add(c);
                    return lol;
                })
                .Where(g => g.Count < Length)
                .SelectMany(g => g)
                .ToList();
            if (excludeCellGroups.Any())
            {
                ret = true;
                CurrentPossibleCells = CurrentPossibleCells.Except(excludeCellGroups);
            }

            if (CurrentPossibleMinCell is not null 
             && CurrentPossibleMaxCell is not null
             && CurrentPossibleMaxCell.Position - CurrentPossibleMinCell.Position + 1 < 2 * Length)
            {
                var assignableLength =
                    2 * Length - (CurrentPossibleMaxCell.Position - CurrentPossibleMinCell.Position + 1);
                var margin = Length - assignableLength;
                var min = CurrentPossibleMinCell.Position + margin;
                var max = CurrentPossibleMinCell.Position + margin + assignableLength - 1;
                var toAssign = CurrentPossibleCells
                    .Where(c => c.Position >= min && c.Position <= max)
                    .Except(AssignedCells)
                    .ToArray();
                if (toAssign.Any())
                {
                    ret = true;
                    AssignedCells = 
                        toAssign.Aggregate(AssignedCells, (current, cell) => current.Add(cell));
                }
            }

            if (CurrentMinCell is not null
             && CurrentMaxCell is not null)
            {
                var upperExclude = CurrentMinCell.Position + Length;
                var lowerExclude = CurrentMaxCell.Position - Length;
                var excludes = CurrentPossibleCells
                    .Where(c => c.Position <= lowerExclude || c.Position >= upperExclude)
                    .ToList();
                if (excludes.Any())
                {
                    ret = true;
                    CurrentPossibleCells = CurrentPossibleCells.Except(excludes);
                }
            }
            
            return ret;
        }
    }
}