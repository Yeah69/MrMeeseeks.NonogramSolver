using MrMeeseeks.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.Model.Game.Solving
{
    public interface ILine : IModelLayerBase
    {
        IReadOnlyList<ISegment> Segments { get; }
        IReadOnlyList<ILineCell> Cells { get; }
        bool Cleared { get; }
        void AddCell(ILineCell cell);
        void InitializeAssignments();

        bool Check();
    }

    internal class Line : ModelLayerBase, ILine
    {
        private readonly List<ILineCell> _cells = new();

        public Line(IReadOnlyList<ISegment> segments)
        {
            Segments = segments;

            ISegment prevSegment = segments[0];
            foreach (var segment in segments.Skip(1))
            {
                segment.Previous = prevSegment;
                prevSegment.Next = segment;
                prevSegment = segment;
            }

            Cells = _cells.ToReadOnlyList();
        }

        public IReadOnlyList<ISegment> Segments { get; }

        public IReadOnlyList<ILineCell> Cells { get; }
        
        public bool Cleared => Segments.All(s => s.Cleared);

        public void AddCell(ILineCell cell)
        {
            _cells.Add(cell);
        }

        public void InitializeAssignments()
        {
            Dictionary<ILineCell, ImmutableHashSet<ISegment>> cellsToPossibleSegments = new();
            var i = 0;
            var possibleSegmentsList = ImmutableHashSet.Create<ISegment>();
            foreach (var segment in Segments)
            {
                possibleSegmentsList = possibleSegmentsList.Add(segment);
                for (int j = i; j < i + segment.Length + 1; j++)
                    if (j < Cells.Count)
                        cellsToPossibleSegments.Add(Cells[j], possibleSegmentsList);

                i += segment.Length + 1;
            }

            for (; i < Cells.Count; i++) cellsToPossibleSegments.Add(Cells[i], possibleSegmentsList);

            i = Cells.Count - 1;
            var possibleSegmentsListInverse = ImmutableHashSet.Create<ISegment>();
            foreach (var segment in Segments.Reverse())
            {
                possibleSegmentsListInverse = possibleSegmentsListInverse.Add(segment);
                for (int j = i; j > i - segment.Length - 1; j--)
                    if (j >= 0)
                        cellsToPossibleSegments[Cells[j]] =
                            cellsToPossibleSegments[Cells[j]].Intersect(possibleSegmentsListInverse);

                i -= segment.Length + 1;
            }

            for (; i >= 0; i--)
                cellsToPossibleSegments[Cells[i]] =
                    cellsToPossibleSegments[Cells[i]].Intersect(possibleSegmentsListInverse);

            foreach (var (cell, possibleAssignments) in cellsToPossibleSegments)
                cell.InitializePossibleAssignments(possibleAssignments);

            var segmentToCellsGroups = cellsToPossibleSegments
                .SelectMany(kvp => kvp.Value.Select(s => (kvp.Key, s)))
                .GroupBy(t => t.s, t => t.Key);
            
            foreach (var segmentToCellsGroup in segmentToCellsGroups)
                segmentToCellsGroup
                    .Key
                    .InitializePossibleAssignments(
                        ImmutableHashSet.Create(
                            segmentToCellsGroup.ToArray()));
        }

        public bool Check()
        {
            var isChecked = false;
            while (Inner()) isChecked = true;
            return isChecked;

            bool Inner()
            {
                var ret = Cells.Aggregate(false, (b, c) => c.Check() | b);
                return Segments.Aggregate(ret, (b, s) => s.Check() | b);
            }
        }
    }
}