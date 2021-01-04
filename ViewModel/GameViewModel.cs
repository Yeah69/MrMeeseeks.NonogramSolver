using MoreLinq;
using System;
using MrMeeseeks.Extensions;
using MrMeeseeks.NonogramSolver.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface IGameViewModel : IViewModelLayerBase
    {
        IReadOnlyList<IReadOnlyList<ILineViewModel>> Columns { get; }
        IReadOnlyList<IReadOnlyList<ILineViewModel>> Rows { get; }
        IReadOnlyList<ICellViewModel> Cells { get; }
        IReadOnlyList<ICellBlockViewModel> CellBlocks { get; }
        int ColumnCount { get; }
        int BlockColumnCount { get; }
        void SaveGame();
        void Solve();
    }

    internal class GameViewModel : ViewModelLayerBase, IGameViewModel
    {
        private readonly IGame _model;
        private readonly ISaveGame _saveGame;

        public GameViewModel(
            // parameters
            IGame model,
            
            // dependencies
            ISaveGame saveGame,
            Func<ILine, ILineViewModel> lineEditorViewModelFactory,
            Func<ICell, ICellViewModel> cellViewModelFactory,
            Func<IReadOnlyList<ICellViewModel>, int, ICellBlockViewModel> cellBlockViewModelFactory)
        {
            _model = model;
            _saveGame = saveGame;
            
            Columns = GenerateLineHeaders(model.Columns);
            
            Rows = GenerateLineHeaders(model.Rows);

            Cells = model
                .Cells
                .Select(cellViewModelFactory)
                .ToReadOnlyList();

            ColumnCount = model.Columns.Count;
            BlockColumnCount = CalculateBlockLength(model.Columns.Count);
            var blockRowCount = CalculateBlockLength(model.Rows.Count);

            var cellBlocks = new List<ICellBlockViewModel>();

            for (int y = 0; y < blockRowCount; y++)
            {
                var yStart = y * 5;
                var yLimit = yStart + CalculateLimitOffset(y, model.Columns.Count);
                for (int x = 0; x < BlockColumnCount; x++)
                {
                    var xStart = x * 5;
                    var xLimitOffset = CalculateLimitOffset(x, model.Rows.Count);
                    var xLimit = xStart + xLimitOffset;
                    var cells = new List<ICellViewModel>();
                    for (int y_ = yStart; y_ < yLimit; y_++)
                    {
                        for (int x_ = xStart; x_ < xLimit; x_++)
                        {
                            cells.Add(Cells[y_ * model.Columns.Count + x_]);
                        }
                    }

                    cellBlocks.Add(cellBlockViewModelFactory(cells, xLimitOffset));
                }
            }

            CellBlocks = cellBlocks.ToReadOnlyList();

            static int CalculateBlockLength(int lineCount) => lineCount / 5 + (lineCount % 5 == 0 ? 0 : 1);

            static int CalculateLimitOffset(int value, int lineCount) => value == lineCount / 5 && lineCount % 5 > 0 
                ? lineCount % 5 
                : 5;
            
            IReadOnlyList<IReadOnlyList<ILineViewModel>> GenerateLineHeaders(IReadOnlyList<ILine> source) =>
                source
                    .Select(lineEditorViewModelFactory)
                    .Batch(5)
                    .Select(l => l.ToReadOnlyList())
                    .ToReadOnlyList();
        }

        public IReadOnlyList<IReadOnlyList<ILineViewModel>> Columns { get; }

        public IReadOnlyList<IReadOnlyList<ILineViewModel>> Rows { get; }
        public IReadOnlyList<ICellViewModel> Cells { get; }
        public IReadOnlyList<ICellBlockViewModel> CellBlocks { get; }
        public int ColumnCount { get; }
        public int BlockColumnCount { get; }

        public void SaveGame() => _saveGame.Save(
            _model, 
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "MrMeeseeksNonogramSolver",
                "SaveGame.json"));

        public void Solve() => _model.Solve();
    }
}