using SqTS;
using Whisper.net.Ggml;
using Whisper.net.LibraryLoader;

// Setup
Console.Title = $"SqTS (fully free!!!)";
bool HotReloading = true;

// Objects
string configPath = "config.scx";

FilterEngine filter = new();
VoiceSynth? synth = null;
AudioListener? listener = null;
WhisperTranscriber? transcriber = null;
DateTime lastRead = DateTime.MinValue;

// Handling Arguments
foreach (string arg in args)
    switch (arg)
    {
        case "--list-sapi-voices":
            Console.Clear();

            var voices = VoiceSynth.GetVoices();

            if (voices == null)
                Environment.Exit(1);

            Console.WriteLine("\n[SAPI5 VOICES]");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            foreach (var voice in voices)
                Console.WriteLine(voice);

            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
            break;

        case "--no-hot-reload":
            Console.WriteLine("[HOT RELOADING DISABLED]");
            HotReloading = false;
            break;
    }

LoadConfig();

// Hot reloading
if (HotReloading)
{
    using var watcher = new FileSystemWatcher(Directory.GetCurrentDirectory(), configPath);
    watcher.NotifyFilter = NotifyFilters.LastWrite;
    watcher.Changed += (s, e) => LoadConfig();
    watcher.EnableRaisingEvents = true;
}

// Loader
async void LoadConfig()
{
    // Debounce
    if (DateTime.Now - lastRead < TimeSpan.FromMilliseconds(500)) return;
    lastRead = DateTime.Now;

    Console.Clear();

    try
    {
        // Setup
        listener?.Dispose();
        listener = new AudioListener(filter.Settings.MinVolume, filter.Settings.SilenceWait);

        synth?.Dispose();
        synth = new VoiceSynth(filter.Settings.TtsVoice, filter.Settings.SpeakingRate);

        transcriber?.Dispose();

        // Load config
        if (File.Exists(configPath))
        {
            filter.LoadConfig(configPath);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[HOT RELOAD] Loaded {filter.Settings.ConfigName} by {filter.Settings.ConfigAuthor}.");
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        else
        {
            Console.WriteLine($"[WARN] {configPath} missing. Using defaults. (or previous)");
        }

        // Transcriber stuff
        GgmlType model = GgmlType.TinyEn;

        switch (filter.Settings.SttModel.ToString().ToLowerInvariant())
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

        // Extra
        Console.WriteLine($"[RT] Using the {RuntimeOptions.LoadedLibrary} runtime.");
        Console.WriteLine($"[RT] Using the {filter.Settings.SttModel} STT model.");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[SETTINGS]" +
            $"\n[CONFIDENCE GATE]: {filter.Settings.MinConfidence}" +
            $"\n[VOLUME GATE]:     {filter.Settings.MinVolume}" +
            $"\n[SHORTNESS GATE]:  {filter.Settings.MinShortness}" +
            $"\n[SILENCE WAIT]:    {filter.Settings.SilenceWait}" +
            $"\n[SPEAKING RATE]:   {filter.Settings.SpeakingRate}" +
            $"\n[VOICE]:           {filter.Settings.TtsVoice}"
        );
        Console.ResetColor();

        if (filter.Settings.ConfigDescription != "")
            Console.WriteLine($"[COMMENT] {filter.Settings.ConfigDescription}");

        // Listener
        listener.OnSegmentReady += async (audioData) =>
        {
            await foreach (var (Text, Prob) in transcriber.TranscribeAsync(audioData))
            {
                var result = filter.Process(Text, Prob);

                if (result.Blocked)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[FILTERED, {Prob:P0}] {Text}");
                    Console.ResetColor();
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[PASSED, {Prob:P0}] {Text}");
                Console.ResetColor();
                synth.Speak(result.Text);
            }
        };

        listener.Start();
        Console.WriteLine("[INFO] SqTS is Active!\n[INFO]You can speak now.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Oopsies! Something unexpected went wrong!\n({ex.Message})");
    }
}

await Task.Delay(-1);