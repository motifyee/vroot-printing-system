using System.Management;

namespace TemplatePrinting.Controllers;

public class ManagedJobCountStrategy : IJobCountStrategy {
  public int GetJobCount(string printerName) {
    try {
      var query = new WqlObjectQuery($"SELECT * FROM Win32_PrintJob WHERE Name LIKE '%{printerName}%'");
      using (var searcher = new ManagementObjectSearcher(query)) {
        var collection = searcher.Get();
        return collection.Count;
      }
    } catch {
      return 0;
    }
  }
}