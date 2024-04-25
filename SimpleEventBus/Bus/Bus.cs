using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleEventBus.Bus
{
    /// <summary>
    /// Am event bus that allows for the publishing and subscribing to events.
    /// Uses reflection for auto search of methods with the Subscribe attribute.
    /// Instance methods should be Subscribed manually, otherwise a new instance of the class will be created.
    /// </summary>
    public class Bus
    {
        /// <summary>
        /// A dictionary of event names to a list of handlers.
        /// </summary>
        protected readonly Dictionary<string, List<Delegate>> _eventHandlers = [];
        private static Lazy<Bus>? _instance;
        private readonly object @lock = new();

        /// <summary>
        /// Returns the instance of the bus.
        /// Searches for all methods with the Subscribe attribute by default.
        /// </summary>
        /// <param name="autoSearch"> Auto search for Methods with the Subscribe Attribute </param>
        /// <param name="flags">Flags to use when searching for methods</param>
        /// <returns> Returns the instance of the Bus </returns>
        public static Bus GetInstance(bool autoSearch = true, BindingFlags? flags = null)
        {
            _instance ??= new Lazy<Bus>(() => new Bus(autoSearch, flags));

            return _instance.Value;
        }

        /// <summary>
        /// Creates a new instance of the bus.
        /// Searches for all methods with the Subscribe attribute by default.
        /// </summary>
        /// <param name="autoSearch">If the Bus should search all Assemblies for subscriptions</param>
        /// <param name="flags">Flags to use when searching for methods</param>
        private Bus(bool autoSearch, BindingFlags? flags)
        {
            if (!autoSearch)
                return;

            flags ??= BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

            var methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass)
                .SelectMany(x => x.GetMethods((BindingFlags)flags))
                .Where(x => x.GetCustomAttributes(typeof(SubscribeAttribute), false).Length != 0);

            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttributes(typeof(SubscribeAttribute), false).FirstOrDefault() is not SubscribeAttribute attribute)
                {
                    continue;
                }

                foreach (string className in attribute.ClassNames)
                {
                    // Get the parameter types of the method
                    Type[] parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                    // Determine the delegate type based on the method's return type (void) and parameter types
                    Type delegateType = Expression.GetActionType(parameterTypes);
                    // Create the delegate for the method

                    if (method.IsStatic)
                    {
                        Delegate methodDelegate = Delegate.CreateDelegate(delegateType, method);
                        Subscribe(className, methodDelegate);
                    }
                    else
                    {
                        // If the method is not static, we need to create an instance of the class to call the method
                        object instance = Activator.CreateInstance(method.DeclaringType!)!;

                        // Create a delegate that calls the method on the instance
                        Delegate methodDelegate = Delegate.CreateDelegate(delegateType, instance, method);
                        Subscribe(className, methodDelegate);
                    }
                }
            }
        }

        /// <summary>
        /// Subscribes an Delegate to an event.
        /// </summary>
        /// <param name="eventName">The event to subscribe to</param>
        /// <param name="handler">The delegate to invoke on event publishing</param>
        public void Subscribe(string eventName, Delegate handler)
        {
            lock (@lock)
            {
                if (!_eventHandlers.TryGetValue(eventName, out List<Delegate>? value))
                {
                    value = [];
                    _eventHandlers.Add(eventName, value);
                }

                value.Add(handler); 
            }
        }

        /// <summary>
        /// Unsubscribes a handler from an event.
        /// </summary>
        /// <param name="eventName">The event to unsubscribe from</param>
        /// <param name="handler">The delegate to remove from the given event</param>
        public void Unsubscribe(string eventName, Delegate handler)
        {
            lock (@lock)
            {
                if (!_eventHandlers.TryGetValue(eventName, out List<Delegate>? handlers))
                {
                    return;
                }

                handlers?.Remove(handler); 
            }
        }

        /// <summary>
        /// Publishes an event with the given data.
        /// </summary>
        /// <param name="eventName">The event name to publish the event to</param>
        /// <param name="data">The data du publish on the event</param>
        public virtual void Publish(string eventName, object[] data)
        {
            List<Delegate>? handlers;
            lock (@lock)
            {
                if (!_eventHandlers.TryGetValue(eventName, out handlers))
                {
                    return;
                } 
            }

            foreach (var handler in handlers)
            {
                handler.DynamicInvoke(data);
            }
        }
    }
}
