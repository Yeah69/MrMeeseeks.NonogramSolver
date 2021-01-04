using MrMeeseeks.NonogramSolver.Model;
using System;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface IMainWindowViewModel : IViewModelLayerBase
    {
        IViewModelLayerBase Content { get; }
        ICommand Build { get; }
        ICommand Save { get; }
        ICommand Load { get; }
        ICommand Solve { get; }
    }

    internal class MainWindowViewModel : ViewModelLayerBase, IMainWindowViewModel
    {
        private IViewModelLayerBase _content;

        public MainWindowViewModel(
            // parameters
            IGameEditorViewModel gameEditorViewModel,
            
            // dependencies
            ILoadGame loadGame,
            Func<IGame, IGameViewModel> gameViewModelFactory,
            CompositeDisposable compositeDisposable)
        {
            _content = gameEditorViewModel;

            var build = RxCommand
                .CallerDeterminedCanExecute(
                    this.ObservePropertyChanged(nameof(Content))
                        .Select(_ => Content is IGameEditorViewModel),
                    true)
                .CompositeDisposalWith(compositeDisposable);
            build
                .Observe
                .Subscribe(_ =>
                {
                    if (Content is IGameEditorViewModel gevm)
                        Content = gevm.BuildGame();
                })
                .CompositeDisposalWith(compositeDisposable);
            Build = build;

            var save = RxCommand
                .CallerDeterminedCanExecute(
                    this.ObservePropertyChanged(nameof(Content))
                        .Select(_ => Content is IGameViewModel),
                    false)
                .CompositeDisposalWith(compositeDisposable);
            save
                .Observe
                .Subscribe(_ =>
                {
                    if (Content is IGameViewModel gvm)
                        gvm.SaveGame();
                })
                .CompositeDisposalWith(compositeDisposable);
            Save = save;

            var load = RxCommand
                .CanAlwaysExecute()
                .CompositeDisposalWith(compositeDisposable);
            load
                .Observe
                .Subscribe(_ => 
                    Content = gameViewModelFactory(
                        loadGame.Load(
                            Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                "MrMeeseeksNonogramSolver",
                                "SaveGame.json"))))
                .CompositeDisposalWith(compositeDisposable);
            Load = load;

            var solve = RxCommand
                .CallerDeterminedCanExecute(
                    this.ObservePropertyChanged(nameof(Content))
                        .Select(_ => Content is IGameViewModel),
                    false)
                .CompositeDisposalWith(compositeDisposable);
            solve
                .Observe
                .Subscribe(_ =>
                {
                    if (Content is IGameViewModel gvm)
                        gvm.Solve();
                })
                .CompositeDisposalWith(compositeDisposable);
            Solve = solve;
        }

        public IViewModelLayerBase Content
        {
            get => _content;
            private set => SetIfChangedAndRaise(ref _content, value);
        }
        
        public ICommand Build { get; }
        
        public ICommand Save { get; }
        
        public ICommand Load { get; }

        public ICommand Solve { get; }
    }
}