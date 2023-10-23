using System.Text;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Elfsquad.ParseableSerilogSink;

public class ParseableSink : IBatchedLogEventSink, IDisposable, IAsyncDisposable
{
    private readonly ValidatedParseableSinkOptions _options;
    private readonly HttpClient _httpClient;

    public ParseableSink(ParseableSinkOptions options)
    {
        _options = options.Validate();
        _httpClient = new HttpClient();
    }
    
    public Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
        var batch = events.Select(CreateDictionary).ToList();
        return SendToParseableAsync(batch);
    }

    private async Task SendToParseableAsync(List<Dictionary<string, object>> batch)
    {
        var accessToken = Environment.GetEnvironmentVariable("PARSEABLE_ACCESS_TOKEN");
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new Exception("PARSEABLE_ACCESS_TOKEN environment variable is not set");

        var host = _options.Host;
        var stream = _options.Stream;

        var endpoint = $"http://{host}/api/v1/ingest";
        var json = JsonConvert.SerializeObject(batch);

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Add("Authorization", $"Basic {accessToken}");
        request.Headers.Add("X-P-Stream", stream);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error sending logs to Parseable: {response.StatusCode}");
    }
    
    private Dictionary<string, object> CreateDictionary(LogEvent logEvent)
    {
        var properties = new Dictionary<string, object>();
        LogException(logEvent, properties);

        properties["level"] = logEvent.Level.ToString();
        properties["message"] = logEvent.RenderMessage();

        foreach (var property in logEvent.Properties)
        {
            if (property.Value is not ScalarValue scalarValue)
                continue;
            
            switch (scalarValue.Value)
            {
                case null:
                    continue;
                case string stringValue:
                    properties.Add(property.Key, stringValue);
                    break;
                case bool boolValue:
                    properties.Add(property.Key, boolValue);
                    break;
                case int intValue:
                    properties.Add(property.Key, intValue);
                    break;
                case long longValue:
                    properties.Add(property.Key, longValue);
                    break;
                case float floatValue:
                    properties.Add(property.Key, floatValue);
                    break;
                case double doubleValue:
                    properties.Add(property.Key, doubleValue);
                    break;
                case decimal decimalValue:
                    properties.Add(property.Key, decimalValue);
                    break;
                case Guid guidValue:
                    properties.Add(property.Key, guidValue.ToString());
                    break;
                case DateTimeOffset dateTimeOffsetValue:
                    properties.Add(property.Key, dateTimeOffsetValue.ToString("o"));
                    break;
            }
        }

        return properties;
    }
    
    private static void LogException(LogEvent logEvent, Dictionary<string, object> values)
    {
        if (logEvent.Exception == null) return;

        values.Add("exception_message", logEvent.Exception.Message);
        values.Add("exception_type", logEvent.Exception.GetType().Name);
        if (!string.IsNullOrWhiteSpace(logEvent.Exception.StackTrace))
            values.Add("exception_stack_trace", logEvent.Exception.StackTrace.Replace("\"", "'"));
        values.TryAdd("event_type", "exception");
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return default;
    }
}
