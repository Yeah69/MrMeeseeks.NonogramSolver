using DynamicData;
using DynamicData.Binding;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

namespace MrMeeseeks.NonogramSolver.ViewModel
{
    public interface ICollectionEditorViewModel<TViewModel>
    {
        ReadOnlyObservableCollection<TViewModel> Collection { get; }
        void Add();
        void Remove(TViewModel viewModel);
    }

    internal class CollectionEditorViewModel<TModel, TViewModel> : ICollectionEditorViewModel<TViewModel>
    {
        private readonly Action _addAction;
        private readonly Action<TViewModel> _removeAction;

        public CollectionEditorViewModel(
            // parameters
            ReadOnlyObservableCollection<TModel> modelCollection,
            Func<TModel, TViewModel> viewModelFactory,
            Action addAction,
            Action<TViewModel> removeAction,
            
            // dependencies
            CompositeDisposable compositeDisposable)
        {
            _addAction = addAction;
            _removeAction = removeAction;
            modelCollection
                .ToObservableChangeSet()
                .Transform(viewModelFactory)
                .Bind(out ReadOnlyObservableCollection<TViewModel> collection)
                .Subscribe()
                .CompositeDisposalWith(compositeDisposable);
            Collection = collection;
        }

        public ReadOnlyObservableCollection<TViewModel> Collection { get; }

        public void Add() => _addAction();

        public void Remove(TViewModel viewModel) => _removeAction(viewModel);
    }
}