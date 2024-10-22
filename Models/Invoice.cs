using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrintingApi {

  public class Invoice {//: IParsable<Invoice>
    private const int MaxLineLength = 35;
    public int? Status { get; set; }
    public string? PrinterName { get; set; }
    public string? TemplateName { get; set; }
    public bool GlobalPrinter { get; set; } = false;

    public string? Company { get; set; }
    public string? Cashier { get; set; }
    public string? Branch { get; set; }
    public string? BranchDesc { get; set; }


    public string? Date { get; set; }
    public string? Time { get; set; }

    public string? InvoiceNo { get; set; }

    public string? InvoiceType { get; set; }


    public string? ClientName { get; set; }
    public string? ClientPhone1 { get; set; }
    public string? ClientPhone2 { get; set; }
    public string? ClientAddress { get; set; }
    public string? ClientArea { get; set; }


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
        if (!wd.Contains("—") && line.Length + wd.Length <= MaxLineLength) {
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

    public List<Entry> ClientInfo {
      get {
        var properties = new Dictionary<string, string>()
        {
                    { "ClientName", "عميل" },
                    { "ClientPhone1", "موبايل" },
                    { "ClientPhone2", "موبايل آخر" },
                    { "ClientAddress", "عنوان" },
                    { "ClientArea", "منطقة" }
                };

        return GetPropertyListEntries(properties, new List<string> { "ClientAddress" });
      }
    }

    public string? DeliveryName { get; set; }
    public string? DeliveryNameTitle {
      get { return (DeliveryName != null && DeliveryName?.Length > 0) ? "طيار" : null; }
    }

    public List<Item> Items { get; set; } = new List<Item>();
    public string? ItemsCountInfo {
      get {
        if (Items == null || Items!.Count == 0)
          return null;
        return $"مجموع الأصناف [ {sumCount(Items)} ]";
      }
    }

    public List<Entry> ItemsInfo {
      get {
        var entries = new List<Entry>();

        foreach (var item in Items) {
          var lines = SplitLongValueLines(item?.Title ?? "");

          // TODO_BESAFE if sent is null
          for (var i = 0; i < lines.Count; i++)
            entries.Add(
                new Entry() {
                  Title = lines[i],
                  Value = i == 0 ? item?.Count : null,
                  // Values = new List<string?> { lines[i] }
                }
            );

          if (item?.Note != null && item?.Note?.Trim().Length > 0) {
            lines = SplitLongValueLines(item?.Note ?? "");
            for (var i = 0; i < lines.Count; i++)
              entries.Add(new Entry() { Title = $" —{lines[i]}—", });
          }
        }

        return entries;
      }
    }

    public List<Item> EditedItems { get; set; } = new List<Item>();
    public string? EditedItemsCountInfo {
      get {
        if (EditedItems == null || EditedItems!.Count == 0)
          return null;
        return $"مجموع الأصناف المعدلة [ {sumCount(EditedItems)} ]";
      }
    }
    public string? EditedTitle {
      get { return (EditedItems != null && EditedItems?.Count > 0) ? "تعديل" : null; }
    }
    public List<Entry> EditedItemsInfo {
      get {
        var entries = new List<Entry>();

        foreach (var item in EditedItems) {
          var lines = SplitLongValueLines(item?.Title ?? "");
          for (var i = 0; i < lines.Count; i++)
            entries.Add(
                new Entry() { Title = lines[i], Value = i == 0 ? item?.Count : null, }
            );

          if (item?.Note != null && item?.Note?.Trim().Length > 0) {
            lines = SplitLongValueLines(item?.Note ?? "");
            for (var i = 0; i < lines.Count; i++)
              entries.Add(new Entry() { Title = $" —{lines[i]}—", });
          }
        }

        return entries;
      }
    }

    public List<Item> OtherKitchensItems { get; set; } = new List<Item>();
    public string? OtherKitchensItemsCountInfo {
      get {
        if (OtherKitchensItems == null || OtherKitchensItems!.Count == 0)
          return null;
        return $"مجموع الأصناف الباقية [ {sumCount(OtherKitchensItems)} ]";
      }
    }
    public List<Entry> OtherKitchensItemsInfo {
      get {
        var entries = new List<Entry>();

        foreach (var item in OtherKitchensItems) {
          var lines = SplitLongValueLines(item?.Title ?? "");

          for (var i = 0; i < lines.Count; i++)
            entries.Add(
                new Entry() { Title = lines[i], Value = i == 0 ? item?.Count : null, }
            );
        }

        return entries;
      }
    }
    public List<string> OtherKitchens { get; set; } = new List<string>();
    public string? OtherKitchensTitle {
      get {
        if (OtherKitchens == null || OtherKitchens?.Count == 0)
          return null;
        if (OtherKitchensItems == null || OtherKitchensItems?.Count == 0)
          return null;
        string kitchenNames = String.Join("و ", OtherKitchens!.ToArray());
        return $"يوجد بقية للطلب في [ {kitchenNames} ]";
      }
    }
    public List<Entry> OtherKitchensInfo {
      get {
        var ret = new List<Entry>();

        if (OtherKitchensTitle == null || OtherKitchensTitle?.Length == 0)
          return ret;
        if (OtherKitchens == null || OtherKitchens?.Count == 0)
          return ret;

        foreach (var item in SplitLongValueLines(OtherKitchensTitle!)) {
          ret.Add(new Entry() { Title = item, });
        }

        return ret;
      }
    }

    public string? SectionName { get; set; }
    public string? ScheduleTime { get; set; }
    public string? ScheduleTitle {
      get { return (ScheduleTime != null && ScheduleTime?.Length > 0) ? "حجز" : null; }
    }

    public string? TableNo { get; set; }
    public string? TableTitle {
      get { return (TableNo != null && TableNo?.Length > 0) ? "طاولة" : null; }
    }

    public string? Discount { get; set; }
    public string? Service { get; set; }
    public string? Delivery { get; set; }
    public string? Vat { get; set; }
    public string? Visa { get; set; }
    public string? VisaPer { get; set; }
    public string? VisaPerTitle {
      get { return (VisaPer != null && VisaPer?.Length > 0) ? $"Visa {VisaPer} %" : null; }
    }
    public string? Total { get; set; }
    public decimal ItemsTotal {
      get { return sumTotal(Items); }
    }
    public decimal ItemsTax14 {
      get { return Decimal.Multiply(ItemsTotal, 0.14M); }
    }

    public List<Entry> InvoiceInfo {
      get {
        var properties = new Dictionary<string, string>()
        {
                    { "Discount", "الخصم" },
                    { "Service", "Service" },
                    { "Delivery", "خدمة توصيل" },
                    { "Vat", "Vat 14%" },
                    { "Visa", VisaPerTitle ?? "" },
                    { "Total", "اجمالي" }
                };
        return GetPropertyListEntries(properties, new List<string> { "ClientAddress" });
      }
    }
    public string? Note { get; set; }
    public string? PrintingDate { get; set; }
    public string? PrintingTime { get; set; }

    // public List<string?> FooterNotes { get; set; } = new List<string?>();
    public string? FooterNote1 { get; set; }
    public string? FooterNote2 { get; set; }
    public string? FooterNote3 { get; set; }
  }

  internal class DateTimeConverter : JsonConverter<DateTime> {
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => JsonSerializer.Deserialize<DateTime>(ref reader, options);

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(value.ToString()); //writer.WriteStringValue(value);
  }

  public class Entry {
    // [JsonConverter(typeof(StringConverter))]
    public string? Title { get; set; }

    // public List<string?> Values { get; set; } = new List<string?>();
    public string? Value { get; set; }
    public string? Value2 { get; set; }
    public string? Value3 { get; set; }
  }

  public class Item {
    // [JsonConverter(typeof(StringConverter))]
    public string? Title { get; set; }
    public string? Count { get; set; }
    public string? Price { get; set; }
    public string? TotalPrice { get; set; }
    public string? Note { get; set; }
  }
}
