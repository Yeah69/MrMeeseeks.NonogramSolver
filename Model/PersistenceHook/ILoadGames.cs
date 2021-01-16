using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System.Collections.Generic;

namespace MrMeeseeks.NonogramSolver.Model.PersistenceHook
{ 
    public interface ILoadGames
    {
        IReadOnlyList<IGame> Load();
    }
}