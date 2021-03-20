using System;
using System.Collections.ObjectModel;

namespace MrMeeseeks.NonogramSolver.Model.Game.Solving
{
    public interface ISegment : IModelLayerBase
    {
        int Length { get; }
        
        ICell? CurrentMinCell { get; }
        
        ICell? CurrentMaxCell { get; }
        
        ReadOnlyObservableCollection<ICell> AssignedCells { get; }

        bool Cleared => Length == AssignedCells.Count;

        void AssignCell(ICell cell);
    }

    internal class Segment : ModelLayerBase, ISegment
    {
        private readonly ICellIterator _cellIterator;
        private readonly ObservableCollection<ICell> _assignedCells = new();
        private ICell? _currentMinCell;
        private ICell? _currentMaxCell;

        public Segment(
            int length,
            ICellIterator cellIterator)
        {
            _cellIterator = cellIterator;
            Length = length > 0
                ? length
                : throw new ArgumentException("Segment length has to be greater than zero.");

            AssignedCells = new ReadOnlyObservableCollection<ICell>(_assignedCells);
        }

        public int Length { get; }

        public ICell? CurrentMinCell
        {
            get => _currentMinCell;
            private set => SetIfChangedAndRaise(ref _currentMinCell, value);
        }

        public ICell? CurrentMaxCell
        {
            get => _currentMaxCell;
            private set => SetIfChangedAndRaise(ref _currentMaxCell, value);
        }
        
        bool Cleared => Length == AssignedCells.Count;
        
        public ReadOnlyObservableCollection<ICell> AssignedCells { get; }
        
        public void AssignCell(ICell cell)
        {
            if (_assignedCells.Contains(cell)) return;
            _assignedCells.Add(cell);
            
            // Update min and max cell
            if (CurrentMinCell is { } minCell && _cellIterator.Coordinate(minCell) > _cellIterator.Coordinate(cell)
                || CurrentMinCell is null)
                CurrentMinCell = cell;
            if (CurrentMaxCell is { } maxCell && _cellIterator.Coordinate(maxCell) < _cellIterator.Coordinate(cell)
                || CurrentMaxCell is null)
                CurrentMaxCell = cell;

            var iCell = CurrentMinCell;
            while (iCell != CurrentMaxCell && _cellIterator.Next(iCell) is {} nextCell)
            {
                iCell = nextCell;
                if (_cellIterator.Assignment(iCell) == this)
                    continue;
                if (_cellIterator.Assignment(iCell) is not null && _cellIterator.Assignment(iCell) != this || iCell.State == CellState.Excluded)
                    throw new Exception();
                _cellIterator.Mark(iCell, this);
            }
            
            if (Cleared)
                OnPropertyChanged(nameof(Cleared));
        }
    }
}