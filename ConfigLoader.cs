namespace SqTS;

public static class ConfigLoader
{
    public static List<Value> ParseLines(IEnumerable<string> lines)
    {
        Dictionary<string, Value> keys = [];
        string currentKeyName = string.Empty;
        
        foreach (string line in lines)
        {
            string trimmed = line.TrimStart();
                
            if (trimmed.StartsWith("::") || trimmed.IsWhiteSpace())
                continue;

            if (trimmed.StartsWith('-'))
            {
                if (!keys.TryGetValue(currentKeyName, out Value? currentValue) || !currentValue.IsList)
                    throw new Exception("Syntax error! (Errno 1) Did you forget to put a parent list mode key? (List mode not enabled!)");

                string final = line[1..].Trim();
                currentValue.Items.Add(final);
            }
            else
            {
                string[] parts = trimmed.Split(':',2);

                if (parts.Length != 2)
                    throw new Exception("Syntax error! (Errno 2) Did you forget to put a colon? (Key-Value pair link missing!)");

                currentKeyName = parts[0];
                Value currentValue = new();

                if (parts[1].IsWhiteSpace())
                    currentValue.IsList = true;
                else if (parts[1] == @"\null")
                    currentValue.ValueString = "";
                else
                    currentValue.ValueString = parts[1];

                keys[currentKeyName] = currentValue;
            }
        }

        List<Value> values = [.. keys.Values];
        return values;
    }
}

public class Value
{
    public bool IsList { get; set; } = false;
    public List<string> Items { get; } = [];
    public string ValueString { get; set; } = "";
}