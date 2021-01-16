using MrMeeseeks.NonogramSolver.Model.Game.Solving;
using MrMeeseeks.NonogramSolver.Model.PersistenceHook;
using System.Collections.ObjectModel;

namespace MrMeeseeks.NonogramSolver.Model.Game
{
    public interface IGameProject
    {
        string Name { get; }
        ReadOnlyObservableCollection<IGame> Games { get; }
        void Add(IGame game);
        void Delete(IGame game);
    }

    internal class GameProject : IGameProject
    {
        private readonly ObservableCollection<IGame> _games;

        public GameProject(
            // parameters
            string name,
            
            // dependencies
            ILoadGames loadGames)
        {
            Name = name;
            _games = new ObservableCollection<IGame>(loadGames.Load());
            Games = new ReadOnlyObservableCollection<IGame>(_games);
        }
        
        public string Name { get; }
        
        public ReadOnlyObservableCollection<IGame> Games { get; }
        
        public void Add(IGame game) => _games.Add(game);
        public void Delete(IGame game)
        {
            _games.Remove(game);
            game.Delete();
        }
    }
}