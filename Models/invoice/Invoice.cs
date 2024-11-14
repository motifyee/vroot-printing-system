namespace TemplatePrinting.Models.Invoice {
  public partial class Invoice {
    private const int _maxLineLength = 35;
    public int? Status { get; set; }

    public string? PrinterName { get; set; }
    public string? TemplateName { get; set; }
    public bool? GlobalPrinter { get; set; }

    public string? Company { get; set; }
    public string? Cashier { get; set; }
    public string? Branch { get; set; }
    public string? BranchDesc { get; set; }

    public string? Date { get; set; }
    public string? Time { get; set; }

    public string? ShiftNo { get; set; }

    public string? InvoiceNo { get; set; }

    public string? InvoiceType { get; set; }


    public string? DeliveryName { get; set; }

    public string? DeliveryNameTitle => DeliveryName != null && DeliveryName?.Length > 0 ? "طيار" : null;

    public string? SectionName { get; set; }
    public string? ScheduleTime { get; set; }

    public string? ScheduleTitle => ScheduleTime != null && ScheduleTime?.Length > 0 ? "حجز" : null;

    public string? TableNo { get; set; }

    public string? TableTitle => TableNo != null && TableNo?.Length > 0 ? "طاولة" : null;

    public string? Discount { get; set; }
    public string? Service { get; set; }
    public string? Delivery { get; set; }
    public string? Vat { get; set; }
    public string? Visa { get; set; }
    public string? VisaPer { get; set; }

    public string? VisaPerTitle => VisaPer != null && VisaPer?.Length > 0 ? $"Visa {VisaPer} %" : null;

    public string? Total { get; set; }

    public decimal ItemsTotal => SumTotal(Items);

    public decimal ItemsTax14 => decimal.Multiply(ItemsTotal, 0.14M);

    public List<Entry> InvoiceInfo {
      get {
        Dictionary<string, string> properties = new() {
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
}