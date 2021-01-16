using MrMeeseeks.NonogramSolver.Model.Game.Solving;

namespace MrMeeseeks.NonogramSolver.ViewModel.Game.Solving
{
    public interface ISegmentViewModel : IViewModelLayerBase
    {
        int Length { get; }
    }

    internal class SegmentViewModel : ViewModelLayerBase, ISegmentViewModel
    {
        private readonly ISegment _model;

        public SegmentViewModel(ISegment model) => _model = model;

        public int Length => _model.Length;
    }
}