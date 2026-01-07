
using PrintingLibrary.EmbeddedResourcesUtils;
using TemplatePrinting.Models;

namespace TemplatePrinting.services {
  public class DataService {
    public readonly Dictionary<Asset, PrintStampInfo> Assets;

    public DataService(IWebHostEnvironment @hostEnvironment) {
      var assetsInfoPath = Path.Combine(hostEnvironment.ContentRootPath, "Assets", "assets_info.json");
      var assetsJson = File.ReadAllText(assetsInfoPath);
      var assetData = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetInfoJson>(assetsJson) ?? new AssetInfoJson();

      var resourcesDict = new Dictionary<Asset, PrintStampInfo>();
      foreach (var kvp in assetData.Assets) {
        if (Enum.TryParse<Asset>(kvp.Key.Replace("_", ""), true, out var assetEnum)) {
          resourcesDict[assetEnum] = new PrintStampInfo {
            Path = kvp.Value.Path,
            Width = kvp.Value.Width,
            Height = kvp.Value.Height
          };
        }
      }
      Assets = resourcesDict;
    }

    public PrintStampInfo GetAssetInfo(Asset asset) {
      return Assets[asset];
    }

    public Dictionary<Asset, IHasPath> GetAssets() {
      return Assets.ToDictionary(kvp => kvp.Key, kvp => (IHasPath)kvp.Value);
    }
  }
}