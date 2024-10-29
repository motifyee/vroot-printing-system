
namespace TemplatePrinting.Models.Invoice;
public partial class Invoice {

  // uses reflection to retrieve a self-value by name
  private string? GetValue(string propertyName) {
    Type type = typeof(Invoice);
    System.Reflection.PropertyInfo? info = type.GetProperty(propertyName);
    return (string?)info?.GetValue(this, null)?.ToString();
  }

  private int parseint(string str) {
    int val = 0;
    Int32.TryParse(str, out val);
    return val;
  }

  private decimal parseDecimal(string str) {
    decimal val = 0M;
    Decimal.TryParse(str, out val);
    return val;
  }

  private int sumCount(List<Item> list) {
    // return list.Aggregate(0, (p, c) => p + parseint(c.Count ?? ""));
    int res = 0;
    foreach (var item in list)
      res += parseint(item.Count ?? "0");

    return res;
  }

  private decimal sumTotal(List<Item> list) {
    decimal res = 0M;
    foreach (var item in list)
      res += parseDecimal(item.TotalPrice ?? "0.0");

    return res;
  }

  private List<string> SplitLongValueLines(string? value) {
    if (value == null || value.Trim().Length == 0)
      return new List<string> { "" };
    var line = "";
    var lines = new List<string>();
    var _value = value.Replace(Environment.NewLine, " — ").Replace("\n", " — ");
    // Console.WriteLine(_value);
    // string[] nm = words.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    foreach (var wd in _value.Split(" ")) {
      if (!wd.Contains("—") && line.Length + wd.Length <= _maxLineLength) {
        line += $" {wd.Trim()}";
      } else {
        lines.Add(line.Trim());
        line = wd.Replace("—", "").Trim();
      }
    }
      ;
    if (line.Trim().Length > 0)
      lines.Add(line.Trim());

    return lines;
  }

  // converts a some properties to an iterable for MiniExcel
  private List<Entry> GetPropertyListEntries(
      Dictionary<string, string> properties,
      List<string> multilineProperties
  ) {
    var entries = new List<Entry>();

    foreach (KeyValuePair<string, string> prop in properties) {
      var value = GetValue(prop.Key);
      if (value == null || value.Trim().Length == 0)
        continue;

      if (!multilineProperties.Contains(prop.Key)) {
        entries.Add(
            new Entry() {
              Title = prop.Value,
              Value = value,
              // Values = new List<string?> { value }
            }
        );
        continue;
      }

      var lines = SplitLongValueLines(value);

      // TODO_BESAFE if sent is null
      for (var i = 0; i < lines.Count; i++) {
        entries.Add(
            new Entry() {
              Title = i == 0 ? prop.Value : null,
              Value = lines[i],
              // Values = new List<string?> { lines[i] }
            }
        );
      }
    }

    return entries;
  }

}