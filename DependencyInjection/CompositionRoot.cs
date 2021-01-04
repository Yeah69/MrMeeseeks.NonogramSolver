using MrMeeseeks.NonogramSolver.View;

namespace MrMeeseeks.NonogramSolver.DependencyInjection
{
    public static class CompositionRoot
    {
        public static MainWindow Start() => AutofacModule.Start();
    }
}