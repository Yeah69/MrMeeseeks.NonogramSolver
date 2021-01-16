using MrMeeseeks.Windows;

namespace MrMeeseeks.NonogramSolver.Model
{
    public interface IModelLayerBase : IObservableObject
    {
        
    }

    internal class ModelLayerBase : ObservableObject, IModelLayerBase
    {
    }
}