using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixPlays.Framework.Events
{
    public class EventManager
    {
        private static Dictionary<Type, List<WeakReferenceEvent>> _eventDictionary = new Dictionary<Type, List<WeakReferenceEvent>>();

        public static void Subscribe<T>(Action<T> action)
        {
            if (_eventDictionary.TryGetValue(typeof(T), out List<WeakReferenceEvent> subscribers))
            {
                subscribers.Add(new WeakReferenceEvent(action));
            }
            else
            {
                List<WeakReferenceEvent> list = new List<WeakReferenceEvent>();
                list.Add(new WeakReferenceEvent(action));
                _eventDictionary.Add(typeof(T), list);
            }
        }

        public static void Unsubscribe(object obj)
        {
            foreach (var eventType in _eventDictionary)
            {
                eventType.Value.RemoveAll(x => x.TryGetTarget(out var t) && t.Target == obj);
            }
        }

        public static void Fire<T>(T eventObject = default)
        {
            if (_eventDictionary.TryGetValue(typeof(T), out List<WeakReferenceEvent> subscribers))
            {
                List<WeakReferenceEvent> toRemove = new List<WeakReferenceEvent>();
                for (int i = 0; i < subscribers.Count; i++)
                {
                    if (subscribers[i].TryGetTarget(out var delegateEvent))
                    {
                        Action<T> action = (Action<T>)delegateEvent;
                        action?.Invoke(eventObject);
                    }
                    else
                    {
                        toRemove.Add(subscribers[i]);
                    }
                }
                for (int i = 0; i < toRemove.Count; i++)
                {
                    subscribers.Remove(toRemove[i]);
                }
            }
        }
    }
}