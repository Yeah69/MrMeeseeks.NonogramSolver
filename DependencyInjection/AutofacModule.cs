using Autofac;
using MrMeeseeks.NonogramSolver.View;
using System.Reactive.Disposables;
using System.Reflection;
using AssemblyInfo = MrMeeseeks.NonogramSolver.ViewModel.AssemblyInfo;
using Module = Autofac.Module;

namespace MrMeeseeks.NonogramSolver.DependencyInjection
{
    internal class AutofacModule : Module
    {
        internal static MainWindow Start()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacModule());
            return builder
                .Build()
                .BeginLifetimeScope()
                .Resolve<MainWindow>();
        }
        
        private AutofacModule()
        {}
        
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = new[]
            {
                Assembly.Load(typeof(Persistence.AssemblyInfo).Assembly.GetName()),
                Assembly.Load(typeof(Model.AssemblyInfo).Assembly.GetName()),
                Assembly.Load(typeof(AssemblyInfo).Assembly.GetName())
            };

            builder.RegisterAssemblyTypes(assemblies)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterType<CompositeDisposable>()
                .AsSelf()
                .UsingConstructor(() => new CompositeDisposable())
                .InstancePerLifetimeScope();

            builder.RegisterType<MainWindow>()
                .AsSelf()
                .PropertiesAutowired();
        }
    }
}