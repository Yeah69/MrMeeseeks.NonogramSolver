using System.Collections.Generic;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface ICellBlockViewModel
    {
        IReadOnlyList<ICellViewModel> Cells { get; }
        int ColumnCount { get; }
    }

    internal class CellBlockViewModel : ICellBlockViewModel
    {
        public CellBlockViewModel(IReadOnlyList<ICellViewModel> cells, int columnCount)
        {
            Cells = cells;
            ColumnCount = columnCount;
        }

        public IReadOnlyList<ICellViewModel> Cells { get; }
        public int ColumnCount { get; }
    }
}