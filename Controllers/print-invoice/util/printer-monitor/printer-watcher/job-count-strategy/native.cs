using System.Runtime.InteropServices;

namespace TemplatePrinting.Controllers;

public class NativeJobCountStrategy : IJobCountStrategy {
  [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
  static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

  [DllImport("winspool.drv", SetLastError = true)]
  static extern bool GetPrinter(IntPtr hPrinter, uint level, IntPtr pPrinter, uint cbBuf, out uint pcbNeeded);

  [DllImport("winspool.drv", SetLastError = true)]
  static extern bool ClosePrinter(IntPtr hPrinter);

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  struct PRINTER_INFO_2 {
    public IntPtr pServerName;
    public IntPtr pPrinterName;
    public IntPtr pShareName;
    public IntPtr pPortName;
    public IntPtr pDriverName;
    public IntPtr pComment;
    public IntPtr pLocation;
    public IntPtr pDevMode;
    public IntPtr pSepFile;
    public IntPtr pPrintProcessor;
    public IntPtr pDatatype;
    public IntPtr pParameters;
    public IntPtr pSecurityDescriptor;
    public uint Attributes;
    public uint Priority;
    public uint DefaultPriority;
    public uint StartTime;
    public uint UntilTime;
    public uint Status;
    public uint cJobs; // This is our target
    public uint AveragePPM;
  }

  public int GetJobCount(string printerName) {
    if (!OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero)) return 0;
    try {
      GetPrinter(hPrinter, 2, IntPtr.Zero, 0, out uint cbNeeded);
      IntPtr pAddr = Marshal.AllocHGlobal((int)cbNeeded);
      if (GetPrinter(hPrinter, 2, pAddr, cbNeeded, out _)) {
        var info = Marshal.PtrToStructure<PRINTER_INFO_2>(pAddr);
        return (int)info.cJobs;
      }
      return 0;
    } finally {
      ClosePrinter(hPrinter);
    }
  }
}