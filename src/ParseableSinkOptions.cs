namespace Elfsquad.ParseableSerilogSink;

public class ParseableSinkOptions
{
    public string? Host { get; set; } = null!;
    
    public string? Stream { get; set; } = null!;

    public int BatchSizeLimit { get; set; } = 1000;
    
    public ValidatedParseableSinkOptions Validate()
    {
        AssertValidHost();
        AssertValidStream();
        return new ValidatedParseableSinkOptions(Host!, Stream!, BatchSizeLimit);
    }
    
    private void AssertValidHost()
    {
        if (string.IsNullOrWhiteSpace(Host))
            throw new ArgumentException("Host is required", nameof(Host));
    }
    
    private void AssertValidStream()
    {
        if (string.IsNullOrWhiteSpace(Stream))
            throw new ArgumentException("Stream is required", nameof(Stream));
        
        if (Stream.Any(c => !char.IsLetterOrDigit(c)))
            throw new ArgumentException("Stream contains special characters", nameof(Stream));
        
        if (Stream.Any(Char.IsUpper))
            throw new ArgumentException("Stream contains uppercase characters", nameof(Stream));
    }
}

public class ValidatedParseableSinkOptions
{
    public string Host { get; }
    
    public string Stream { get; }

    public int BatchSizeLimit { get; }
    
    internal ValidatedParseableSinkOptions(string host, string stream, int batchSizeLimit)
    {
        Host = host;
        Stream = stream;
        BatchSizeLimit = batchSizeLimit;
    }
}
