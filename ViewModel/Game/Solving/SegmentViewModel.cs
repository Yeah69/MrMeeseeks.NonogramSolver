using Avalonia.Media;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.ViewModel.Game.Solving
{
    public interface ISegmentViewModel : IViewModelLayerBase
    {
        int Length { get; }
        ISolidColorBrush Background { get; }
        String ToolTip { get; }
    }

    internal class SegmentViewModel : ViewModelLayerBase, ISegmentViewModel
    {
        private readonly ISegment _model;

        public SegmentViewModel(ISegment model)
        {
            _model = model;
            EscalateNotifications(
                model, 
                (nameof(model.Cleared), nameof(Background)));
        }

        public int Length => _model.Length;

        public ISolidColorBrush Background => _model.Cleared
            ? Brushes.DarkGray
            : Brushes.Black;

        public string ToolTip => $"P. Cells: {string.Join(", ", _model.CurrentPossibleCells.Except(_model.AssignedCells).Select(c => c.Position))}";
    }
}