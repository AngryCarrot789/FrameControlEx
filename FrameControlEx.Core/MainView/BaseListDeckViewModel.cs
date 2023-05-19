using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.MainView {
    /// <summary>
    /// A base view model for a list in which items can be added, removed, moved up/down, etc
    /// </summary>
    /// <typeparam name="T">The type of item to store</typeparam>
    public abstract class BaseListDeckViewModel<T> : BaseViewModel {
        protected readonly ObservableCollectionEx<T> items;
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
            this.Items = new ReadOnlyObservableCollection<T>(this.items);
            this.AddCommand = new AsyncRelayCommand(this.AddActionAsync);
            this.RemoveSelectedCommand = new AsyncRelayCommand(this.RemoveSelectedActionAsync);
            this.MoveSelectedUpCommand = new RelayCommand(this.MoveSelectedItemUpAction);
            this.MoveSelectedDownCommand = new RelayCommand(this.MoveSelectedItemDownAction);
            this.ClearCommand = new AsyncRelayCommand(this.ClearActionAsync);
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
    }
}