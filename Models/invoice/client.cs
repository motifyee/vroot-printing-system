namespace TemplatePrinting.Models.Invoice;

public partial class Invoice {
  public string? ClientName { get; set; }
  public string? ClientPhone1 { get; set; }
  public string? ClientPhone2 { get; set; }
  public string? ClientAddress { get; set; }
  public string? ClientArea { get; set; }

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

}
