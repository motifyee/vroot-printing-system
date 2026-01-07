
using PuppeteerSharp;

namespace PrintingLibrary.Setup;

public interface IPrintingSetup {
  /// <summary>
  /// Load spire license key and setup browser
  /// </summary>
  void Setup();
  /// <summary>
  /// Settings for printing
  /// loaded from printing-settings.json
  /// </summary>
  PrintingSettings Settings { get; }
  /// <summary>
  /// Secret used to generate hash for print stamp image
  /// </summary>
  string? PrintStampSecret { get; }
  /// <summary>
  /// Secret used to encrypt template output
  /// </summary>
  string? TemplateEncOutputSecret { get; }
  /// <summary>
  /// Password used to encrypt generated Excel files
  /// </summary>
  string? EncryptionPassword { get; }
}