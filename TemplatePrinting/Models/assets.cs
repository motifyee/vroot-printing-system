using PrintingLibrary.EmbeddedResourcesUtils;

namespace TemplatePrinting.Models;

public enum Asset {
  PrintStamp,
  PrintStamp72,
  PrintStamp144,
  PrintStamp288,
  PrintStamp512
}

public class PrintStampInfo : IHasPath {
  public string Hash { get; set; } = "";
  public required string Path { get; set; }
  public required int Width { get; set; }
  public required int Height { get; set; }
  public byte[]? ImageBytes { get; set; }
}

public class AssetInfoJson {
  public Dictionary<string, AssetDetailJson> Assets { get; set; } = new();
}

public class AssetDetailJson {
  public string Path { get; set; } = "";
  public int Width { get; set; }
  public int Height { get; set; }
}
