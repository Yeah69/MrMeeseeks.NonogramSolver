using Avalonia.Media;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;

namespace MrMeeseeks.NonogramSolver.ViewModel.Game.Solving
{
    public interface ISegmentViewModel : IViewModelLayerBase
    {
        int Length { get; }
        ISolidColorBrush Background { get; }
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
    }
}