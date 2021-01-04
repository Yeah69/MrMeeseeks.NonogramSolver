using MrMeeseeks.Windows;
using System.ComponentModel;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface IViewModelLayerBase : INotifyPropertyChanged
    {
        
    }

    public class ViewModelLayerBase : ObservableObject, IViewModelLayerBase
    {
    }
}