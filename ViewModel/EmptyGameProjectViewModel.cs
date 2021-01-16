namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface IEmptyGameProjectViewModel : IGameProjectViewModelBase
    {
        
    }

    internal class EmptyGameProjectViewModel : ViewModelLayerBase, IEmptyGameProjectViewModel
    {
        public bool IsGamesVisible => false;
        public bool IsEditorVisible => false;
        public void Solve()
        {
        }

        public void Build()
        {
        }
    }
}