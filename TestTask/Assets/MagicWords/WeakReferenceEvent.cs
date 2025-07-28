using System;
using System.Reflection;
namespace PixPlays.Framework.Events
{
    public class WeakReferenceEvent
    {
        private WeakReference _weakReference;
        private MethodInfo _methodInfo;
        private Type _delegateType;

        public WeakReferenceEvent(Delegate delegateEvent)
        {
            _delegateType = delegateEvent.GetType();
            _methodInfo = delegateEvent.Method;
            _weakReference = new WeakReference(delegateEvent.Target);
        }

        public bool TryGetTarget(out Delegate outEvent)
        {
            if (_weakReference.IsAlive && _weakReference.Target != null && !_weakReference.Target.Equals(null))
            {
                outEvent = _methodInfo.CreateDelegate(_delegateType, _weakReference.Target);
                return true;
            }
            outEvent = null;
            return false;
        }
    }
}