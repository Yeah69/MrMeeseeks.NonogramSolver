using MrMeeseeks.Extensions;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.Model.Game.Editing
{
    public interface IGameEditor : IModelLayerBase
    {
        string Name { get; set; }
        ReadOnlyObservableCollection<ILineEditor> Columns { get; }
        ReadOnlyObservableCollection<ILineEditor> Rows { get; }
        void AddColumn();
        void AddRow();
        void RemoveColumn(ILineEditor column);
        void RemoveRow(ILineEditor row);
        IGame Build();
    }

    internal class GameEditor : ModelLayerBase, IGameEditor
    {
        private readonly Func<ILineEditor> _lineEditorFactory;
        private readonly ICreateGame _createGame;
        private readonly ObservableCollection<ILineEditor> _columns = new();
        private readonly ObservableCollection<ILineEditor> _rows = new();
        private string _name = "Untitled";

        public GameEditor(
            Func<ILineEditor> lineEditorFactory,
            ICreateGame createGame)
        {
            _lineEditorFactory = lineEditorFactory;
            _createGame = createGame;
            Columns = new ReadOnlyObservableCollection<ILineEditor>(_columns);
            Rows = new ReadOnlyObservableCollection<ILineEditor>(_rows);
        }

        public string Name
        {
            get => _name;
            set => this.SetIfChangedAndRaise(ref _name, value);
        }

        public ReadOnlyObservableCollection<ILineEditor> Columns { get; }

        public ReadOnlyObservableCollection<ILineEditor> Rows { get; }

        public void AddColumn() => _columns.Add(_lineEditorFactory());

        public void AddRow() => _rows.Add(_lineEditorFactory());

        public void RemoveColumn(ILineEditor column) => _columns.Remove(column);
        
        public void RemoveRow(ILineEditor row) => _rows.Remove(row);
        
        public IGame Build() => 
            _createGame.CreateNewGame(
                Name,
                (_columns.Select(l => l.Build()).ToReadOnlyList(), 
                _rows.Select(l => l.Build()).ToReadOnlyList()));
    }
}