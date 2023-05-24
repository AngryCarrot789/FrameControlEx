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
        private static List<object> DeckClipboard;

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

                this.items.Remove(item);
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

            this.items.Clear();
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

        public IEnumerable<T> ReverseEnumerable() {
            return this.items.ReverseEnumerable();
        }
    }
}