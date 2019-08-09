﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Prometheus.Client;
using Prometheus.Client.MetricServer;

namespace RTL433_exporter
{
    internal class Program
    {
        private static readonly IDictionary<string, Gauge> metrics = new ConcurrentDictionary<string, Gauge>();

        private static void Main(string[] args)
        {
            var fileName = args[0];

            var metricServer = new MetricServer(null, new MetricServerOptions {Port = 58433});
            try
            {
                metricServer.Start();

                Observable.Create<string>(async (o, cancel) =>
                    {
                        using var reader = File.OpenText(fileName);
                        while (!cancel.IsCancellationRequested)
                        {
                            var line = reader.ReadLine();
                            if (reader.BaseStream.Length < reader.BaseStream.Position)
                                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                            if (line != null)
                            {
                                Console.WriteLine();
                                o.OnNext(line);
                            }
                            else
                            {
                                await Task.Delay(1000, cancel);
                            }
                        }

                        o.OnCompleted();
                    })
                    .Select(JObject.Parse)
                    .Select(j => new Reading(j))
                    .SelectMany(r => r.Metrics.Select(m => new Metric(r.Unique + m.Key, m.Key, m.Value, r.Labels)))
                    .Subscribe(m =>
                    {
                        if (!metrics.TryGetValue(m.Unique, out var gauge))
                            gauge = Metrics.CreateGauge(m.Name, "", true, "model", "channel", "id", "unique");
                        gauge.WithLabels(m.LabelValues)
                            .Set(m.Value, DateTimeOffset.Now);
                    });
            }
            finally
            {
                metricServer.Stop();
            }
        }
    }
}