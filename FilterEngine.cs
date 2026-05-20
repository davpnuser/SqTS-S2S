using Whisper.net.Ggml;

namespace SqTS;

public class FilterEngine
{
    public FilteringSettings Settings = new();
    public Oopsies Process(string _, float __) => new(true, "");

    public void LoadConfig(string _) { }

    /* Ha, yes! i AM rewriting this. */
}

public record Oopsies(bool Blocked,string Text);
public class FilteringSettings
{
    public string ConfigName { get; set; } = "";
    public string ConfigAuthor { get; set; } = "";
    public string ConfigDescription { get; set; } = "";
    public GgmlType SttModel { get; set; } = GgmlType.TinyEn;
    public float MinConfidence { get; set; } = 0.45f;
    public string TtsVoice { get; set; } = "Microsoft Zira Desktop";
    public int SpeakingRate { get; set; } = 0;
    public int SilenceWait { get; set; } = 600;
    public int MinShortness { get; set; } = 8000;
    public float MinVolume { get; set; } = 0.02f;
}