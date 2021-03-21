using System;
using System.Collections.ObjectModel;

namespace MrMeeseeks.NonogramSolver.Model.Game.Solving
{
    public interface ISegment : IModelLayerBase
    {
        int Length { get; }
        
        ILineCell? CurrentMinCell { get; }
        
        ILineCell? CurrentMaxCell { get; }
        
        ReadOnlyObservableCollection<ILineCell> AssignedCells { get; }

        bool Cleared => Length == AssignedCells.Count;

        void AssignCell(ILineCell cell);

        void PossibleAssignCell(ILineCell cell);

        void InitializeTrivialAssignments();
    }

    internal class Segment : ModelLayerBase, ISegment
    {
        private readonly ObservableCollection<ILineCell> _assignedCells = new();
        private ILineCell? _currentMinCell;
        private ILineCell? _currentMaxCell;
        private ILineCell? _currentPossibleMinCell;
        private ILineCell? _currentPossibleMaxCell;

        public Segment(int length)
        {
            Length = length > 0
                ? length
                : throw new ArgumentException("Segment length has to be greater than zero.");

            AssignedCells = new ReadOnlyObservableCollection<ILineCell>(_assignedCells);
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
        
        bool Cleared => Length == AssignedCells.Count;
        
        public ReadOnlyObservableCollection<ILineCell> AssignedCells { get; }
        
        public void AssignCell(ILineCell cell)
        {
            if (_assignedCells.Contains(cell)) return;
            _assignedCells.Add(cell);
            
            // Update min and max cell
            if (CurrentMinCell is { } minCell && minCell.Position > cell.Position
                || CurrentMinCell is null)
                CurrentMinCell = cell;
            if (CurrentMaxCell is { } maxCell && maxCell.Position < cell.Position
                || CurrentMaxCell is null)
                CurrentMaxCell = cell;

            var assignedSpan = CurrentMaxCell.Position - CurrentMinCell.Position + 1;
            var leftToAssign = Length - assignedSpan;
            if (_currentPossibleMinCell?.Position < (CurrentMinCell.Position - leftToAssign))
            {
                ILineCell? mCell = CurrentMinCell;
                for (int i = 0; i < leftToAssign; i++)
                    mCell = mCell?.Previous;
                _currentPossibleMinCell = mCell;
            }
            if (_currentPossibleMaxCell?.Position > (CurrentMaxCell.Position + leftToAssign))
            {
                ILineCell? mCell = CurrentMaxCell;
                for (int i = 0; i < leftToAssign; i++)
                    mCell = mCell?.Next;
                _currentPossibleMaxCell = mCell;
            }
            
            ///*
            if (_currentPossibleMinCell?.Position > (CurrentMinCell.Position - leftToAssign))
            {
                var assignableSpan = _currentPossibleMinCell?.Position - (CurrentMinCell.Position - leftToAssign);
                ILineCell? mCell = CurrentMaxCell;
                for (int i = 0; i < assignableSpan; i++)
                {
                    mCell = mCell?.Next;
                }
                mCell?.Mark(this);
            }
            assignedSpan = CurrentMaxCell.Position - CurrentMinCell.Position + 1;
            leftToAssign = Length - assignedSpan;
            if (_currentPossibleMaxCell?.Position < (CurrentMaxCell.Position + leftToAssign))
            {
                var assignableSpan = (CurrentMaxCell.Position + leftToAssign) - _currentPossibleMaxCell?.Position;
                ILineCell? mCell = CurrentMinCell;
                for (int i = 0; i < assignableSpan; i++)
                {
                    mCell = mCell?.Previous;
                }
                mCell?.Mark(this);
            }//*/
            

            var iCell = CurrentMinCell;
            while (iCell != CurrentMaxCell && iCell.Next is {} nextCell)
            {
                iCell = nextCell;
                if (iCell.Assignment == this)
                    continue;
                if (iCell.Assignment is not null && iCell.Assignment != this || iCell.State == CellState.Excluded)
                    throw new Exception();
                iCell.Mark(this);
            }

            if (Cleared)
            {
                OnPropertyChanged(nameof(Cleared));
                if (CurrentMinCell is {Previous: {} prevToMinCell})
                    prevToMinCell.Exclude();
                if (CurrentMaxCell is {Next: {} nextToMaxCell})
                    nextToMaxCell.Exclude();
            }
        }

        public void PossibleAssignCell(ILineCell cell)
        {
            if (_currentPossibleMinCell is null || _currentPossibleMinCell.Position > cell.Position)
                _currentPossibleMinCell = cell;
            
            if (_currentPossibleMaxCell is null || _currentPossibleMaxCell.Position < cell.Position)
                _currentPossibleMaxCell = cell;
        }

        public void InitializeTrivialAssignments()
        {
            if (_currentPossibleMaxCell is null || _currentPossibleMinCell is null) return;
            var span = _currentPossibleMaxCell.Position - _currentPossibleMinCell.Position + 1;
            var margin = span - Length;
            if (margin >= Length) return;
            var trivialSpan = Length - margin;
            ILineCell? iCell = _currentPossibleMinCell;
            for (int i = 0; i < margin; i++)
                iCell = iCell?.Next;
            for (int i = 0; i < trivialSpan; i++)
            {
                iCell?.Mark(this);
                iCell = iCell?.Next;
            }
        }
    }
}