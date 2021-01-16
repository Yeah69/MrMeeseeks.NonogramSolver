namespace MrMeeseeks.NonogramSolver.Persistence
{
    public interface IGameProjectDbPath
    {
        string Value { get; }
    }

    internal class GameProjectDbPath : IGameProjectDbPath
    {
        public GameProjectDbPath(string value) => Value = value;

        public string Value { get; }
    }
}