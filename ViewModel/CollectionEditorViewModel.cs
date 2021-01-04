using DynamicData;
using DynamicData.Binding;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface ICollectionEditorViewModel<TViewModel>
    {
        ReadOnlyObservableCollection<TViewModel> Collection { get; }
        ICommand Add { get; }
        ICommand Remove { get; }
    }

    internal class CollectionEditorViewModel<TModel, TViewModel> : ICollectionEditorViewModel<TViewModel>
    {

        public CollectionEditorViewModel(
            // parameters
            ReadOnlyObservableCollection<TModel> modelCollection,
            Func<TModel, TViewModel> viewModelFactory,
            Action addAction,
            Action<TViewModel> removeAction,
            
            // dependencies
            CompositeDisposable compositeDisposable)
        {
            modelCollection
                .ToObservableChangeSet()
                .Transform(viewModelFactory)
                .Bind(out ReadOnlyObservableCollection<TViewModel> collection)
                .Subscribe()
                .CompositeDisposalWith(compositeDisposable);
            Collection = collection;
            
            var addSegment = RxCommand
                .CanAlwaysExecute()
                .CompositeDisposalWith(compositeDisposable);
            addSegment
                .Observe
                .Subscribe(_ => addAction())
                .CompositeDisposalWith(compositeDisposable);
            Add = addSegment;
            
            var removeSegment = RxCommand
                .CanAlwaysExecute()
                .CompositeDisposalWith(compositeDisposable);
            removeSegment
                .ObserveOfType<TViewModel>()
                .Subscribe(removeAction)
                .CompositeDisposalWith(compositeDisposable);
            Remove = removeSegment;
        }

        public ReadOnlyObservableCollection<TViewModel> Collection { get; }
        
        public ICommand Add { get; }
        
        public ICommand Remove { get; }
    }
}