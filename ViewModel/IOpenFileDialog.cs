using System.Collections.Generic;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface IOpenFileDialog
    {
        Task<string[]> Show(
            string title, 
            string directory, 
            string initialFileName, 
            bool allowMultiple,
            params (string Name, IReadOnlyList<string> Extensions)[] filters);
    }

    public interface IMainWindowOpenFileDialog : IOpenFileDialog
    {
    }
}