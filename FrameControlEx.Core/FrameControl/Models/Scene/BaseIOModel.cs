using System;
using FrameControlEx.Core.RBC;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.FrameControl.Models.Scene {
    public abstract class BaseIOModel : IDisposable, IRBESerialisable {
        public string ReadableName { get; set; }

        public bool IsEnabled { get; set; } = true;

        public string TypeId => IORegistry.GetTypeIdForModel(this.GetType());

        protected BaseIOModel() {

        }

        /// <summary>
        /// Disposes this IO model. This should NOT be called from the destructor/finalizer
        /// </summary>
        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack($"Exception while disposing {this.GetType()}")) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Push(new Exception($"Unexpected exception while invoking {nameof(this.DisposeCore)}", e));
                }
            }
        }

        /// <summary>
        /// The core method for disposing of sources and outputs. This method really should not throw,
        /// and instead, exceptions should be added to the given <see cref="ExceptionStack"/>
        /// </summary>
        /// <param name="e">The exception stack in which exception should be added into when encountered during disposal</param>
        /// <param name="isDisposing"></param>
        protected virtual void DisposeCore(ExceptionStack e) {

        }

        public virtual void WriteToRBE(RBEDictionary data) {
            // Save memory by not writing basic stuff if it is defaulted
            if (!(this.TypeId is string id))
                throw new Exception($"Model Type is not registered: {this.GetType()}");
            data.SetString(nameof(this.TypeId), id);
            if (!string.IsNullOrEmpty(this.ReadableName))
                data.SetString(nameof(this.ReadableName), this.ReadableName);
            if (!this.IsEnabled)
                data.SetBool(nameof(this.IsEnabled), false);
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.ReadableName = data.GetString(nameof(this.ReadableName), null);
            this.IsEnabled = data.GetBool(nameof(this.IsEnabled), true);
        }

        public static RBEDictionary WriteToRBE(BaseIOModel model) {
            RBEDictionary data = new RBEDictionary();
            string id = model.TypeId;
            if (id == null) {
                throw new Exception($"Model Type is not registered: {model.GetType()}");
            }

            data.SetString(nameof(model.TypeId), id);
            model.WriteToRBE(data);
            return data;
        }

        public static T ReadFromRBE<T>(RBEDictionary data) where T : BaseIOModel {
            BaseIOModel model = CreateModelForId(data);
            if (!(model is T targetModel)) {
                throw new Exception($"Failed to create model of type {typeof(T)}. Got {model?.GetType()} instead");
            }

            targetModel.ReadFromRBE(data);
            return targetModel;
        }

        public static BaseIOModel CreateModelForId(RBEDictionary data) {
            string dataId = data.GetString(nameof(TypeId));
            if (string.IsNullOrWhiteSpace(dataId)) {
                throw new Exception($"RBE data contained a null or empty TypeId string");
            }

            return IORegistry.CreateModel(dataId);
        }
    }
}