using System;
using System.Collections.ObjectModel;

namespace MrMeeseeks.NonogramSolver.Model
{
    public interface ISegment : IModelLayerBase
    {
        int Length { get; }
        
        ReadOnlyObservableCollection<ICell> AssignedCells { get; }

        bool Cleared => Length == AssignedCells.Count;

        void AssignCell(ICell cell);
    }

    internal class Segment : ModelLayerBase, ISegment
    {
        private readonly ObservableCollection<ICell> _assignedCells = new();

        public Segment(int length)
        {
            Length = length > 0
                ? length
                : throw new ArgumentException("Segment length has to be greater than zero.");

            AssignedCells = new ReadOnlyObservableCollection<ICell>(_assignedCells);
        }

        public int Length { get; }
        
        public ReadOnlyObservableCollection<ICell> AssignedCells { get; }
        
        public void AssignCell(ICell cell)
        {
            _assignedCells.Add(cell);
        }
    }
}