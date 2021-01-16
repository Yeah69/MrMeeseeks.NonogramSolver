using LiteDB;
using MrMeeseeks.NonogramSolver.Model.Game;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System;
using System.Collections.Generic;

namespace MrMeeseeks.NonogramSolver.Persistence
{
    internal class CreateGame : ICreateGame
    {
        private readonly Func<ObjectId, string, (IReadOnlyList<ILine>, IReadOnlyList<ILine>), IGame> _gameFactory;

        public CreateGame(Func<ObjectId, string, (IReadOnlyList<ILine>, IReadOnlyList<ILine>), IGame> gameFactory) => 
            _gameFactory = gameFactory;

        public IGame CreateNewGame(string name, (IReadOnlyList<ILine>, IReadOnlyList<ILine>) columnsAndRows) => 
            _gameFactory(ObjectId.NewObjectId(), name, columnsAndRows);
    }
}