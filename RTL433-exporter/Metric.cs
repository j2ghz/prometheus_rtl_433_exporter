using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace RTL433_exporter
{
    public class Metric
    {
        public string Unique { get; }
        public string Name { get; }
        public double Value { get; }
        public string[] LabelValues { get; }

        public Metric(string unique, string name, double value, string[] labelValues)
        {
            Unique = unique;
            Name = name;
            Value = value;
            LabelValues = labelValues;
        }
    }
}
