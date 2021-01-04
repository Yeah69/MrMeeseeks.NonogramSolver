using MrMeeseeks.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.Model
{
    public interface IGameEditor : IModelLayerBase
    {
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
        private readonly IVerticalCellIterator _verticalCellIterator;
        private readonly IHorizontalCellIterator _horizontalCellIterator;
        private readonly Func<ICellIterator, ILineEditor> _lineEditorFactory;
        private readonly Func<(IReadOnlyList<ILine>, IReadOnlyList<ILine>), IGame> _gameFactory;
        private readonly ObservableCollection<ILineEditor> _columns = new();
        private readonly ObservableCollection<ILineEditor> _rows = new();
        
        public GameEditor(
            IVerticalCellIterator verticalCellIterator,
            IHorizontalCellIterator horizontalCellIterator,
            Func<ICellIterator, ILineEditor> lineEditorFactory,
            Func<(IReadOnlyList<ILine>, IReadOnlyList<ILine>), IGame> gameFactory)
        {
            _verticalCellIterator = verticalCellIterator;
            _horizontalCellIterator = horizontalCellIterator;
            _lineEditorFactory = lineEditorFactory;
            _gameFactory = gameFactory;
            Columns = new ReadOnlyObservableCollection<ILineEditor>(_columns);
            Rows = new ReadOnlyObservableCollection<ILineEditor>(_rows);
        }
        
        public ReadOnlyObservableCollection<ILineEditor> Columns { get; }

        public ReadOnlyObservableCollection<ILineEditor> Rows { get; }

        public void AddColumn() => _columns.Add(_lineEditorFactory(_verticalCellIterator));

        public void AddRow() => _rows.Add(_lineEditorFactory(_horizontalCellIterator));

        public void RemoveColumn(ILineEditor column) => _columns.Remove(column);
        
        public void RemoveRow(ILineEditor row) => _rows.Remove(row);
        
        public IGame Build() => 
            _gameFactory(
                (_columns.Select(l => l.Build()).ToReadOnlyList(), 
                _rows.Select(l => l.Build()).ToReadOnlyList()));
    }
}