namespace TemplatePrinting.Models;

public class Entry {
  // [JsonConverter(typeof(StringConverter))]
  public string? Title { get; set; }

  // public List<string?> Values { get; set; } = new List<string?>();
  public string? Value { get; set; }
  public string? Value2 { get; set; }
  public string? Value3 { get; set; }
}

