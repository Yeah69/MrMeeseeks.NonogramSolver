using Autofac;
using MrMeeseeks.NonogramSolver.Model.Game;
using MrMeeseeks.NonogramSolver.Model.Settings;
using MrMeeseeks.NonogramSolver.Persistence;
using MrMeeseeks.NonogramSolver.View;
using MrMeeseeks.NonogramSolver.ViewModel;
using System;
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
            
            // Persistence
            builder.RegisterType<GameRepositoryRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<GameProjectDbPath>()
                .As<IGameProjectDbPath>()
                .InstancePerLifetimeScope();
            
            // Model
            builder.RegisterType<CurrentSettings>()
                .As<ICurrentSettings>()
                .SingleInstance();

            builder.RegisterType<GameProject>()
                .As<IGameProject>()
                .InstancePerLifetimeScope();
            
            
            // ViewModel
            builder.RegisterType<EmptyGameProjectViewModel>()
                .As<IEmptyGameProjectViewModel>()
                .SingleInstance();

            builder.Register<Func<string, IGameProjectViewModel>>(c =>
            {
                var currentLifetimeScope = c.Resolve<ILifetimeScope>();
                return filePath =>
                {
                    ILifetimeScope beginLifetimeScope = currentLifetimeScope.BeginLifetimeScope();
                    beginLifetimeScope.Resolve<IGameProjectDbPath>(TypedParameter.From(filePath));
                    var gameProject = beginLifetimeScope.Resolve<IGameProject>(TypedParameter.From(filePath));
                    return beginLifetimeScope.Resolve<IGameProjectViewModel>(TypedParameter.From(gameProject));
                };
            });

            
            // View
            var uiTypes = new[]
            {
                typeof(GameEditorView)
            };

            builder.RegisterTypes(uiTypes)
                .AsSelf()
                .AsImplementedInterfaces()
                .PropertiesAutowired();
            
            builder.RegisterType<MainWindow>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance()
                .PropertiesAutowired();
            
            builder.RegisterType<MainWindowOpenFileDialogWrapper>()
                .AsImplementedInterfaces();
            
            builder.RegisterType<MainWindowSaveFileDialogWrapper>()
                .AsImplementedInterfaces();
        }
    }
}