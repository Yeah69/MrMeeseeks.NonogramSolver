using MrMeeseeks.NonogramSolver.Model.PersistenceHook;

namespace MrMeeseeks.NonogramSolver.Model.Settings
{
    public interface ICurrentSettings
    {
        ISettings Value { get; }
    }

    internal class CurrentSettings : ICurrentSettings
    {
        public CurrentSettings(ILoadSettings loadSettings) => Value = loadSettings.Load();

        public ISettings Value { get; }
    }
}