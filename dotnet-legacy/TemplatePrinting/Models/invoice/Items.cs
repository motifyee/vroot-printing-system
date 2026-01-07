using PrintingLibrary.Setup;

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

  public List<Item> ItemsInfo {
    get {
      var util = new PrintingSetup();
      var printNote = util.Settings.PrintItemInfoNote;

      var entries = new List<Item>();

      foreach (var item in Items) {
        var lines = SplitLongValueLines(item?.Title ?? "");

        // TODO_BESAFE if sent is null
        for (var i = 0; i < lines.Count; i++)
          entries.Add(
              new Item() {
                Title = lines[i],
                Value = i == 0 ? item?.Count : null,
                Count = i == 0 ? item?.Count : null,
                Price = i == 0 ? item?.Price : null,
                TotalPrice = i == 0 ? item?.TotalPrice : null,
              }
          );

        if (printNote && item?.Note != null && item?.Note?.Trim().Length > 0) {
          lines = SplitLongValueLines(item?.Note ?? "");
          for (var i = 0; i < lines.Count; i++)
            entries.Add(new Item() { Title = $" — {lines[i]} —", });
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
  public List<Item> EditedItemsInfo {
    get {
      var entries = new List<Item>();

      foreach (var item in EditedItems) {
        var lines = SplitLongValueLines(item?.Title ?? "");
        for (var i = 0; i < lines.Count; i++)
          entries.Add(
              new Item() { Title = lines[i], Value = i == 0 ? item?.Count : null, }
          );

        if (item?.Note != null && item?.Note?.Trim().Length > 0) {
          lines = SplitLongValueLines(item?.Note ?? "");
          for (var i = 0; i < lines.Count; i++)
            entries.Add(new Item() { Title = $" — {lines[i]} —", });
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
  public List<Item> OtherKitchensItemsInfo {
    get {
      var entries = new List<Item>();

      foreach (var item in OtherKitchensItems) {
        var lines = SplitLongValueLines(item?.Title ?? "");

        for (var i = 0; i < lines.Count; i++)
          entries.Add(
              new Item() { Title = lines[i], Value = i == 0 ? item?.Count : null, }
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
  public List<Item> OtherKitchensInfo {
    get {
      var ret = new List<Item>();

      if (OtherKitchensTitle == null || OtherKitchensTitle?.Length == 0)
        return ret;
      if (OtherKitchens == null || OtherKitchens?.Count == 0)
        return ret;

      foreach (var item in SplitLongValueLines(OtherKitchensTitle!)) {
        ret.Add(new Item() { Title = item, });
      }

      return ret;
    }
  }
}
