using MrMeeseeks.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.Model.Game.Solving
{
    public interface ILine : IModelLayerBase
    {
        IReadOnlyList<ISegment> Segments { get; }
        IReadOnlyList<ILineCellForLine> Cells { get; }
        bool Cleared { get; }
        void AddCell(ILineCellForLine cell);
        void InitializeAssignments();

        bool CheckChildren();

        bool Check();
    }

    internal class Line : ModelLayerBase, ILine
    {
        private readonly List<ILineCellForLine> _cells = new();

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

        public IReadOnlyList<ILineCellForLine> Cells { get; }

        public bool Cleared => Segments.All(s => s.Cleared);

        public void AddCell(ILineCellForLine cell) => _cells.Add(cell);

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

        public bool CheckChildren()
        {
            var isChecked = false;
            while (Inner()) isChecked = true;
            return isChecked;

            bool Inner()
            {
                bool ret = Cells.Aggregate(false, (b, c) => c.Check() | b);
                ret = Segments.Aggregate(ret, (b, s) => s.Check() | b);

                return ret;
            }
        }

        public bool Check()
        {
            var ret = false;
            var groupsOfNeighboredMarkedButUnassignedCells = Cells
                .Where(c => c.State == CellState.Marked && c.Assignment is null)
                .OrderBy(c => c.Position)
                .Aggregate(new List<List<ILineCellForLine>>(), (lol, c) =>
                {
                    if (lol.None() || lol.Last().Last().Position + 1 < c.Position)
                        lol.Add(new List<ILineCellForLine> {c});
                    else
                        lol.Last().Add(c);
                    return lol;
                });
            
            foreach (var group in groupsOfNeighboredMarkedButUnassignedCells)
            {
                var nextToMaxCell = group.MaxBy(c => c.Position)?.Next as ILineCellForLine;
                var prevToMinCell = group.MinBy(c => c.Position)?.Previous as ILineCellForLine;
                
                var tooSmallPossibleAssignments =
                    group
                        .SelectMany(c => c.PossibleAssignments.Select(s => (s, c)))
                        .Where(t => t.s.Length < group.Count)
                        .ToList();
                if (tooSmallPossibleAssignments.Any())
                {
                    ret = true;
                    foreach (var (s, c) in tooSmallPossibleAssignments)
                        c.ExcludePossibleAssignment(s);
                }

                if (group.SelectMany(c => c.PossibleAssignments).All(s => s.Length == group.Count))
                {
                    if (nextToMaxCell is not null)
                    {
                        ret = true;
                        nextToMaxCell.ExcludeAllPossibleAssignments();
                    }
                    if (prevToMinCell is not null)
                    {
                        ret = true;
                        prevToMinCell.ExcludeAllPossibleAssignments();
                    }
                }

                if ((nextToMaxCell ?? prevToMinCell) is {State: CellState.Excluded})
                {
                    var min = @group.SelectMany(c => c.PossibleAssignments)
                        .Select(s => s.Length)
                        .Min();
                    var diff = min - group.Count;
                    if (diff > 0)
                    {
                        ret = true;
                        if (nextToMaxCell is {State: CellState.Undecided}) nextToMaxCell.MarkWithoutAssignment();
                        if (prevToMinCell is {State: CellState.Undecided}) prevToMinCell.MarkWithoutAssignment();
                    }
                }
            }

            if (groupsOfNeighboredMarkedButUnassignedCells.Any() && Segments.Count >= 2
             && groupsOfNeighboredMarkedButUnassignedCells[0].Count == Segments[0].Length
             && Segments[0].AssignedCells.None() && Segments[1].AssignedCells.None()
             && Cells[groupsOfNeighboredMarkedButUnassignedCells[0].Count].State == CellState.Undecided
             && groupsOfNeighboredMarkedButUnassignedCells[0].MinBy(c => c.Position)?.Position ==
                groupsOfNeighboredMarkedButUnassignedCells.Count + 1)
            {
                ret = true;
                Cells[groupsOfNeighboredMarkedButUnassignedCells[0].Count].ExcludeAllPossibleAssignments();
            }

            if (groupsOfNeighboredMarkedButUnassignedCells.Any() 
             && Segments.Count >= 2
             && groupsOfNeighboredMarkedButUnassignedCells[^1].Count == Segments[^1].Length
             && Segments[^1].AssignedCells.None() && Segments[^2].AssignedCells.None()
             && Cells[^(groupsOfNeighboredMarkedButUnassignedCells[^1].Count + 1)].State == CellState.Undecided
             && groupsOfNeighboredMarkedButUnassignedCells[^1].MaxBy(c => c.Position)?.Position ==
                Cells.Count - groupsOfNeighboredMarkedButUnassignedCells.Count - 2)
            {
                ret = true;
                Cells[^(groupsOfNeighboredMarkedButUnassignedCells[^1].Count + 1)].ExcludeAllPossibleAssignments();
            }

            return ret;
        }
    }
}