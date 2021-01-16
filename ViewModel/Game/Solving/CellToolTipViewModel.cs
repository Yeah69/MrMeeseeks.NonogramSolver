using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.ViewModel.Game.Solving
{
    public interface ICellToolTipViewModel
    {
        int X { get; }
        int Y { get; }
        string PossibleColumnSegments { get; }
        string PossibleRowSegments { get; }
    }

    internal class CellToolTipViewModel : ICellToolTipViewModel
    {
        private readonly ICell _cell;
        private readonly (ILine Column, ILine Row) _lines;

        public CellToolTipViewModel(
            // parameters
            ICell cell,
            (ILine Column, ILine Row) lines)
        {
            _cell = cell;
            _lines = lines;
        }

        public int X => _cell.X;
        public int Y => _cell.Y;

        public string PossibleColumnSegments => PossibleSegmentsTemplate(_lines.Column, _cell.Y);
        public string PossibleRowSegments => PossibleSegmentsTemplate(_lines.Row, _cell.X);
        
        private string PossibleSegmentsTemplate(ILine line, int index) =>
            string.Join(
                ", ", 
                line
                    .GetPossibleSegments(index)
                    .Select(s => (s == _cell.HorizontalAssignment || s == _cell.VerticalAssignment) 
                        ? $"_{s.Length}_" 
                        : s.Length.ToString()));
    }
}