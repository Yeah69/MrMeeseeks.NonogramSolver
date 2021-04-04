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
        bool Cleared { get; }

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
                if (cell.Y > 0) iCell.Vertical.Previous = Cells[i - Columns.Count].Vertical;
                if (cell.Y < Columns.Count - 1) iCell.Vertical.Next = Cells[i + Columns.Count].Vertical;
                if (cell.X > 0) iCell.Horizontal.Previous = Cells[i - 1].Horizontal;
                if (cell.X < Rows.Count - 1) iCell.Horizontal.Next = Cells[i + 1].Horizontal;
            }
        }
        
        public string Name { get; }
        
        public IReadOnlyList<ILine> Columns { get; }

        public IReadOnlyList<ILine> Rows { get; }
        
        public IReadOnlyList<ICell> Cells { get; }
        public bool Cleared => Columns.Concat(Rows).All(l => l.Cleared);

        public void Solve()
        {
            // Do the match thing
            foreach (ILine line in Columns)
                line.InitializeAssignments();

            foreach (ILine line in Rows)
                line.InitializeAssignments();

            do
            {
                while (Columns.Concat(Rows).Aggregate(false, (b, l) => l.CheckChildren() | b))
                {

                }
            } while (Columns.Concat(Rows).Aggregate(false, (b, l) => l.Check() | b));
        }

        public abstract void Save();

        public abstract void Delete();
    }
}