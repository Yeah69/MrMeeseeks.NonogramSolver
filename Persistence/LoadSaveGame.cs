using LiteDB;
using MrMeeseeks.Extensions;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using MrMeeseeks.NonogramSolver.Model.PersistenceHook;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MrMeeseeks.NonogramSolver.Persistence
{
    internal class GameRepositoryRepository : ILoadGames, IGameRepository
    {
        private readonly string _fileName;
        private readonly Func<ObjectId, string, (IReadOnlyList<ILine>, IReadOnlyList<ILine>), GamePersistence> _gameFactory;
        private readonly Func<IReadOnlyList<ISegment>, ILine> _lineFactory;
        private readonly Func<int, ISegment> _segmentFactory;

        private class Segment
        {
            public int Length { get; set; }
        }

        private class Line
        {
            public Line()
            {
                Segments = new List<Segment>();
            }
            
            public IReadOnlyList<Segment> Segments { get; set; }
        }

        private class Game
        {
            public Game()
            {
                GameId = ObjectId.NewObjectId();
                Name = String.Empty;
                Columns = new List<Line>();
                Rows = new List<Line>();
            }

            public ObjectId GameId { get; set; }
            public string Name { get; set; }
            public IReadOnlyList<Line> Columns { get; set; }
            public IReadOnlyList<Line> Rows { get; set; }

            public void Deconstruct(out ObjectId gameId, out string name, out IReadOnlyList<Line> columns, out IReadOnlyList<Line> rows)
            {
                gameId = GameId;
                name = Name;
                columns = Columns;
                rows = Rows;
            }
        }

        public GameRepositoryRepository(
            IGameProjectDbPath gameProjectDbPath,
            Func<ObjectId, string, (IReadOnlyList<ILine>, IReadOnlyList<ILine>), GamePersistence> gameFactory,
            Func<IReadOnlyList<ISegment>, ILine> lineFactory,
            Func<int, ISegment> segmentFactory)
        {
            _fileName = gameProjectDbPath.Value;
            _gameFactory = gameFactory;
            _lineFactory = lineFactory;
            _segmentFactory = segmentFactory;
        }

        public IReadOnlyList<IGame> Load()
        {
            using var db = new LiteDatabase(_fileName);
            return db.GetCollection<Game>().FindAll()
                .Select(gameDto =>
                {
                    var (objectId, name, columns, rows) = gameDto;
                    return _gameFactory(
                        objectId,
                        name,
                        (columns
                                .Select(l =>
                                    _lineFactory(l.Segments.Select(s => _segmentFactory(s.Length)).ToReadOnlyList()))
                                .ToReadOnlyList(),
                            rows
                                .Select(l =>
                                    _lineFactory(l.Segments.Select(s => _segmentFactory(s.Length)).ToReadOnlyList()))
                                .ToReadOnlyList()));
                })
                .ToReadOnlyList();
        }

        public void Save(ObjectId id, IGame game)
        {
            var gameDto = new Game
            {
                GameId = id, 
                Name = game.Name,
                Columns = game
                    .Columns
                    .Select(l => new Line { Segments = l.Segments.Select(s => new Segment {Length = s.Length}).ToReadOnlyList() })
                    .ToReadOnlyList(),
                Rows = game
                    .Rows
                    .Select(l => new Line { Segments = l.Segments.Select(s => new Segment {Length = s.Length}).ToReadOnlyList() })
                    .ToReadOnlyList()
            };

            using var db = new LiteDatabase(_fileName);
            db.GetCollection<Game>().Insert(gameDto);
        }

        public void Delete(ObjectId id)
        {
            using var db = new LiteDatabase(_fileName);
            db.GetCollection<Game>().Delete(id);
        }
    }
}