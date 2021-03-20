using MrMeeseeks.NonogramSolver.Model.Game.Solving;

namespace MrMeeseeks.NonogramSolver.Model.Game
{
    public interface ICellIterator
    {
        int Coordinate(ICell cell);
        ICell? Previous(ICell cell);
        ICell? Next(ICell cell);
        ISegment? Assignment(ICell cell);
        void Mark(ICell cell, ISegment segment);
    }

    public interface IVerticalCellIterator : ICellIterator
    {
    }


    public interface IHorizontalCellIterator : ICellIterator
    {
    }

    internal class VerticalCellIterator : IVerticalCellIterator
    {
        public int Coordinate(ICell cell) => cell.Y;

        public ICell? Previous(ICell cell) => cell.Up;

        public ICell? Next(ICell cell) => cell.Down;

        public ISegment? Assignment(ICell cell) => cell.VerticalAssignment;

        public void Mark(ICell cell, ISegment segment) => cell.MarkVertical(segment);
    }

    internal class HorizontalCellIterator : IHorizontalCellIterator
    {
        public int Coordinate(ICell cell) => cell.X;
        
        public ICell? Previous(ICell cell) => cell.Left;

        public ICell? Next(ICell cell) => cell.Right;

        public ISegment? Assignment(ICell cell) => cell.HorizontalAssignment;

        public void Mark(ICell cell, ISegment segment) => cell.MarkHorizontal(segment);
    }
}