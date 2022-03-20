using System.ComponentModel;

namespace Win11OculusWorkaround.Services {
  using System.IO;
  using System.Text;
  using System.Threading.Tasks;

  internal class Utilities {
    public static async Task<string> ReadAllTextAsync(string path, Encoding? encoding = null) {
      const int fileBufferSize = 4096;
      using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, fileBufferSize, true);
      using var reader = new StreamReader(fileStream, encoding ?? Encoding.UTF8);
      return await reader.ReadToEndAsync().ConfigureAwait(false);
    }
  }
}

namespace System.Runtime.CompilerServices {
  /// <summary>
  /// Workaround for missing record feature.
  /// https://stackoverflow.com/a/62656145
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  internal class IsExternalInit { }
}
