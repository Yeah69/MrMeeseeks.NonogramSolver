using MrMeeseeks.Extensions;
using MrMeeseeks.NonogramSolver.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.Persistence
{
    internal class LoadSaveGame : ILoadGame, ISaveGame
    {
        private readonly IVerticalCellIterator _verticalCellIterator;
        private readonly IHorizontalCellIterator _horizontalCellIterator;
        private readonly Func<(IReadOnlyList<ILine>, IReadOnlyList<ILine>), IGame> _gameFactory;
        private readonly Func<IReadOnlyList<ISegment>, ICellIterator, ILine> _lineFactory;
        private readonly Func<int, ISegment> _segmentFactory;

        private record Segment(int Length);

        private record Line(IReadOnlyList<Segment> Segments);

        private record Game(IReadOnlyList<Line> Columns, IReadOnlyList<Line> Rows);

        public LoadSaveGame(
            IVerticalCellIterator verticalCellIterator,
            IHorizontalCellIterator horizontalCellIterator,
            Func<(IReadOnlyList<ILine>, IReadOnlyList<ILine>), IGame> gameFactory,
            Func<IReadOnlyList<ISegment>, ICellIterator, ILine> lineFactory,
            Func<int, ISegment> segmentFactory)
        {
            _verticalCellIterator = verticalCellIterator;
            _horizontalCellIterator = horizontalCellIterator;
            _gameFactory = gameFactory;
            _lineFactory = lineFactory;
            _segmentFactory = segmentFactory;
        }
        
        public void Save(IGame game, string filePath)
        {
            var gameDto = new Game(
                game
                    .Columns
                    .Select(l => new Line(l.Segments.Select(s => new Segment(s.Length)).ToReadOnlyList()))
                    .ToReadOnlyList(),
                game
                    .Rows
                    .Select(l => new Line(l.Segments.Select(s => new Segment(s.Length)).ToReadOnlyList()))
                    .ToReadOnlyList());

            FileInfo fileInfo = new (filePath);
            if (fileInfo.Directory?.Exists.Not() ?? true)
                Directory.CreateDirectory(fileInfo.DirectoryName ?? throw new Exception());

            File.WriteAllText(filePath, JsonConvert.SerializeObject(gameDto));
        }

        public IGame Load(string filePath)
        {
            var (columns, rows) = JsonConvert.DeserializeObject<Game>(File.ReadAllText(filePath));
            return _gameFactory((
                columns
                    .Select(l => _lineFactory(l.Segments.Select(s => _segmentFactory(s.Length)).ToReadOnlyList(), _verticalCellIterator))
                    .ToReadOnlyList(),
                rows
                    .Select(l => _lineFactory(l.Segments.Select(s => _segmentFactory(s.Length)).ToReadOnlyList(), _horizontalCellIterator))
                    .ToReadOnlyList()));
        }
    }
}