namespace MrMeeseeks.NonogramSolver.Model
{
    public interface ICellIterator
    {
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
        public ICell? Previous(ICell cell) => cell.Up;

        public ICell? Next(ICell cell) => cell.Down;

        public ISegment? Assignment(ICell cell) => cell.VerticalAssignment;

        public void Mark(ICell cell, ISegment segment) => cell.MarkVertical(segment);
    }

    internal class HorizontalCellIterator : IHorizontalCellIterator
    {
        public ICell? Previous(ICell cell) => cell.Left;

        public ICell? Next(ICell cell) => cell.Right;

        public ISegment? Assignment(ICell cell) => cell.HorizontalAssignment;

        public void Mark(ICell cell, ISegment segment) => cell.MarkHorizontal(segment);
    }
}