using System;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Instruments Json Serializer.
    /// </summary>
    public class MetricJsonSerializer : JsonSerializer
    {
        /// <summary>
        /// Cached timers.
        /// </summary>
        private readonly TimerMetric _serialize;
        private readonly TimerMetric _deserialize;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MetricJsonSerializer(IMetricsService metrics)
        {
            _serialize = metrics.Timer("Json.Serialize");
            _deserialize = metrics.Timer("Json.Deserialize");
        }

        /// <inheritdoc />
        public override void Serialize(object value, out byte[] bytes)
        {
            var id = _serialize.Start();

            base.Serialize(value, out bytes);

            _serialize.Stop(id);
        }

        /// <inheritdoc />
        public override void Deserialize(Type type, ref byte[] bytes, out object value)
        {
            var id = _deserialize.Start();

            base.Deserialize(type, ref bytes, out value);

            _deserialize.Stop(id);
        }
    }
}