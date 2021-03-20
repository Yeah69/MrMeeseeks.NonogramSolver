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
        void DoTheMatchThing();
        void InitializeAssignments();
        void TryToAssignUnassigned();
        IReadOnlyList<ISegment> GetPossibleSegments(ILineCell cell);
    }

    internal class Line : ModelLayerBase, ILine
    {
        private readonly List<ILineCell> _cells = new();
        private readonly Dictionary<ILineCell, ImmutableHashSet<ISegment>> _cellsToPossibleSegments = new();
        
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
                            if (s.CurrentMinCell is {} minCell && minCell.Previous is {} prevToMinCell)
                            {
                                prevToMinCell.Exclude();
                            }
                            if (s.CurrentMaxCell is {} maxCell && maxCell.Next is {} nextToMaxCell)
                            {
                                nextToMaxCell.Exclude();
                            }

                            foreach (var cell in _cellsToPossibleSegments.Keys.Except(s.AssignedCells))
                            {
                                _cellsToPossibleSegments[cell] =
                                    _cellsToPossibleSegments[cell].Except(s.ToEnumerable());
                                if (_cellsToPossibleSegments[cell].Count == 0 && cell.State == CellState.Undecided)
                                    cell.Exclude();
                            }
                        }))
                .Merge()
                .Subscribe();
        }
        
        public IReadOnlyList<ISegment> Segments { get; }
        
        private int SegmentsMinLength { get; }
        
        public IReadOnlyList<ILineCell> Cells { get; }
        
        public ILine? Previous { get; set; }
        
        public ILine? Next { get; set; }

        public void AddCell(ILineCell cell) => _cells.Add(cell);

        public void DoTheMatchThing()
        {
            ///*
            var diff = Cells.Count - SegmentsMinLength;
            if (diff < 0) throw new Exception();
            int i = 0;
            foreach (var segment in Segments)
            {
                i += Math.Min(diff, segment.Length);
                var segmentDiff = Math.Max(0, segment.Length - diff);
                foreach (var j in Enumerable.Range(i, segmentDiff))
                {
                    Cells[j].Mark(segment);
                    _cellsToPossibleSegments[Cells[j]] = ImmutableHashSet.Create(segment);
                }
                i += segmentDiff;
                if (diff == 0 && i < Cells.Count)
                {
                    Cells[i].Exclude();
                    _cellsToPossibleSegments[Cells[i]] = ImmutableHashSet<ISegment>.Empty;
                }
                i++;
            }//*/

            /*
            foreach (var segment in Segments)
            {
                var segmentSpan = _cellsToPossibleSegments
                    .Select((hs, i) => (hs, i))
                    .SkipWhile(t => t.hs.Contains(segment).Not())
                    .TakeWhile(t => t.hs.Contains(segment))
                    .ToReadOnlyList();

                for (i = Math.Max(0, segmentSpan.Count - segment.Length); i < Math.Min(segmentSpan.Count, segment.Length); i++)
                    _cellIterator.Mark(Cells[segmentSpan[i].i], segment);
            }//*/
        }

        public void InitializeAssignments()
        {
            int i = 0;
            var possibleSegmentsList = ImmutableHashSet.Create<ISegment>();
            foreach (var segment in Segments)
            {
                possibleSegmentsList = possibleSegmentsList.Add(segment);
                for (int j = i; j < i + segment.Length + 1; j++)
                    if (j < Cells.Count)
                        _cellsToPossibleSegments.Add(Cells[j], possibleSegmentsList);
                i += segment.Length + 1;
            }
            for (; i < Cells.Count; i++)
                _cellsToPossibleSegments.Add(Cells[i], possibleSegmentsList);
            
            i = Cells.Count - 1;
            var possibleSegmentsListInverse = ImmutableHashSet.Create<ISegment>();
            foreach (var segment in Segments.Reverse())
            {
                possibleSegmentsListInverse = possibleSegmentsListInverse.Add(segment);
                for (int j = i; j > i - segment.Length - 1; j--)
                    if (j >= 0)
                        _cellsToPossibleSegments[Cells[j]] = _cellsToPossibleSegments[Cells[j]].Intersect(possibleSegmentsListInverse);
                i -= segment.Length + 1;
            }
            for (; i >= 0; i--)
                _cellsToPossibleSegments[Cells[i]] = _cellsToPossibleSegments[Cells[i]].Intersect(possibleSegmentsListInverse);
        }

        public void TryToAssignUnassigned()
        {
            foreach (var cell in Cells)
            {
                if (cell.State == CellState.Marked && _cellsToPossibleSegments[cell].Count == 1)
                {
                    if (cell.Assignment is {}) continue;
                    cell.Mark(_cellsToPossibleSegments[cell].First());
                    CheckAndMarkSuitableNeighbors(cell, _cellsToPossibleSegments[cell].First());
                }
            }
        }

        public IReadOnlyList<ISegment> GetPossibleSegments(ILineCell cell) => 
            _cellsToPossibleSegments[cell].ToReadOnlyList();

        private void CheckAndMarkSuitableNeighbors(ILineCell seed, ISegment segment)
        {
            var segmentNeighborhood = new List<ILineCell> { seed };
            var previous = new List<ILineCell>();
            var iCell = seed;
            var i = 1;
            while (i <= segment.Length && iCell.Previous is { } cell && 
                   (cell.State == CellState.Undecided 
                    || cell.State == CellState.Marked 
                    && (cell.Assignment is null 
                        || cell.Assignment == segment)))
            {
                previous.Add(cell);
                i++;
                iCell = cell;
            }
            var next = new List<ILineCell>();
            iCell = seed;
            i = 1;
            while (i <= segment.Length && iCell.Next is { } cell && 
                   (cell.State == CellState.Undecided 
                    || cell.State == CellState.Marked 
                    && (cell.Assignment is null 
                        || cell.Assignment == segment)))
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

            var diff = segment.Length - segmentNeighborhood.Count - previous.Count;
            for (int j = 0; j < diff; j++)
            {
                var cell = next.First();
                cell.Mark(segment);
                segmentNeighborhood.Add(cell);
                next.Remove(cell);
            }

            diff = segment.Length - segmentNeighborhood.Count - next.Count;
            for (int j = 0; j < diff; j++)
            {
                var cell = previous.First();
                cell.Mark(segment);
                segmentNeighborhood.Add(cell);
                previous.Remove(cell);
            }
        }
    }
}