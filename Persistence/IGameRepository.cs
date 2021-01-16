using LiteDB;
using MrMeeseeks.NonogramSolver.Model.Game.Solving;

namespace MrMeeseeks.NonogramSolver.Persistence
{
    internal interface IGameRepository
    {
        void Save(ObjectId id, IGame game);
        void Delete(ObjectId id);
    }
}