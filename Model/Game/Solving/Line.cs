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
        IReadOnlyList<ICell> Cells { get; }
        ILine? Previous { get; }
        ILine? Next { get; }
        void AddCell(ICell cell);
        void DoTheMatchThing();
        void InitializeAssignments();
        void TryToAssignUnassigned();
        IReadOnlyList<ISegment> GetPossibleSegments(ICell cell);
    }

    internal class Line : ModelLayerBase, ILine
    {
        private readonly ICellIterator _cellIterator;
        private readonly List<ICell> _cells = new();
        private readonly Dictionary<ICell, ImmutableHashSet<ISegment>> _cellsToPossibleSegments = new();
        
        public Line(
            IReadOnlyList<ISegment> segments,
            ICellIterator cellIterator)
        {
            _cellIterator = cellIterator;
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
                            if (s.CurrentMinCell is {} minCell && cellIterator.Previous(minCell) is {} prevToMinCell)
                            {
                                prevToMinCell.Exclude();
                            }
                            if (s.CurrentMaxCell is {} maxCell && cellIterator.Next(maxCell) is {} nextToMaxCell)
                            {
                                nextToMaxCell.Exclude();
                            }

                            foreach (var cell in _cellsToPossibleSegments.Keys.Except(s.AssignedCells))
                            {
                                _cellsToPossibleSegments[cell] =
                                    _cellsToPossibleSegments[cell].Except(s.ToEnumerable());
                                if (_cellsToPossibleSegments[cell].Count == 0 && cell.State == CellState.Undecided)
                                    cell.Exclude();
                                if (_cellsToPossibleSegments[cell].Count == 1 
                                    && cell is {State: CellState.Marked} 
                                    && cellIterator.Assignment(cell) is null)
                                    cellIterator.Mark(cell, _cellsToPossibleSegments[cell].First());
                            }
                        }))
                .Merge()
                .Subscribe();
        }
        
        public IReadOnlyList<ISegment> Segments { get; }
        
        private int SegmentsMinLength { get; }
        
        public IReadOnlyList<ICell> Cells { get; }
        
        public ILine? Previous { get; set; }
        
        public ILine? Next { get; set; }

        public void AddCell(ICell cell) => _cells.Add(cell);

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
                    _cellIterator.Mark(Cells[j], segment);
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
                    if (_cellIterator.Assignment(cell) is {}) continue;
                    _cellIterator.Mark(cell, _cellsToPossibleSegments[cell].First());
                    CheckAndMarkSuitableNeighbors(cell, _cellsToPossibleSegments[cell].First());
                }
            }
        }

        public IReadOnlyList<ISegment> GetPossibleSegments(ICell cell) => 
            _cellsToPossibleSegments[cell].ToReadOnlyList();

        private void CheckAndMarkSuitableNeighbors(ICell seed, ISegment segment)
        {
            var segmentNeighborhood = new List<ICell> { seed };
            var previous = new List<ICell>();
            var iCell = seed;
            var i = 1;
            while (i <= segment.Length && _cellIterator.Previous(iCell) is { } cell && 
                   (cell.State == CellState.Undecided || cell.State == CellState.Marked && (_cellIterator.Assignment(cell) is null ||_cellIterator.Assignment(cell) == segment)))
            {
                previous.Add(cell);
                i++;
                iCell = cell;
            }
            var next = new List<ICell>();
            iCell = seed;
            i = 1;
            while (i <= segment.Length && _cellIterator.Next(iCell) is { } cell && 
                   (cell.State == CellState.Undecided || cell.State == CellState.Marked && (_cellIterator.Assignment(cell) is null ||_cellIterator.Assignment(cell) == segment)))
            {
                next.Add(cell);
                i++;
                iCell = cell;
            }

            if (previous.Any(c => _cellIterator.Assignment(c) == segment))
            {
                foreach (ICell cell in previous
                    .AsEnumerable()
                    .Reverse()
                    .SkipUntil(c => _cellIterator.Assignment(c) == segment)
                    .Where(c => _cellIterator.Assignment(c) != segment))
                {
                    _cellIterator.Mark(cell, segment);
                    segmentNeighborhood.Add(cell);
                    previous.Remove(cell);
                }
            }

            if (next.Any(c => _cellIterator.Assignment(c) == segment))
            {
                foreach (ICell cell in next
                    .AsEnumerable()
                    .Reverse()
                    .SkipUntil(c => _cellIterator.Assignment(c) == segment)
                    .Where(c => _cellIterator.Assignment(c) != segment))
                {
                    _cellIterator.Mark(cell, segment);
                    segmentNeighborhood.Add(cell);
                    next.Remove(cell);
                }
            }

            var diff = segment.Length - segmentNeighborhood.Count - previous.Count;
            for (int j = 0; j < diff; j++)
            {
                var cell = next.First();
                _cellIterator.Mark(cell, segment);
                segmentNeighborhood.Add(cell);
                next.Remove(cell);
            }

            diff = segment.Length - segmentNeighborhood.Count - next.Count;
            for (int j = 0; j < diff; j++)
            {
                var cell = previous.First();
                _cellIterator.Mark(cell, segment);
                segmentNeighborhood.Add(cell);
                previous.Remove(cell);
            }
        }
    }
}