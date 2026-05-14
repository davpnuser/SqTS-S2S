using NAudio.Utils;
using NAudio.Wave;

namespace SqTS;

public class AudioListener : IDisposable
{
    private readonly WaveInEvent _waveIn;
    private readonly MemoryStream _buffer = new();
    private WaveFileWriter _writer;
    private readonly float _threshold;
    private readonly int _silenceTimeout;
    private DateTime _lastVolumeTime = DateTime.Now;
    private bool _isSpeaking = false;
    private bool _disposed = false;

    public event Action<byte[]>? OnSegmentReady;

    public AudioListener(float threshold, int silenceTimeout)
    {
        _threshold = threshold;
        _silenceTimeout = silenceTimeout;
        _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 1) };
        _writer = new WaveFileWriter(new IgnoreDisposeStream(_buffer), _waveIn.WaveFormat);

        _waveIn.DataAvailable += ProcessAudio;
    }

    private void ProcessAudio(object? sender, WaveInEventArgs e)
    {
        float max = 0;
        var buffer = new WaveBuffer(e.Buffer);
        for (int i = 0; i < e.BytesRecorded / 2; i++)
        {
            float sample = Math.Abs(buffer.ShortBuffer[i] / 32768f);
            if (sample > max) max = sample;
        }

        if (max > _threshold)
        {
            _isSpeaking = true;
            _lastVolumeTime = DateTime.Now;
        }

        _writer.Write(e.Buffer, 0, e.BytesRecorded);
        CheckSilence();
    }

    private void CheckSilence()
    {
        if (_isSpeaking && (DateTime.Now - _lastVolumeTime).TotalMilliseconds > _silenceTimeout)
        {
            _isSpeaking = false;
            _writer.Flush();
            var data = _buffer.ToArray();

            if (data.Length > 8000) OnSegmentReady?.Invoke(data);

            ResetBuffer();
        }
    }

    private void ResetBuffer()
    {
        _buffer.SetLength(0);
        _writer = new WaveFileWriter(new IgnoreDisposeStream(_buffer), _waveIn.WaveFormat);
    }

    public void Start() => _waveIn.StartRecording();

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
                _waveIn.DataAvailable -= ProcessAudio;
                _waveIn.StopRecording();
                _waveIn.Dispose();
                _writer.Dispose();
                _buffer.Dispose();
            }

            _disposed = true;
        }
    }
}