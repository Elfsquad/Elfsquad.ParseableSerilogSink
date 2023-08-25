# Elfsquad.ParseableSerilogSink

## Usage

```c#
var parseableSinkOptions = new ParseableSinkOptions
{
    Host = "localhost:8000",
    Stream = "teststream",
    BatchSizeLimit = 100,
};

builder.UseSerilog((_, services, configuration) =>
{
    configuration
        .WriteTo.Parseable(parseableSinkOptions)
    ...
}
```
