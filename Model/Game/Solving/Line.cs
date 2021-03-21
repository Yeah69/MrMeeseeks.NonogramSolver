using MoreLinq;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;

namespace MrMeeseeks.NonogramSolver.Model.Game.Solving
{
    public interface ILine : IModelLayerBase
    {
        IReadOnlyList<ISegment> Segments { get; }
        IReadOnlyList<ILineCell> Cells { get; }
        ILine? Previous { get; }
        ILine? Next { get; }
        void AddCell(ILineCell cell);
        void InitializeAssignments();
    }

    internal class Line : ModelLayerBase, ILine
    {
        private readonly List<ILineCell> _cells = new();

        public Line(IReadOnlyList<ISegment> segments)
        {
            Segments = segments;

            SegmentsMinLength = Segments.Any()
                ? Segments.Sum(s => s.Length) + Segments.Count - 1
                : 0;

            Cells = _cells.ToReadOnlyList();

            segments
                .Select(s =>
                    s.ObservePropertyChanged(nameof(s.Cleared))
                        .Where(_ => s.Cleared)
                        .Do(_ =>
                        {
                            foreach (var cell in Cells.Except(s.AssignedCells))
                            {
                                cell.ExcludePossibleAssignment(s);
                            }
                        }))
                .Merge()
                .Subscribe();
        }

        private int SegmentsMinLength { get; }

        public IReadOnlyList<ISegment> Segments { get; }

        public IReadOnlyList<ILineCell> Cells { get; }

        public ILine? Previous { get; set; }

        public ILine? Next { get; set; }

        public void AddCell(ILineCell cell)
        {
            _cells.Add(cell);
        }

        public void InitializeAssignments()
        {
            Dictionary<ILineCell, ImmutableHashSet<ISegment>> cellsToPossibleSegments = new();
            int i = 0;
            ImmutableHashSet<ISegment>? possibleSegmentsList = ImmutableHashSet.Create<ISegment>();
            foreach (var segment in Segments)
            {
                possibleSegmentsList = possibleSegmentsList.Add(segment);
                for (int j = i; j < i + segment.Length + 1; j++)
                {
                    if (j < Cells.Count)
                    {
                        cellsToPossibleSegments.Add(Cells[j], possibleSegmentsList);
                    }
                }

                i += segment.Length + 1;
            }

            for (; i < Cells.Count; i++)
            {
                cellsToPossibleSegments.Add(Cells[i], possibleSegmentsList);
            }

            i = Cells.Count - 1;
            ImmutableHashSet<ISegment>? possibleSegmentsListInverse = ImmutableHashSet.Create<ISegment>();
            foreach (var segment in Segments.Reverse())
            {
                possibleSegmentsListInverse = possibleSegmentsListInverse.Add(segment);
                for (int j = i; j > i - segment.Length - 1; j--)
                {
                    if (j >= 0)
                    {
                        cellsToPossibleSegments[Cells[j]] =
                            cellsToPossibleSegments[Cells[j]].Intersect(possibleSegmentsListInverse);
                    }
                }

                i -= segment.Length + 1;
            }

            for (; i >= 0; i--)
            {
                cellsToPossibleSegments[Cells[i]] =
                    cellsToPossibleSegments[Cells[i]].Intersect(possibleSegmentsListInverse);
            }

            foreach (var (cell, possibleAssignments) in cellsToPossibleSegments)
            {
                cell.InitializePossibleAssignments(possibleAssignments);
            }

            foreach (var segment in Segments)
            {
                segment.InitializeTrivialAssignments();
            }
        }

        public void TryToAssignUnassigned()
        {
            foreach (var cell in Cells)
            {
                if (cell.State == CellState.Marked && cell.PossibleAssignments.Count == 1)
                {
                    if (cell.Assignment is { })
                    {
                        continue;
                    }

                    CheckAndMarkSuitableNeighbors(cell, cell.PossibleAssignments.First());
                }
            }

            void CheckAndMarkSuitableNeighbors(ILineCell seed, ISegment segment)
            {
                List<ILineCell>? segmentNeighborhood = new List<ILineCell> {seed};
                List<ILineCell>? previous = new List<ILineCell>();
                ILineCell? iCell = seed;
                int i = 1;
                while (i <= segment.Length && iCell.Previous is { } cell &&
                       (cell.State == CellState.Undecided
                        || (cell.State == CellState.Marked
                            && (cell.Assignment is null
                                || cell.Assignment == segment))))
                {
                    previous.Add(cell);
                    i++;
                    iCell = cell;
                }

                List<ILineCell> next = new ();
                iCell = seed;
                i = 1;
                while (i <= segment.Length && iCell.Next is { } cell &&
                       (cell.State == CellState.Undecided
                        || (cell.State == CellState.Marked
                            && (cell.Assignment is null
                                || cell.Assignment == segment))))
                {
                    next.Add(cell);
                    i++;
                    iCell = cell;
                }

                if (previous.Any(c => c.Assignment == segment))
                {
                    foreach (ILineCell cell in previous
                        .AsEnumerable()
                        .Reverse()
                        .SkipUntil(c => c.Assignment == segment)
                        .Where(c => c.Assignment != segment))
                    {
                        cell.Mark(segment);
                        segmentNeighborhood.Add(cell);
                        previous.Remove(cell);
                    }
                }

                if (next.Any(c => c.Assignment == segment))
                {
                    foreach (ILineCell cell in next
                        .AsEnumerable()
                        .Reverse()
                        .SkipUntil(c => c.Assignment == segment)
                        .Where(c => c.Assignment != segment))
                    {
                        cell.Mark(segment);
                        segmentNeighborhood.Add(cell);
                        next.Remove(cell);
                    }
                }

                int diff = segment.Length - segmentNeighborhood.Count - previous.Count;
                for (int j = 0; j < diff; j++)
                {
                    ILineCell? cell = next.First();
                    cell.Mark(segment);
                    segmentNeighborhood.Add(cell);
                    next.Remove(cell);
                }

                diff = segment.Length - segmentNeighborhood.Count - next.Count;
                for (int j = 0; j < diff; j++)
                {
                    ILineCell? cell = previous.First();
                    cell.Mark(segment);
                    segmentNeighborhood.Add(cell);
                    previous.Remove(cell);
                }
            }
        }
    }
}