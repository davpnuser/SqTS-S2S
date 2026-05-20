using System.Speech.Synthesis;

namespace SqTS;

public class VoiceSynth : IDisposable
{
    private readonly SpeechSynthesizer _synth;
    private bool _disposed = false;

    public VoiceSynth(string voiceName, int rate)
    {
        _synth = new SpeechSynthesizer();
        _synth.SetOutputToDefaultAudioDevice();
        _synth.Rate = rate;
        _synth.SelectVoice(voiceName);
    }

    public static List<string> GetVoices()
    {
        List<string> voiceList = [];

        foreach (var item in new SpeechSynthesizer().GetInstalledVoices())
            voiceList.Add(item.VoiceInfo.Name);

        return voiceList;
    }

    public void Speak(string text) => _synth.SpeakAsync(text);

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
                _synth?.Dispose();

            _disposed = true;
        }
    }
}