namespace SimpleEventBus.Bus
{
    /// <summary>
    /// An attribute to mark a method as a subscriber to an event.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SubscribeAttribute : Attribute
    {
        /// <summary>
        /// The names to listen to on the bus.
        /// </summary>
        public string[] ClassNames { get; }

        /// <summary>
        /// Marks a method as a subscriber to an event.
        /// </summary>
        /// <param name="className">The name to listen to on the bus</param>
        public SubscribeAttribute(string className)
        {
            ClassNames = [className];
        }

        /// <summary>
        /// Marks a method as a subscriber to multiple events.
        /// </summary>
        /// <param name="classNames">The names to listen to on the bus</param>
        public SubscribeAttribute(string[] classNames)
        {
            ClassNames = classNames;
        }
    }
}
