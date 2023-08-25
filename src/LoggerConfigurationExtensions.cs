using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.PeriodicBatching;

namespace Elfsquad.ParseableSerilogSink;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration Parseable(
        this LoggerSinkConfiguration loggerConfiguration,
        ParseableSinkOptions options
    )
    {
        if (loggerConfiguration == null)
            throw new ArgumentNullException(nameof(loggerConfiguration));

        var parseableSink = new ParseableSink(options);
        
        var batchingOptions = new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = options.BatchSizeLimit,
            Period = TimeSpan.FromSeconds(1),
            EagerlyEmitFirstEvent = true
        };
        
        var batchingSink = new PeriodicBatchingSink(parseableSink, batchingOptions);

        return loggerConfiguration.Sink(batchingSink);
    }
}
