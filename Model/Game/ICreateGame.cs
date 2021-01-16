using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using System.Collections.Generic;

namespace MrMeeseeks.NonogramSolver.Model.Game
{
    public interface ICreateGame
    {
        IGame CreateNewGame(
            string name,
            (IReadOnlyList<ILine>, IReadOnlyList<ILine>) columnsAndRows);
    }
}