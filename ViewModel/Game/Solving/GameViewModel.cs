using MoreLinq;
using MrMeeseeks.Extensions;
using MrMeeseeks.NonogramSolver.Model.Game;
using MrMeeseeks.NonogramSolver.Model.Game.Editing;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using MrMeeseeks.NonogramSolver.ViewModel.Game.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.ViewModel.Game.Solving
{
    public interface IGameViewModel : IViewModelLayerBase
    {
        string Name { get; }
        IReadOnlyList<IReadOnlyList<ILineViewModel>> Columns { get; }
        IReadOnlyList<int> ColumnsFiveStepHints { get; }
        IReadOnlyList<IReadOnlyList<ILineViewModel>> Rows { get; }
        IReadOnlyList<int> RowsFiveStepHints { get; }
        IReadOnlyList<ICellViewModel> Cells { get; }
        IReadOnlyList<ICellBlockViewModel> CellBlocks { get; }
        int ColumnCount { get; }
        int BlockColumnCount { get; }
        void Solve();
        void Delete();
        IGameEditorViewModel CreateEditableCopy();
    }

    internal class GameViewModel : ViewModelLayerBase, IGameViewModel
    {
        private readonly IGame _model;
        private readonly IGameProject _gameProject;
        private readonly Func<IGameEditor> _gameEditorFactory;
        private readonly Func<IGameEditor, IGameEditorViewModel> _gameEditorViewModelFactory;

        public GameViewModel(
            // parameters
            IGame model,
            IGameProject gameProject,
            
            // dependencies
            Func<ILine, ILineViewModel> lineEditorViewModelFactory,
            Func<ICell, (ILine Column, ILine Row), ICellViewModel> cellViewModelFactory,
            Func<IGameEditor> gameEditorFactory,
            Func<IGameEditor, IGameEditorViewModel> gameEditorViewModelFactory,
            Func<IReadOnlyList<ICellViewModel>, int, ICellBlockViewModel> cellBlockViewModelFactory)
        {
            _model = model;
            _gameProject = gameProject;
            _gameEditorFactory = gameEditorFactory;
            _gameEditorViewModelFactory = gameEditorViewModelFactory;

            Columns = GenerateLineHeaders(model.Columns);
            
            Rows = GenerateLineHeaders(model.Rows);

            Cells = model
                .Cells
                .Select(c => cellViewModelFactory(c, (model.Columns[c.Vertical.Position], model.Rows[c.Horizontal.Position])))
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

            ColumnsFiveStepHints = GenerateFiveStepHints(Columns);

            RowsFiveStepHints = GenerateFiveStepHints(Rows);

            static int CalculateBlockLength(int lineCount) => lineCount / 5 + (lineCount % 5 == 0 ? 0 : 1);

            static int CalculateLimitOffset(int value, int lineCount) => value == lineCount / 5 && lineCount % 5 > 0 
                ? lineCount % 5 
                : 5;
            
            IReadOnlyList<int> GenerateFiveStepHints(IReadOnlyList<IReadOnlyList<ILineViewModel>> source) =>
                source
                    .Where(lines => lines.Count == 5)
                    .Scan(0, (acc, _) => acc + 5)
                    .Skip(1)
                    .ToReadOnlyList();
            
            IReadOnlyList<IReadOnlyList<ILineViewModel>> GenerateLineHeaders(IReadOnlyList<ILine> source) =>
                source
                    .Select(lineEditorViewModelFactory)
                    .Batch(5)
                    .Select(l => l.ToReadOnlyList())
                    .ToReadOnlyList();
        }

        public string Name => _model.Name;
        public IReadOnlyList<IReadOnlyList<ILineViewModel>> Columns { get; }
        public IReadOnlyList<int> ColumnsFiveStepHints { get; }

        public IReadOnlyList<IReadOnlyList<ILineViewModel>> Rows { get; }
        public IReadOnlyList<int> RowsFiveStepHints { get; }
        public IReadOnlyList<ICellViewModel> Cells { get; }
        public IReadOnlyList<ICellBlockViewModel> CellBlocks { get; }
        public int ColumnCount { get; }
        public int BlockColumnCount { get; }

        public void Solve() => Task.Run(() =>_model.Solve());
        public void Delete() => _gameProject.Delete(_model);
        public IGameEditorViewModel CreateEditableCopy()
        {
            var gameEditor = _gameEditorFactory();
            foreach (var line in _model.Columns)
            {
                gameEditor.AddColumn();
                var column = gameEditor.Columns.Last();
                SetupLine(line, column);
            }
            foreach (var line in _model.Rows)
            {
                gameEditor.AddRow();
                var row = gameEditor.Rows.Last();
                SetupLine(line, row);
            }

            gameEditor.Name = $"{_model.Name} Copy";

            return _gameEditorViewModelFactory(gameEditor);

            static void SetupLine(ILine line, ILineEditor lineEditor)
            {
                foreach (var segment in line.Segments)
                {
                    lineEditor.AddSegment();
                    lineEditor.Segments.Last().Length = segment.Length;
                }
            }
        }
    }
}