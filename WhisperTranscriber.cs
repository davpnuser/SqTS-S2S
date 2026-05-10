using Whisper.net;
using Whisper.net.Ggml;

namespace StS_Demo;

public class WhisperTranscriber : IDisposable
{
    public static string ModelPath { get; private set; } = "";
    private WhisperProcessor? _processor;
    private bool _disposed = false;

    public WhisperTranscriber()
    {
        var factory = WhisperFactory.FromPath(ModelPath);

        _processor = factory.CreateBuilder()
            .WithLanguage("en")
            .WithProbabilities()
            .Build();
    }

    public static async Task CheckModel(GgmlType model)
    {
        ModelPath = $"{model}.bin";

        if (!File.Exists(ModelPath))
            await DownloadModel(model);
    }

    public async IAsyncEnumerable<(string Text, float Prob)> TranscribeAsync(byte[] audioData)
    {
        using var ms = new MemoryStream(audioData);

        if (_processor == null)
            throw new Exception("The whisper STT processor has not been initialized yet!", null);

        await foreach (var result in _processor.ProcessAsync(ms))
            yield return (result.Text.Trim(), result.Probability);
    }

    private static async Task DownloadModel(GgmlType model)
    {
        Console.WriteLine($"[RT] Downloading the {model} STT model.");
        using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(model);
        using var fileWriter = File.OpenWrite(ModelPath);
        await modelStream.CopyToAsync(fileWriter);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            { 
                _processor?.Dispose();
                _processor = null;
            }

            _disposed = true;
        }
    }
}