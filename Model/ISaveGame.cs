namespace MrMeeseeks.NonogramSolver.Model
{
    public interface ISaveGame
    {
        void Save(IGame game, string filePath);
    }
    
    public interface ILoadGame
    {
        IGame Load(string filePath);
    }
}