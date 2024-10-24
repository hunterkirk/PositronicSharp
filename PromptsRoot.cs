using System.Collections.Generic;

public class Prompt
{
    public string PromptText { get; set; } // Renamed to match the property in YAML
    public string FileExtension { get; set; } // Matches 'file_extension'
}

public class PromptsRoot
{
    public List<Prompt> Prompts { get; set; } // Should match the root key 'prompts'
}