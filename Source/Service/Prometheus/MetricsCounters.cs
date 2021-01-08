using Prometheus;

namespace Service.Prometheus
{
    public static class MetricsCounters
    {
        public static readonly Counter ProcCnt = Metrics.CreateCounter("gw_archiveprocessing_proc_total", "Total number of processed archives.",
            new CounterConfiguration
            {
                LabelNames = new[] { "outcome" }
            });

        public static readonly Histogram ProcTime = Metrics.CreateHistogram("gw_archiveprocessing_proc_time", "Time taken to process archive.");
    }
}
