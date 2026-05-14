using System.Text.RegularExpressions;

namespace StS_Demo;

public class FilterEngine
{
    private readonly List<FilterRule> _rules = [];
    public VrxSettings Settings { get; private set; } = new();

    public void LoadConfig(string filePath)
    {
        _rules.Clear();
        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;
            
            if (trimmed.Contains(':') && !IsCommand(trimmed))
            {
                ParseSetting(trimmed);
            }
            else
            {
                var rule = ParseRule(trimmed);
                if (rule != null) _rules.Add(rule);
            }
        }
    }

    private static bool IsCommand(string line)
    {
        var upper = line.ToUpper();
        return upper.Contains("CONTAINS:") || upper.Contains("EQUALS:") ||
               upper.Contains("STARTS:") || upper.Contains("ENDS:") || upper.Contains("REGEX:");
    }

    private void ParseSetting(string line)
    {
        var parts = line.Split(':', 2);
        if (parts.Length < 2) return;

        var key = parts[0].Trim().ToUpper();
        var val = parts[1].Trim();

        switch (key)
        {
            case "NAME": Settings.Name = val; break;
            case "COMMENT": Settings.Comment = val; break;
            case "AUTHOR": Settings.Author = val; break;
            case "CONFIDENCE": Settings.Confidence = int.Parse(val) / 100f; break;
            case "RATE": Settings.Rate = int.Parse(val); break;
            case "VOICE": Settings.Voice = val; break;
            case "VOLUME": Settings.Volume = float.Parse(val); break;
            case "SILENCE": Settings.Silence = int.Parse(val); break;
            case "SHORTNESS": Settings.Shortness = int.Parse(val); break;
            case "MODEL": Settings.Model = val; break;
        }
    }

    private static FilterRule? ParseRule(string line)
    {
        var rule = new FilterRule();

        if (line.StartsWith("NOT ", StringComparison.OrdinalIgnoreCase))
        {
            rule.IsInverted = true;
            line = line[4..];
        }

        var parts = line.Split(':', 2);
        if (parts.Length < 2) return null;

        var cmdPart = parts[0].Trim().ToUpper();
        var target = parts[1].Trim();

        if (cmdPart.Contains("BAR"))
        {
            var match = Regex.Match(cmdPart, @"BAR (\d+)");
            if (match.Success) rule.MinConfidence = int.Parse(match.Groups[1].Value) / 100f;
        }

        if (cmdPart.Contains("LIMIT"))
        {
            var match = Regex.Match(cmdPart, @"LIMIT (\d+)");
            if (match.Success) rule.MaxConfidence = int.Parse(match.Groups[1].Value) / 100f;
        }

        if (cmdPart.Contains("CONTAINS")) rule.Command = FilterCommand.CONTAINS;
        else if (cmdPart.Contains("EQUALS")) rule.Command = FilterCommand.EQUALS;
        else if (cmdPart.Contains("STARTS")) rule.Command = FilterCommand.STARTS;
        else if (cmdPart.Contains("ENDS")) rule.Command = FilterCommand.ENDS;
        else if (cmdPart.Contains("REGEX")) rule.Command = FilterCommand.REGEX;

        rule.Target = target.ToLowerInvariant();
        return rule;
    }

    public bool ShouldBlock(string text, float confidence)
    {
        if (confidence < Settings.Confidence) return true;

        string cleanText = text.Trim().ToLowerInvariant();

        foreach (var rule in _rules)
        {
            if (confidence < rule.MinConfidence || confidence > rule.MaxConfidence) continue;

            bool match = rule.Command switch
            {
                FilterCommand.EQUALS => cleanText == rule.Target,
                FilterCommand.CONTAINS => cleanText.Contains(rule.Target),
                FilterCommand.STARTS => cleanText.StartsWith(rule.Target),
                FilterCommand.ENDS => cleanText.EndsWith(rule.Target),
                FilterCommand.REGEX => Regex.IsMatch(cleanText, rule.Target),
                _ => false
            };

            if (rule.IsInverted) match = !match;
            if (match) return true;
        }
        return false;
    }
}

public class VrxSettings
{
    public string Name { get; set; } = "Untitled Profile";
    public string Comment { get; set; } = "";
    public string Author { get; set; } = "Unknown";
    public float Confidence { get; set; } = 0.45f;
    public int Rate { get; set; } = 2;
    public string Voice { get; set; } = "Microsoft Guy Native";
    public float Volume { get; set; } = 0.02f;
    public int Silence { get; set; } = 600;
    public int Shortness { get; set; } = 8000;
    public string Model { get; set; } = "Base";
}

public enum FilterCommand { NONE, EQUALS, CONTAINS, STARTS, ENDS, REGEX }

public class FilterRule
{
    public FilterCommand Command { get; set; } = FilterCommand.NONE;
    public string Target { get; set; } = "";
    public bool IsInverted { get; set; }
    public float MinConfidence { get; set; } = 0f;
    public float MaxConfidence { get; set; } = 1f;
}