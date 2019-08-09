using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RTL433_exporter
{
    public class Reading
    {
        private readonly IDictionary<string, double> _metrics = new Dictionary<string, double>();

        public Reading(JObject obj)
        {
            Model = obj["model"]?.ToString() ?? "Unknown";
            Channel = obj["channel"]?.ToString() ?? "Unknown";
            ID = obj["id"]?.ToString() ?? "Unknown";

            var metrics = obj.Properties()
                .Where(p => !new[] {"time", "model", "id", "channel"}.Contains(p.Name))
                .Where(p => p.Value.Type == JTokenType.Boolean || p.Value.Type == JTokenType.Integer ||
                            p.Value.Type == JTokenType.Float);


            foreach (var metric in metrics) _metrics.Add(metric.Name, metric.Value.ToObject<double>());
        }

        public string Channel { get; }
        public string ID { get; }

        public string[] Labels => new[] {Model, Channel, ID, Unique};

        public IReadOnlyDictionary<string, double> Metrics => new ReadOnlyDictionary<string, double>(_metrics);

        public string Model { get; }
        public string Unique => string.Join('-', Model, Channel, ID);
    }
}