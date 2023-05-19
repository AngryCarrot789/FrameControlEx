using System;

namespace FrameControlEx.Core.Services {
    public abstract class DispatcherService {
        public abstract IDispatcher ForCurrentThread();
    }
}