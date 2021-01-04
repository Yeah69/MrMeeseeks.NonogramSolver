using MrMeeseeks.Windows;
using System.ComponentModel;

namespace MrMeeseeks.NonogramSolver.Model
{
    public interface IModelLayerBase : INotifyPropertyChanged
    {
        
    }

    internal class ModelLayerBase : ObservableObject, IModelLayerBase
    {
    }
}