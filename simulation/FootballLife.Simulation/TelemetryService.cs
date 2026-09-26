using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# telemetry and analytics pipeline service.
    /// Manages event queuing, batch flushing, and GDPR privacy compliance (opt-out).
    /// Safe for both headless simulation testing and in-game analytics dispatch (UGS / PostHog).
    /// </summary>
    public sealed class TelemetryService
    {
        private static readonly Lazy<TelemetryService> _lazyInstance =
            new Lazy<TelemetryService>(() => new TelemetryService());

        public static TelemetryService Instance => _lazyInstance.Value;

        private readonly object _lock = new object();
        private readonly Queue<TelemetryEvent> _eventQueue = new Queue<TelemetryEvent>();

        public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
        public bool IsOptedOut { get; set; } = false;
        public int BufferFlushThreshold { get; set; } = 20;

        public event Action<IReadOnlyList<TelemetryEvent>>? OnBatchFlushed;

        public int BufferedEventCount
        {
            get
            {
                lock (_lock)
                {
                    return _eventQueue.Count;
                }
            }
        }

        public TelemetryService()
        {
        }

        /// <summary>
        /// Records a structured telemetry event.
        /// If the player has opted out of analytics for privacy, the event is immediately discarded.
        /// </summary>
        public void Track(TelemetryEventType type, IReadOnlyDictionary<string, object>? parameters = null)
        {
            if (IsOptedOut) return;

            var evt = TelemetryEvent.Create(type, SessionId, parameters);
            bool shouldFlush = false;

            lock (_lock)
            {
                _eventQueue.Enqueue(evt);
                if (_eventQueue.Count >= BufferFlushThreshold)
                {
                    shouldFlush = true;
                }
            }

            if (shouldFlush)
            {
                Flush();
            }
        }

        /// <summary>
        /// Empties the current event buffer and emits a batch flush.
        /// </summary>
        public IReadOnlyList<TelemetryEvent> Flush()
        {
            List<TelemetryEvent> batch;
            lock (_lock)
            {
                if (_eventQueue.Count == 0)
                {
                    return Array.Empty<TelemetryEvent>();
                }

                batch = new List<TelemetryEvent>(_eventQueue.Count);
                while (_eventQueue.Count > 0)
                {
                    batch.Add(_eventQueue.Dequeue());
                }
            }

            OnBatchFlushed?.Invoke(batch);
            return batch;
        }

        /// <summary>
        /// Clears all buffered events without firing flush handlers.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _eventQueue.Clear();
            }
        }
    }
}
