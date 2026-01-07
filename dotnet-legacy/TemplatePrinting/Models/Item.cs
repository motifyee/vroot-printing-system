namespace TemplatePrinting.Models;

public class Item {
  // [JsonConverter(typeof(StringConverter))]
  public string? Title { get; set; }
  public string? Value { get; set; }
  public string? Count { get; set; }
  public string? Price { get; set; }
  public string? TotalPrice { get; set; }
  public string? Note { get; set; }
}