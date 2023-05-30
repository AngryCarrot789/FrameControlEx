using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.FrameControl {
    /// <summary>
    /// A base view model for a list in which items can be added, removed, moved up/down, etc
    /// </summary>
    /// <typeparam name="T">The type of item to store</typeparam>
    public abstract class BaseListDeckViewModel<T> : BaseViewModel {
        private readonly ObservableCollectionEx<T> items;
        public ReadOnlyObservableCollection<T> Items { get; }

        // cannot be non-null due to how the ListBoxHelper deals with selected items
        // TODO: move to observable collection for selected items

        private IList<T> selectedItems = new List<T>();
        public IList<T> SelectedItems {
            get => this.selectedItems;
            set => this.RaisePropertyChanged(ref this.selectedItems, value ?? new List<T>());
        }

        private T primarySelectedItem;
        public T PrimarySelectedItem {
            get => this.primarySelectedItem;
            set {
                T oldItem = this.primarySelectedItem;
                this.RaisePropertyChanged(ref this.primarySelectedItem, value);
                this.OnPrimarySelectionChanged(oldItem, value);
            }
        }

        public bool IsEmpty => this.items.Count < 1;

        public AsyncRelayCommand AddCommand { get; }
        public AsyncRelayCommand RemoveSelectedCommand { get; }
        public RelayCommand MoveSelectedUpCommand { get; }
        public RelayCommand MoveSelectedDownCommand { get; }
        public AsyncRelayCommand ClearCommand { get; }

        protected BaseListDeckViewModel() {
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

        protected virtual void OnPrimarySelectionChanged(T oldValue, T newValue) {

        }

        // static BaseListDeckViewModel() {
        //     ActionManager.Instance.Register("actions.deck.copy", CommandActionBuilder.Of().ForType().ToAction());
        // }
//
        // private class CopyAction : AnAction {
        //     public override Task<bool> ExecuteAsync(AnActionEventArgs e) {
        //         if (e.DataContext.TryGetContext(out SceneDeckViewModel deck)) {
//
        //         }
        //     }
        // }

        protected virtual void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            this.RaisePropertyChanged(nameof(this.IsEmpty));
        }

        public virtual Task AddActionAsync() {
            return Task.CompletedTask;
        }

        public virtual Task RemoveSelectedActionAsync() {
            if (this.SelectedItems.Count > 0) {
                return this.RemoveItemsAction(this.SelectedItems);
            }

            return Task.CompletedTask;
        }

        public virtual async Task RemoveItemsAction(T item) {
            await this.RemoveItemsAction(new List<T> {item});
        }

        public virtual async Task RemoveItemsAction(IList<T> list) {
            if (list.Count < 1) {
                return;
            }

            foreach (T item in list.ToList()) {
                if (item is IDisposable) {
                    try {
                        ((IDisposable) item).Dispose();
                    }
                    catch (Exception e) {
                        await IoC.MessageDialogs.ShowMessageExAsync("Exception disposing item", $"Failed to dispose {item.GetType()} properly", e.GetToString());
                    }
                }

                this.RemoveItem(item);
            }
        }

        protected virtual void RemoveItems(IEnumerable<T> enumerable) {
            this.items.RemoveRange(enumerable);
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

            this.Clear();
        }

        public void DisposeItemsAndClear(ExceptionStack stack) {
            foreach (T source in this.Items) {
                if (source is IDisposable) {
                    try {
                        ((IDisposable) source).Dispose();
                    }
                    catch (Exception e) {
                        stack.Push(new Exception($"Failed to dispose {source}", e));
                    }
                }
            }

            this.Clear();
        }

        public virtual void MoveSelectedItems(int offset) {
            if (offset == 0 || this.SelectedItems.Count < 1) {
                return;
            }

            List<int> selection = new List<int>();
            foreach (T item in this.SelectedItems) {
                int index = this.items.IndexOf(item);
                if (index < 0) {
                    continue;
                }

                selection.Add(index);
            }

            if (offset > 0) {
                selection.Sort((a, b) => b.CompareTo(a));
            }
            else {
                selection.Sort((a, b) => a.CompareTo(b));
            }

            for (int i = 0; i < selection.Count; i++) {
                int target = selection[i] + offset;
                if (target < 0 || target >= this.items.Count || selection.Contains(target)) {
                    continue;
                }

                this.items.Move(selection[i], target);
                selection[i] = target;
            }
        }

        public virtual void MoveSelectedItemUpAction() {
            this.MoveSelectedItems(-1);
        }

        public virtual void MoveSelectedItemDownAction() {
            this.MoveSelectedItems(1);
        }

        protected virtual void OnItemAdded(T item) {

        }

        protected virtual void OnItemAddedAt(T item, int index) {
            this.OnItemAdded(item);
        }

        protected virtual void OnItemsAdded(IEnumerable<T> enumerable) {
            int index = this.items.Count;
            foreach (T item in enumerable) {
                this.OnItemAddedAt(item, index++);
            }
        }

        protected virtual void OnRemovingItem(T item) {

        }

        protected virtual void OnRemovingItemAt(T item, int index) {
            this.OnRemovingItem(item);
        }

        protected virtual void OnRemovingItems(IEnumerable<T> enumerable) {
            foreach (T item in enumerable) {
                this.OnRemovingItem(item);
            }
        }

        protected virtual void OnClearing() {
            foreach (T item in this.items) {
                this.OnRemovingItem(item);
            }
        }

        protected virtual void AddRange(IEnumerable<T> enumerable) {
            ICollection<T> list = enumerable as ICollection<T> ?? enumerable.ToList();
            this.items.AddRange(list);
            this.OnItemsAdded(list);
        }

        protected virtual void Add(T item) {
            int index = this.items.Count;
            this.items.Add(item);
            this.OnItemAddedAt(item, index);
        }

        protected virtual void Insert(int index, T item) {
            this.items.Insert(index, item);
            this.OnItemAddedAt(item, index);
        }

        protected virtual void InsertRange(int index, IEnumerable<T> enumerable) {
            ICollection<T> list = enumerable as ICollection<T> ?? enumerable.ToList();
            this.items.InsertRange(index, list);
            this.OnItemsAdded(list);
        }

        protected virtual bool Contains(T item) {
            return this.items.Contains(item);
        }

        protected virtual bool RemoveItem(T item) {
            int index = this.IndexOf(item);
            if (index < 0) {
                return false;
            }

            this.OnRemovingItemAt(item, index);
            this.items.RemoveAt(index);
            return true;
        }

        protected virtual void RemoveAll(IEnumerable<T> enumerable) {
            if (ReferenceEquals(enumerable, this.Items) || ReferenceEquals(enumerable, this.items)) {
                this.Clear();
            }
            else {
                foreach (T item in enumerable) {
                    this.RemoveItem(item);
                }
            }
        }

        protected virtual void RemoveAll(Predicate<T> canRemove) {
            // this.RemoveAll(this.items.Where(canRemove).ToList());
            for (int i = this.items.Count - 1; i >= 0; i--) {
                T item = this.items[i];
                if (canRemove(item)) {
                    this.OnRemovingItemAt(item, i);
                    this.items.RemoveAt(i);
                }
            }
        }

        protected virtual int IndexOf(T item) {
            return this.items.IndexOf(item);
        }

        protected virtual void RemoveAt(int index) {
            this.OnRemovingItemAt(this.items[index], index);
            this.items.RemoveAt(index);
        }

        protected virtual void Clear() {
            this.OnClearing();
            this.items.Clear();
        }

        protected virtual void ClearAndAdd(T item) {
            this.OnClearing();
            this.items.ClearAndAdd(item);
            this.OnItemAddedAt(item, 0);
        }

        protected virtual void ClearAndAddRange(IEnumerable<T> enumerable) {
            this.OnClearing();
            ICollection<T> list = enumerable as ICollection<T> ?? enumerable.ToList();
            this.items.ClearAndAddRange(list);
            this.OnItemsAdded(list);
        }

        public IEnumerable<T> ReverseEnumerable() {
            return this.items.ReverseEnumerable();
        }

        protected ObservableCollectionEx<T> GetBackingListUnsafe() => this.items;
    }
}