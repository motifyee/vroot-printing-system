using System.ComponentModel.DataAnnotations;
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

public interface IHasPath
{
  string? Path { get; }
}

public class Resources<T>(Dictionary<T, IHasPath> resources) where T : struct, Enum
{
  public Stream? GetStream(T path)
  {
    var assembly = typeof(T).Assembly;
    // var field = path.GetType().GetField(path.ToString());
    // var attribute = field?.GetCustomAttribute<DisplayAttribute>();
    // var name = attribute?.Name ?? path.ToString();
    var name = resources[path].Path;
    var resourceName = $"{assembly.GetName().Name}.{name?.Replace('/', '.')}";
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

  public IHasPath GetInfo(T path) => resources[path];
}

