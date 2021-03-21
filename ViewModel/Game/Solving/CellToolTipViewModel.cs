using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System.Collections.Generic;
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

        public CellToolTipViewModel(
            // parameters
            ICell cell)
        {
            _cell = cell;
        }

        public int X => _cell.Horizontal.Position;
        public int Y => _cell.Vertical.Position;

        public string PossibleColumnSegments => PossibleSegmentsTemplate(_cell.Vertical.PossibleAssignments);
        public string PossibleRowSegments => PossibleSegmentsTemplate(_cell.Horizontal.PossibleAssignments);
        
        private string PossibleSegmentsTemplate(IEnumerable<ISegment> possibleSegments) =>
            string.Join(
                ", ", 
                possibleSegments
                    .Select(s => (s == _cell.Horizontal.Assignment || s == _cell.Vertical.Assignment) 
                        ? $"_{s.Length}_" 
                        : s.Length.ToString()));
    }
}