using MrMeeseeks.NonogramSolver.Model.Settings;

namespace MrMeeseeks.NonogramSolver.Model.PersistenceHook
{
    public interface ISaveSettings
    {
        void Save(ISettings settings);
    }
    
    public interface ILoadSettings
    {
        ISettings Load();
    }
}