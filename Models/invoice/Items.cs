namespace TemplatePrinting.Models.Invoice;
public partial class Invoice {

  public List<Item> Items { get; set; } = new List<Item>();
  public string? ItemsCountInfo {
    get {
      if (Items == null || Items!.Count == 0)
        return null;
      return $"مجموع الأصناف [ {SumCount(Items)} ]";
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
            entries.Add(new Entry() { Title = $" — {lines[i]} —", });
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
      return $"مجموع الأصناف المعدلة [ {SumCount(EditedItems)} ]";
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
            entries.Add(new Entry() { Title = $" — {lines[i]} —", });
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
      return $"مجموع الأصناف الباقية [ {SumCount(OtherKitchensItems)} ]";
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
}
