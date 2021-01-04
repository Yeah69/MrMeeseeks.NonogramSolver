using MrMeeseeks.NonogramSolver.Model;

namespace MrMeeseeks.NonogramSolver.ViewModel
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