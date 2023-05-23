using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.MainView {
    /// <summary>
    /// A base view model for a list in which items can be added, removed, moved up/down, etc
    /// </summary>
    /// <typeparam name="T">The type of item to store</typeparam>
    public abstract class BaseListDeckViewModel<T> : BaseViewModel {
        private readonly ObservableCollectionEx<T> items;
        public ReadOnlyObservableCollection<T> Items { get; }

        private T selectedItem;
        public T SelectedItem {
            get => this.selectedItem;
            set => this.RaisePropertyChanged(ref this.selectedItem, value);
        }

        public AsyncRelayCommand AddCommand { get; }
        public AsyncRelayCommand RemoveSelectedCommand { get; }
        public RelayCommand MoveSelectedUpCommand { get; }
        public RelayCommand MoveSelectedDownCommand { get; }
        public AsyncRelayCommand ClearCommand { get; }

        public BaseListDeckViewModel() {
            this.items = new ObservableCollectionEx<T>();
            // add before creating Items so that we get called first before UI stuff; probably better?
            this.items.CollectionChanged += this.OnItemCollectionChanged;
            this.Items = new ReadOnlyObservableCollection<T>(this.items);
            this.AddCommand = new AsyncRelayCommand(this.AddActionAsync);
            this.RemoveSelectedCommand = new AsyncRelayCommand(this.RemoveSelectedActionAsync);
            this.MoveSelectedUpCommand = new RelayCommand(this.MoveSelectedItemUpAction);
            this.MoveSelectedDownCommand = new RelayCommand(this.MoveSelectedItemDownAction);
            this.ClearCommand = new AsyncRelayCommand(this.ClearActionAsync);
        }

        protected virtual void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {

        }

        public virtual Task AddActionAsync() {
            return Task.CompletedTask;
        }

        public virtual Task RemoveSelectedActionAsync() {
            if (this.selectedItem != null) {
                return this.RemoveItemAction(this.selectedItem);
            }

            return Task.CompletedTask;
        }

        public virtual async Task RemoveItemAction(T item) {
            int index = this.items.IndexOf(item);
            if (index >= 0) {
                if (item is IDisposable) {
                    try {
                        ((IDisposable) item).Dispose();
                    }
                    catch (Exception e) {
                        await IoC.MessageDialogs.ShowMessageExAsync("Exception disposing image", $"Failed to dispose {item.GetType()} properly. This error can be ignored", e.ToString());
                    }
                    finally {
                        this.items.Remove(item);
                    }
                }
                else {
                    this.items.Remove(item);
                }

                int count = this.items.Count;
                if (count > 0) {
                    this.SelectedItem = this.items[Math.Min(index, count - 1)];
                }
            }
        }

        public virtual async Task ClearActionAsync() {
            List<(T, Exception)> disposureFailure = new List<(T, Exception)>();
            this.DisposeAllAndClear(disposureFailure);
            if (disposureFailure.Count > 0) {
                await IoC.MessageDialogs.ShowMessageExAsync(
                    "Exception disposing items",
                    "An exception occurred while disposing one or more items",
                    string.Join("\n\n", disposureFailure.Select(x => x.Item1.GetType() + " -> " + x.Item2)));
            }
        }

        public void DisposeAllAndClear(List<(T, Exception)> exceptions) {
            try {
                foreach (T item in this.items) {
                    if (item is IDisposable disposable) {
                        try {
                            disposable.Dispose();
                        }
                        catch (Exception e) {
                            exceptions.Add((item, e));
                        }
                    }
                }
            }
            finally {
                this.items.Clear();
            }
        }

        public virtual void MoveSelectedItemUpAction() {
            int index;
            if (this.selectedItem == null || (index = this.items.IndexOf(this.selectedItem)) < 0) {
                return;
            }

            if (index > 0) {
                this.items.Move(index, index - 1);
            }
        }

        public virtual void MoveSelectedItemDownAction() {
            int index;
            if (this.selectedItem == null || (index = this.items.IndexOf(this.selectedItem)) < 0) {
                return;
            }

            if ((index + 1) < this.items.Count) {
                this.items.Move(index, index + 1);
            }
        }

        protected virtual void EnsureItem(T item, bool valid) {

        }

        protected virtual void EnsureItems(IEnumerable<T> enumerable, bool valid) {
            foreach (T item in enumerable) {
                this.EnsureItem(item, valid);
            }
        }

        protected virtual void AddRange(IEnumerable<T> enumerable) {
            List<T> list = enumerable.ToList();
            this.items.AddRange(list);
            this.EnsureItems(list, true);
        }

        protected virtual void Add(T item) {
            this.items.Add(item);
            this.EnsureItem(item, true);
        }

        protected virtual void Insert(int index, T item) {
            this.items.Insert(index, item);
            this.EnsureItem(item, true);
        }

        protected virtual void InsertRange(int index, IEnumerable<T> enumerable) {
            List<T> list = enumerable.ToList();
            this.items.InsertRange(index, list);
            this.EnsureItems(list, true);
        }

        protected virtual bool Contains(T item) {
            return this.items.Contains(item);
        }

        protected virtual bool Remove(T item) {
            int index = this.IndexOf(item);
            if (index < 0) {
                return false;
            }

            this.RemoveAt(index);
            return true;
        }

        protected virtual void RemoveAll(IEnumerable<T> enumerable) {
            foreach (T item in enumerable) {
                this.Remove(item);
            }
        }

        protected virtual void RemoveAll(Predicate<T> canRemove) {
            // this.RemoveAll(this.items.Where(canRemove).ToList());
            for (int i = this.items.Count - 1; i >= 0; i--) {
                T item = this.items[i];
                if (canRemove(item)) {
                    this.EnsureItem(item, false);
                    this.items.RemoveAt(i);
                }
            }
        }

        protected virtual int IndexOf(T item) {
            return this.items.IndexOf(item);
        }

        protected virtual void RemoveAt(int index) {
            this.EnsureItem(this.items[index], false);
            this.items.RemoveAt(index);
        }

        protected virtual void Clear() {
            this.EnsureItems(this.items, false);
            this.items.Clear();
        }

        protected virtual void ClearAndAdd(T item) {
            this.EnsureItems(this.items, false);
            this.items.ClearAndAdd(item);
        }

        protected virtual void ClearAndAddRange(IEnumerable<T> enumerable) {
            this.EnsureItems(this.items, false);
            this.items.ClearAndAddRange(enumerable);
        }
    }
}