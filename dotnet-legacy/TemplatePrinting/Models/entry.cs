namespace TemplatePrinting.Models;

public class Entry {
  // [JsonConverter(typeof(StringConverter))]
  public string? Title { get; set; }

  public string? Value { get; set; }
}

