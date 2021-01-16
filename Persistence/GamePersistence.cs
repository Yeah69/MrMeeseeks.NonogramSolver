using LiteDB;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System;
using System.Collections.Generic;

namespace MrMeeseeks.NonogramSolver.Persistence
{
    internal class GamePersistence : Game
    {
        private readonly ObjectId _id;
        private readonly IGameRepository _gameRepository;

        public GamePersistence(
            // parameters
            ObjectId id,
            string name, 
            (IReadOnlyList<ILine>, IReadOnlyList<ILine>) columnsAndRows, 
            
            // dependencies
            Func<(int, int), ICell> cellFactory,
            IGameRepository gameRepository) : base(name, columnsAndRows, cellFactory)
        {
            _id = id;
            _gameRepository = gameRepository;
        }

        public override void Save() =>  _gameRepository.Save(_id, this);

        public override void Delete() => _gameRepository.Delete(_id);
    }
}