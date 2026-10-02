namespace NQueen.Kernel.Solvers.Engines;

internal readonly struct ProgressReporter(Action<double> report, int bucketSize = 1, int heartbeatMs = 1500)
{
    private readonly Action<double> _report = report;
    private readonly int _bucketSize = bucketSize;
    private readonly Stopwatch _heartbeat = Stopwatch.StartNew();
    private readonly int _heartbeatMs = heartbeatMs;

    public void ReportBucket(int done, int totalTasks, ref int bucketReported)
    {
        double pct = totalTasks == 0 ? 100.0 : (double)done / totalTasks * 100.0;
        int bucket = (int)pct / _bucketSize * _bucketSize;
        int observed;
        while (bucket > (observed = Volatile.Read(ref bucketReported)))
        {
            if (Interlocked.CompareExchange(ref bucketReported, bucket, observed) == observed)
            {
                _report(bucket);
                break;
            }
        }
        if (_heartbeat.ElapsedMilliseconds >= _heartbeatMs)
        {
            _report(Math.Min(99.0, pct));
            _heartbeat.Restart();
        }
    }
}
