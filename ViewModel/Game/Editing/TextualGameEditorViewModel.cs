using MrMeeseeks.NonogramSolver.Model.Game;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.ViewModel.Game.Editing
{
    public interface ITextualGameEditorViewModel : IGameEditorViewModelBase
    {
        string Columns { get; set; }
        string Rows { get; set; }
    }

    public class TextualGameEditorViewModel : ViewModelLayerBase, ITextualGameEditorViewModel
    {
        private readonly ICreateGame _createGame;
        private readonly Func<IReadOnlyList<ISegment>, ILine> _lineFactory;
        private readonly Func<int, ISegment> _segmentFactory;
        private readonly TaskCompletionSource<IGame> _resultSource;
        private string _columns = string.Empty;
        private string _rows = string.Empty;
        private string _name = string.Empty;

        public TextualGameEditorViewModel(
            ICreateGame createGame,
            Func<IReadOnlyList<ISegment>, ILine> lineFactory,
            Func<int, ISegment> segmentFactory)
        {
            _createGame = createGame;
            _lineFactory = lineFactory;
            _segmentFactory = segmentFactory;
            _resultSource = new TaskCompletionSource<IGame>();
            Result = _resultSource.Task;

            Name = "Untitled";
        }

        public string Name
        {
            get => _name;
            set => SetIfChangedAndRaise(ref _name, value);
        }

        public string Columns
        {
            get => _columns;
            set => SetIfChangedAndRaise(ref _columns, value);
        }

        public string Rows
        {
            get => _rows;
            set => SetIfChangedAndRaise(ref _rows, value);
        }

        public Task<IGame> Result { get; }
        public void Okay() => _resultSource.SetResult(_createGame.CreateNewGame(
            Name,
            (Columns.Split(Environment.NewLine).Select(l => _lineFactory(l.Split(' ').Select(int.Parse).Select(_segmentFactory).ToList())).ToList(),
                Rows.Split(Environment.NewLine).Select(l => _lineFactory(l.Split(' ').Select(int.Parse).Select(_segmentFactory).ToList())).ToList())));

        public void Cancel() => _resultSource.SetCanceled();
    }
}