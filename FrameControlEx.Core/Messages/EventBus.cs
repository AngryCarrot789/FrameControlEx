using System;
using System.Collections.Generic;
using System.Threading;

namespace FrameControlEx.Core.Messages {
    /// <summary>
    /// Stores information about event handlers for a specific event type
    /// </summary>
    public class EventBus { // totally took the name from forge :P
        private static long NEXT_ID;

        private readonly object registrationLock;
        private Dictionary<Type, List<RegisteredListener>> listeners_exact;
        private Dictionary<Type, List<RegisteredListener>> listeners_derived;

        public string BusName { get; }

        public long Id { get; }

        public EventBus(string busName) {
            this.BusName = busName;
            this.Id = Interlocked.Increment(ref NEXT_ID);
            this.registrationLock = new object();
        }

        /// <summary>
        /// Registers an event handler for the specific instance
        /// </summary>
        /// <param name="instance">The instance in which the handler is invoked</param>
        /// <param name="handler"></param>
        /// <param name="keepTargetAlive"></param>
        /// <param name="canReceiveDerivedEvent"></param>
        /// <typeparam name="TEvent"></typeparam>
        public void Register<TEvent>(object target, Action<TEvent> handler, bool keepTargetAlive = false, bool canReceiveDerivedEvent = true) {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");
            }

            lock (this.registrationLock) {
                Type eventType = typeof(TEvent);
                Dictionary<Type, List<RegisteredListener>> dictionary;
                if (canReceiveDerivedEvent) {
                    dictionary = this.listeners_derived ?? (this.listeners_derived = new Dictionary<Type, List<RegisteredListener>>());
                }
                else {
                    dictionary = this.listeners_exact ?? (this.listeners_exact = new Dictionary<Type, List<RegisteredListener>>());
                }

                if (!dictionary.TryGetValue(eventType, out List<RegisteredListener> list)) {
                    dictionary[eventType] = list = new List<RegisteredListener>();
                }

                list.Add(new RegisteredListener(target, handler, handler.Method.IsStatic, keepTargetAlive, eventType, canReceiveDerivedEvent));
            }
        }

        public void Post<TEvent>(in TEvent e) {
            if (e == null) {
                throw new ArgumentNullException(nameof(e), "Event cannot be null");
            }

            lock (this.registrationLock) {
                Type eventType = typeof(TEvent);
                if (this.listeners_exact.TryGetValue(eventType, out List<RegisteredListener> list)) {
                    this.SendToList(in e, list);
                }
            }
        }

        private void SendToList<TEvent>(in TEvent e, List<RegisteredListener> list) {
            Type type = null;
            for (int i = list.Count - 1; i >= 0; i--) {
                RegisteredListener listener = list[i];
                if (!listener.GetTargetHandler(out object handler)) {
                    list.RemoveAt(i);
                }
                else if (handler is Action<TEvent> action) {
                    if (!listener.exactType || listener.eventType == (type ?? (type = e.GetType()))) {
                        action(e);
                    }
                }
            }
        }

        public override string ToString() {
            return $"{nameof(EventBus)}({this.BusName}: 0 total handlers)";
        }

        private class RegisteredListener {
            public readonly WeakReference<object> targetWeak;
            public readonly WeakReference<object> handlerWeak;
            public readonly object targetStrong;
            public readonly object handlerStrong;
            public readonly bool isStatic;
            public readonly bool keepAlive;
            public readonly Type eventType;
            public readonly bool exactType;

            public RegisteredListener(object target, object handler, bool isStatic, bool keepTargetAlive, Type eventType, bool exactType) {
                this.isStatic = isStatic;
                if (!isStatic && target == null)
                    throw new Exception("Handler is non-static but target is null");
                if (isStatic && target != null)
                    throw new Exception("Handler is static but target is non-null");

                this.exactType = exactType;
                this.eventType = eventType;
                this.keepAlive = keepTargetAlive;
                if (keepTargetAlive) {
                    this.targetStrong = target;
                    this.handlerStrong = handler;
                }
                else {
                    this.targetWeak = new WeakReference<object>(target);
                    this.handlerWeak = new WeakReference<object>(handler);
                }
            }

            public bool GetTargetHandler(out object instance) {
                if (!this.IsTargetAlive()) {
                    instance = null;
                    return false;
                }

                if (this.keepAlive) {
                    return (instance = this.handlerStrong) != null;
                }
                else if (this.handlerWeak.TryGetTarget(out instance)) {
                    return true;
                }
                else {
                    return false;
                }
            }

            public bool IsTargetAlive() {
                if (this.isStatic || this.keepAlive) {
                    return true;
                }

                return this.targetWeak.TryGetTarget(out _);
            }
        }
    }
}