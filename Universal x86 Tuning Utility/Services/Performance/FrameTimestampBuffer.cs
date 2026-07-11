using System;
using System.Buffers;

namespace Universal_x86_Tuning_Utility.Services.Performance
{
    internal sealed class FrameTimestampBuffer
    {
        private const int Capacity = 8192;
        private readonly double[] _timestamps = new double[Capacity];
        private readonly object _sync = new();
        private int _nextIndex;
        private int _count;

        public bool HasDxgiEvents { get; set; }

        public void Add(double timestampMs)
        {
            lock (_sync)
            {
                _timestamps[_nextIndex] = timestampMs;
                _nextIndex = (_nextIndex + 1) % Capacity;
                if (_count < Capacity)
                    _count++;
            }
        }

        public FrameStatistics Calculate(double nowMs)
        {
            lock (_sync)
            {
                if (_count < 2)
                    return default;

                double[] rented = ArrayPool<double>.Shared.Rent(_count - 1);
                try
                {
                    int intervalCount = 0;
                    double recentTotal = 0;
                    int recentCount = 0;
                    double previous = 0;

                    for (int i = 0; i < _count; i++)
                    {
                        double current = GetChronological(i);
                        if (current < nowMs - 5000)
                            continue;

                        if (previous > 0)
                        {
                            double frameTime = current - previous;
                            if (frameTime is > 0 and < 1000)
                            {
                                rented[intervalCount++] = frameTime;
                                if (current >= nowMs - 500)
                                {
                                    recentTotal += frameTime;
                                    recentCount++;
                                }
                            }
                        }

                        previous = current;
                    }

                    if (intervalCount == 0)
                        return default;

                    Array.Sort(rented, 0, intervalCount);
                    double currentFrameTime = recentCount > 0 ? recentTotal / recentCount : rented[intervalCount / 2];
                    double onePercentFrameTime = AverageSlowest(rented, intervalCount, 0.01);
                    double pointOnePercentFrameTime = AverageSlowest(rented, intervalCount, 0.001);

                    return new FrameStatistics(
                        1000d / currentFrameTime,
                        currentFrameTime,
                        1000d / onePercentFrameTime,
                        onePercentFrameTime,
                        1000d / pointOnePercentFrameTime,
                        pointOnePercentFrameTime);
                }
                finally
                {
                    ArrayPool<double>.Shared.Return(rented);
                }
            }
        }

        private double GetChronological(int index)
        {
            int oldest = _count == Capacity ? _nextIndex : 0;
            return _timestamps[(oldest + index) % Capacity];
        }

        private static double AverageSlowest(double[] sorted, int count, double percentile)
        {
            int sampleCount = Math.Max(1, (int)Math.Ceiling(count * percentile));
            double total = 0;
            for (int i = count - sampleCount; i < count; i++)
                total += sorted[i];
            return total / sampleCount;
        }
    }

    internal readonly record struct FrameStatistics(
        double Fps,
        double FrameTimeMs,
        double OnePercentLowFps,
        double OnePercentFrameTimeMs,
        double PointOnePercentLowFps,
        double PointOnePercentFrameTimeMs);
}
