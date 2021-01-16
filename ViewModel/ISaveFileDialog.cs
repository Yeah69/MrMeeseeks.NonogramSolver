using System.Collections.Generic;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface ISaveFileDialog
    {
        Task<string> Show(
            string title, 
            string directory, 
            string initialFileName, 
            string defaultExtension,
            params (string Name, IReadOnlyList<string> Extensions)[] filters);
    }

    public interface IMainWindowSaveFileDialog : ISaveFileDialog
    {
    }
}