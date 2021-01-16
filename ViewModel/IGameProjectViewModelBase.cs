namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface IGameProjectViewModelBase : IViewModelLayerBase
    {
        bool IsGamesVisible { get; }
        bool IsEditorVisible { get; }
        void Solve();
        void Build();
    }
}