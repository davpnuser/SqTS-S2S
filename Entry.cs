using StS_Demo;
using Whisper.net.Ggml;
using Whisper.net.LibraryLoader;

Console.Title = "SqTS Real-time S2S";

var filter = new FilterEngine();
string configPath = "config.vrx";

VoiceSynth? synth = null;
AudioListener? listener = null;
WhisperTranscriber? transcriber = null;
DateTime lastRead = DateTime.MinValue;

async void Reload()
{
    if (DateTime.Now - lastRead < TimeSpan.FromMilliseconds(500)) return;
    lastRead = DateTime.Now;

    Console.Clear();

    try
    {
        listener?.Dispose();
        synth?.Dispose();
        transcriber?.Dispose();

        if (File.Exists(configPath))
        {
            filter.LoadConfig(configPath);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[HOT RELOAD] Loaded {filter.Settings.Name} by {filter.Settings.Author}.");
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        else
        {
            Console.WriteLine($"[WARN] {configPath} missing. Using defaults.");
        }

        Console.WriteLine($"[RT] Using the {filter.Settings.Model} STT model.");

        GgmlType model = GgmlType.TinyEn;

        switch (filter.Settings.Model.ToLowerInvariant())
        {
            case "base": model = GgmlType.BaseEn; break;
            case "small": model = GgmlType.SmallEn; break;
            case "medium": model = GgmlType.MediumEn; break;
            case "largev1": model = GgmlType.LargeV1; break;
            case "largev2": model = GgmlType.LargeV2; break;
            case "largev3": model = GgmlType.LargeV3; break;
            case "turbo": model = GgmlType.LargeV3Turbo; break;
        }

        await WhisperTranscriber.CheckModel(model);

        transcriber = new();

        Console.WriteLine($"[RT] Using the {RuntimeOptions.LoadedLibrary} runtime.");

        synth = new VoiceSynth(filter.Settings.Voice, filter.Settings.Rate);
        listener = new AudioListener(filter.Settings.Volume, filter.Settings.Silence);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[SETTINGS]" +
            $"\n[CONFIDENCE GATE]: {filter.Settings.Confidence}" +
            $"\n[VOLUME GATE]:     {filter.Settings.Volume}" +
            $"\n[SHORTNESS GATE]:  {filter.Settings.Shortness}" +
            $"\n[SILENCE WAIT]:    {filter.Settings.Silence}" +
            $"\n[SPEAKING RATE]:   {filter.Settings.Rate}" +
            $"\n[VOICE]:           {filter.Settings.Voice}"
        );
        Console.ResetColor();

        if (filter.Settings.Comment != "")
            Console.WriteLine($"[COMMENT] {filter.Settings.Comment}");

        listener.OnSegmentReady += async (audioData) =>
        {
            await foreach (var (Text, Prob) in transcriber.TranscribeAsync(audioData))
            {
                var text = Text.Trim();
                if (filter.ShouldBlock(text, Prob))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[FILTERED, {Prob:P0}] {text}");
                    Console.ResetColor();
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[PASSED, {Prob:P0}] {text}");
                Console.ResetColor();
                synth.Speak(text);
            }
        };

        listener.Start();
        Console.WriteLine("[INFO] Engine Active.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Reload failed: {ex.Message}");
    }
}

Reload();

if (args.Contains("-list-voices"))
{
    Console.Clear();

    var voices = synth?.GetVoices();

    if (voices == null)
        Environment.Exit(1);

    Console.WriteLine("\n[VOICES]");
    Console.ForegroundColor = ConsoleColor.DarkGray;

    foreach (var voice in voices)
        Console.WriteLine(voice);

    Console.ForegroundColor = ConsoleColor.Gray;
    Environment.Exit(0);
}

using var watcher = new FileSystemWatcher(Directory.GetCurrentDirectory(), "*.vrx");
watcher.NotifyFilter = NotifyFilters.LastWrite;
watcher.Changed += (s, e) => Reload();
watcher.EnableRaisingEvents = true;

await Task.Delay(-1);