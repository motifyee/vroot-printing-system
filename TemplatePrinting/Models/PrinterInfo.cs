namespace TemplatePrinting.Models;

public class PrinterInfo {
  public string Name { get; set; } = string.Empty;
  public string FullName { get; set; } = string.Empty;
  public bool IsDefault { get; set; }
  public bool IsValid { get; set; }
  public bool IsOffline { get; set; }
  public bool IsNetwork { get; set; }
  public bool IsShared { get; set; }
  public string ShareName { get; set; } = string.Empty;
  public string DriverName { get; set; } = string.Empty;
  public string PortName { get; set; } = string.Empty;
  public string Location { get; set; } = string.Empty;
  public int JobCount { get; set; }
  public string Status { get; set; } = string.Empty;
  public bool SupportsColor { get; set; }
  public int MaxCopies { get; set; }
  public string Duplex { get; set; } = string.Empty;
  public List<PaperSizeInfo> PaperSizes { get; set; } = [];
  public PaperSizeInfo? DefaultPaperSize { get; set; }
  public List<string> Resolutions { get; set; } = [];
}

public class PaperSizeInfo {
  public string Name { get; set; } = string.Empty;
  public int Width { get; set; }
  public int Height { get; set; }
  public string Kind { get; set; } = string.Empty;
  public int RawKind { get; set; }
}
