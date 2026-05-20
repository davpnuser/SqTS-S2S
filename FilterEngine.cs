using Whisper.net.Ggml;

namespace SqTS;

public record FilterResult(bool Blocked, string Text);

public class FilterEngine
{
    public FilteringSettings Settings { get; private set; } = new();
    private readonly List<IFilterRule> _rules = [];

    public void LoadConfig(string path)
    {
        if (!File.Exists(path)) return;

        var configData = ConfigLoader.ParseLines(File.ReadAllLines(path));
        Settings = ConfigurationMapper.Map(configData);
        _rules.Clear();

        _rules.Add(new SimpleRule(
            new ConfidenceEvent(new Value { ValueString = Settings.MinConfidence.ToString() }),
            new BlockAction())
        );

        // 2. Truly Modular Pairing
        foreach (var (key, val) in configData)
        {
            if (key.StartsWith("on_"))
            {
                var ev = CreateEvent(key, val);
                var act = DetermineAction(configData);

                if (ev != null && act != null)
                {
                    _rules.Add(new SimpleRule(ev, act));
                }
            }
        }
    }

    private static IFilterEvent? CreateEvent(string key, Value val) => key switch
    {
        "on_contains" => new ContainsEvent(val),
        "on_confidence" => new ConfidenceEvent(val),
        _ => null
    };

    private static IFilterAction? DetermineAction(Dictionary<string, Value> data)
    {
        if (data.TryGetValue("replace_word", out var target))
        {
            var replacement = data.GetValueOrDefault("with", new Value { ValueString = "" });
            return new ReplaceAction(target, replacement);
        }

        if (data.TryGetValue("block", out var b) && b.ValueString.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return new BlockAction();
        }

        return null;
    }

    public FilterResult Process(string text, float probability)
    {
        string currentText = text;
        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(currentText, probability);
            if (result.Blocked) return result;
            currentText = result.Text;
        }
        return new FilterResult(false, currentText);
    }
}

public interface IFilterRule { FilterResult Evaluate(string text, float prob); }
public interface IFilterEvent { bool Matches(string text, float prob); }
public interface IFilterAction { FilterResult Execute(string text); }

public class SimpleRule(IFilterEvent ev, IFilterAction action) : IFilterRule
{
    public FilterResult Evaluate(string text, float prob) =>
        ev.Matches(text, prob) ? action.Execute(text) : new FilterResult(false, text);
}

// --- Events ---
public record ContainsEvent(Value Data) : IFilterEvent
{
    public bool Matches(string text, float prob)
    {
        // If it's a list (e.g., - word1 - word2), check if ANY item matches
        if (Data.IsList)
        {
            return Data.Items.Any(item => text.Contains(item, StringComparison.OrdinalIgnoreCase));
        }

        // If it's just a normal string (e.g., on_contains: hello), check just that one
        return text.Contains(Data.ValueString, StringComparison.OrdinalIgnoreCase);
    }
}

public record ConfidenceEvent(Value Data) : IFilterEvent
{
    public bool Matches(string text, float prob)
    {
        float threshold = float.TryParse(Data.ValueString, out var f) ? f : 0;
        return prob < threshold;
    }
}

// --- Actions ---
public class BlockAction : IFilterAction
{
    public FilterResult Execute(string text) => new(true, text);
}

public class ReplaceAction(Value Target, Value Replacement) : IFilterAction
{
    public FilterResult Execute(string text)
    {
        string result = text;
        if (Target.IsList)
        {
            for (int i = 0; i < Target.Items.Count; i++)
            {
                string find = Target.Items[i];
                string replaceWith = (Replacement.IsList && i < Replacement.Items.Count)
                    ? Replacement.Items[i]
                    : Replacement.ValueString;

                result = result.Replace(find, replaceWith, StringComparison.OrdinalIgnoreCase);
            }
        }
        else
        {
            result = result.Replace(Target.ValueString, Replacement.ValueString, StringComparison.OrdinalIgnoreCase);
        }
        return new FilterResult(false, result.Trim());
    }
}

public static class ConfigurationMapper
{
    public static FilteringSettings Map(Dictionary<string, Value> data) => new()
    {
        ConfigName = Get(data, "name", "Unnamed"),
        ConfigAuthor = Get(data, "author", "Unknown"),
        ConfigDescription = Get(data, "description", "Unknown"),
        SttModel = Enum.TryParse<GgmlType>(Get(data, "stt_model", "TinyEn"), true, out var m) ? m : GgmlType.TinyEn,
        TtsVoice = Get(data, "tts_voice", "Microsoft Zira Desktop"),
        SpeakingRate = int.TryParse(Get(data, "speaking_rate", "0"), out var r) ? r : 0,
        MinVolume = float.TryParse(Get(data, "min_volume", "0.02"), out var v) ? v : 0.02f,
        SilenceWait = int.TryParse(Get(data, "silence_wait", "600"), out var s) ? s : 600,
        MinConfidence = float.TryParse(Get(data, "min_confidence", "0.45"), out var c) ? c : 0.45f,
        MinShortness = int.TryParse(Get(data, "min_shortness", "8000"), out var sh) ? sh : 8000
    };

    private static string Get(Dictionary<string, Value> d, string k, string def)
        => d.TryGetValue(k, out var v) ? v.ValueString : def;
}

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