using System.Collections.Generic;

namespace Supono.App.Analytics
{
    /// <summary>Receives an event's parameters; each backend maps them to its own types.</summary>
    public interface IEventParameters
    {
        void Add(string name, string value);
        void Add(string name, long value);
        void Add(string name, double value);
    }

    /// <summary>
    /// One analytics event. Each event type knows its own name and parameters, so backends never
    /// switch over event kinds. Names follow Firebase's recommended events where one exists.
    /// </summary>
    public abstract class AnalyticsEvent
    {
        public abstract string Name { get; }
        public abstract void Describe(IEventParameters parameters);
    }

    /// <summary>Collects parameters into a dictionary (logging, tests).</summary>
    public sealed class ParameterDictionary : IEventParameters
    {
        public readonly Dictionary<string, object> Values = new();

        public void Add(string name, string value) => Values[name] = value;
        public void Add(string name, long value) => Values[name] = value;
        public void Add(string name, double value) => Values[name] = value;
    }
}
