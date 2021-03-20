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
    }

    internal class Segment : ModelLayerBase, ISegment
    {
        private readonly ObservableCollection<ILineCell> _assignedCells = new();
        private ILineCell? _currentMinCell;
        private ILineCell? _currentMaxCell;

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
                OnPropertyChanged(nameof(Cleared));
        }
    }
}