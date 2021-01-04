using MrMeeseeks.Extensions;
using MrMeeseeks.NonogramSolver.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface ILineViewModel : IViewModelLayerBase
    {
        IReadOnlyList<ISegmentViewModel> Segments { get; }
    }

    internal class LineViewModel : ViewModelLayerBase, ILineViewModel
    {
        public LineViewModel(
            // parameters
            ILine model,
            
            // dependencies
            Func<ISegment, ISegmentViewModel> segmentViewModelFactory) =>
            Segments = model
                .Segments
                .Select(segmentViewModelFactory)
                .ToReadOnlyList();

        public IReadOnlyList<ISegmentViewModel> Segments { get; }
    }
}