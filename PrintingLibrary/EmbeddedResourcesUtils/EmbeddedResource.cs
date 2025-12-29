using System.Reflection;

namespace PrintingLibrary.EmbeddedResourcesUtils;

internal static class InternalResources
{
  public static Stream? GetStream(string relativePath)
  {
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = $"{assembly.GetName().Name}.{relativePath.Replace('/', '.')}";
    return assembly.GetManifestResourceStream(resourceName);
  }

  public static byte[]? GetBytes(string relativePath)
  {
    using var stream = GetStream(relativePath);
    if (stream == null) return null;

    using MemoryStream ms = new();
    stream.CopyTo(ms);
    return ms.ToArray();
  }

  public static string? GetText(string relativePath)
  {
    using var stream = GetStream(relativePath);
    if (stream == null) return null;

    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
  }

  public static byte[] PrintStamp => GetBytes("Assets.print_stamp.png")!;
}


public class Resources<T>() where T : struct, Enum
{
  public Stream? GetStream(T path)
  {
    var assembly = typeof(T).Assembly;
    var resourceName = $"{assembly.GetName().Name}.{path.ToString().Replace('/', '.')}";
    return assembly.GetManifestResourceStream(resourceName);
  }

  public byte[]? GetBytes(T path)
  {
    using var stream = GetStream(path);
    if (stream == null) return null;

    using MemoryStream ms = new();
    stream.CopyTo(ms);
    return ms.ToArray();
  }

  public string? GetText(T path)
  {
    using var stream = GetStream(path);
    if (stream == null) return null;

    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
  }
}

