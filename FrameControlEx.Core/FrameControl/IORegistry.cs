using System;
using System.Collections.Generic;
using FrameControlEx.Core.FrameControl.Models.Scene;
using FrameControlEx.Core.FrameControl.Models.Scene.Outputs;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources;
using FrameControlEx.Core.FrameControl.ViewModels.Scene;
using FrameControlEx.Core.FrameControl.ViewModels.Scene.Outputs;
using FrameControlEx.Core.FrameControl.ViewModels.Scene.Sources;

namespace FrameControlEx.Core.FrameControl {
    public static class IORegistry {
        private static readonly Dictionary<string, Entry> Registry;
        private static readonly Dictionary<Type, Entry> ViewModelToRegistry;
        private static readonly Dictionary<Type, Entry> ModelToRegistry;

        static IORegistry() {
            Registry = new Dictionary<string, Entry>();
            ViewModelToRegistry = new Dictionary<Type, Entry>();
            ModelToRegistry = new Dictionary<Type, Entry>();
        }

        public static void RegisterStandard() {
            // Sources
            Register<ImageSourceModel, ImageSourceViewModel>("image_source");
            Register<LoopbackSourceModel, LoopbackSourceViewModel>("loopback_source");
            Register<MMFSourceModel, MMFAVSourceViewModel>("mmf_source");
            Register<SceneSourceModel, SceneSourceViewModel>("scene_source");

            // Outputs
            Register<BufferedOutputModel, BufferedOutputViewModel>("buffered_output");
            Register<MMFOutputModel, MMFAVOutputViewModel>("mmf_output");
        }

        public static void Register<TModel, TViewModel>(string id) where TModel : BaseIOModel where TViewModel : BaseIOViewModel {
            ValidateId(id);
            RegisterInternal<TModel, TViewModel>(id, null, null);
        }

        public static void Register<TModel, TViewModel>(string id, Func<TModel> modelFunc, Func<TModel, TViewModel> viewModelFunc) where TModel : BaseIOModel where TViewModel : BaseIOViewModel{
            ValidateId(id);
            RegisterInternal<TModel, TViewModel>(id, modelFunc, (m) => viewModelFunc((TModel) m));
        }

        private static void ValidateId(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            if (Registry.ContainsKey(id))
                throw new Exception($"A registration already exists with the id {id}");
        }

        private static void RegisterInternal<TModel, TViewModel>(string id, Func<BaseIOModel> modelFunc, Func<BaseIOModel, BaseIOViewModel> viewModelFunc) where TModel : BaseIOModel where TViewModel : BaseIOViewModel{
            AddEntry(new Entry(id, typeof(TModel), typeof(TViewModel), modelFunc, viewModelFunc));
        }

        public static BaseIOModel CreateModel(string id) {
            return GetEntryInternal(id).CreateModel();
        }

        public static BaseIOViewModel CreateViewModel(string id) {
            return GetEntryInternal(id).CreateViewModel();
        }

        public static BaseIOViewModel CreateViewModelFromModel(BaseIOModel model) {
            if (ModelToRegistry.TryGetValue(model.GetType(), out var entry)) {
                return entry.CreateViewModel(model);
            }

            throw new Exception($"No such registration for model type: {model.GetType()}");
        }

        public static string GetTypeId(BaseIOModel model) {
            return GetTypeIdForModel(model.GetType());
        }

        public static string GetTypeId(BaseIOViewModel model) {
            return GetTypeIdForViewModel(model.GetType());
        }

        public static string GetTypeIdForModel(Type modelType) {
            return ModelToRegistry.TryGetValue(modelType, out Entry entry) ? entry.Id : null;
        }

        public static string GetTypeIdForViewModel(Type viewModelType) {
            return ViewModelToRegistry.TryGetValue(viewModelType, out Entry entry) ? entry.Id : null;
        }

        private static void AddEntry(Entry entry) {
            Registry[entry.Id] = entry;
            ModelToRegistry[entry.ModelType] = entry;
            ViewModelToRegistry[entry.ViewModelType] = entry;
        }

        private static Entry GetEntryInternal(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            if (!Registry.TryGetValue(id, out var entry))
                throw new Exception($"No such registration with id: {id}");
            return entry;
        }

        private class Entry {
            public readonly string Id;
            public readonly Type ModelType;
            public readonly Type ViewModelType;
            public readonly Func<BaseIOModel> ModelFunc;
            public readonly Func<BaseIOModel, BaseIOViewModel> ViewModelFunc;

            public Entry(string id, Type modelType, Type viewModelType) {
                this.Id = id;
                this.ModelType = modelType;
                this.ViewModelType = viewModelType;
            }

            public Entry(string id, Type modelType, Type viewModelType, Func<BaseIOModel> modelFunc, Func<BaseIOModel, BaseIOViewModel> viewModelFunc) : this(id, modelType, viewModelType) {
                this.ModelFunc = modelFunc;
                this.ViewModelFunc = viewModelFunc;
            }

            public static Entry FromFunctions<TModel, TViewModel>(string id, Func<TModel> modelFunc, Func<TModel, TViewModel> viewModelFunc) where TModel : BaseIOModel where TViewModel : BaseIOViewModel {
                return new Entry(id, typeof(TModel), typeof(TViewModel), modelFunc, (m) => viewModelFunc((TModel) m));
            }

            public static Entry FromFunctions(string id, Type modelType, Type viewModelType, Func<BaseIOModel> modelFunc, Func<BaseIOModel, BaseIOViewModel> viewModelFunc) {
                return new Entry(id, modelType, viewModelType, modelFunc, viewModelFunc);
            }

            public BaseIOModel CreateModel() {
                if (this.ModelFunc != null)
                    return this.ModelFunc();
                return (BaseIOModel) Activator.CreateInstance(this.ModelType);
            }

            public BaseIOViewModel CreateViewModel() {
                return this.CreateViewModel(this.CreateModel());
            }

            public BaseIOViewModel CreateViewModel(BaseIOModel model) {
                if (this.ViewModelFunc != null)
                    return this.ViewModelFunc(model);
                return (BaseIOViewModel) Activator.CreateInstance(this.ViewModelType, model);
            }
        }
    }
}