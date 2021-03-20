using MrMeeseeks.Extensions;
using MrMeeseeks.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.Model.Game.Solving
{
    public interface IGame : IModelLayerBase
    {
        string Name { get; }
        IReadOnlyList<ILine> Columns { get; }
        IReadOnlyList<ILine> Rows { get; }
        IReadOnlyList<ICell> Cells { get; }

        void Solve();

        void Save();
        void Delete();
    }

    public abstract class Game : ObservableObject, IGame
    {
        private readonly List<ICell> _cells = new ();
        
        public Game(
            // parameters
            string name,
            (IReadOnlyList<ILine>, IReadOnlyList<ILine>) columnsAndRows,
            
            // dependencies
            Func<(int, int), ICell> cellFactory)
        {
            Name = name;
            (Columns, Rows) = columnsAndRows;
            
            Cells = _cells.ToReadOnlyList();

            for (int y = 0; y < Rows.Count; y++)
            {
                for (int x = 0; x < Columns.Count; x++)
                {
                    var cell = cellFactory((x, y));
                    Columns[x].AddCell(cell.Vertical);
                    Rows[y].AddCell(cell.Horizontal);
                    _cells.Add(cell);
                }
            }
            
            foreach (var iCell in Cells)
            {
                var cell = (Cell)iCell;
                var i = cell.Y * Columns.Count + cell.X;
                if (cell.Y > 0) cell.Up = (Cell)Cells[i - Columns.Count];
                if (cell.Y < Columns.Count - 1) cell.Down = (Cell)Cells[i + Columns.Count];
                if (cell.X > 0) cell.Left = (Cell)Cells[i - 1];
                if (cell.X < Rows.Count - 1) cell.Right = (Cell)Cells[i + 1];
            }

            for (int y = 0; y < Rows.Count; y++)
            {
                var line = (Line) Rows[y];
                if (y > 0) line.Previous = Rows[y - 1];
                if (y < Rows.Count - 1) line.Next = Rows[y + 1];
            }
            
            for (int x = 0; x < Columns.Count; x++)
            {
                var line = (Line) Columns[x];
                if (x > 0) line.Previous = Columns[x - 1];
                if (x < Rows.Count - 1) line.Next = Columns[x + 1];
            }
        }
        
        public string Name { get; }
        
        public IReadOnlyList<ILine> Columns { get; }

        public IReadOnlyList<ILine> Rows { get; }
        
        public IReadOnlyList<ICell> Cells { get; }

        public void Solve()
        {
            // Exclude empty lines
            foreach (var cell in Columns
                .Where(c => c.Segments.None())
                .SelectMany(c => c.Cells)
                .Concat(Rows
                    .Where(c => c.Segments.None())
                    .SelectMany(c => c.Cells))
                .Distinct())
                cell.Exclude();
            
            // Do the match thing
            foreach (ILine line in Columns)
            {
                line.InitializeAssignments();
                line.DoTheMatchThing();
            }

            foreach (ILine line in Rows)
            {
                line.InitializeAssignments();
                line.DoTheMatchThing();
            }
            
            foreach (ILine line in Columns)
            {
                line.TryToAssignUnassigned();
            }

            foreach (ILine line in Rows)
            {
                line.TryToAssignUnassigned();
            }
        }

        public abstract void Save();

        public abstract void Delete();
    }
}