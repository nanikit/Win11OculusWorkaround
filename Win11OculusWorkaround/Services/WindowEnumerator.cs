namespace Win11OculusWorkaround.Services {
  using PInvoke;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.InteropServices;
  using static PInvoke.User32;
  using IPALogger = IPA.Logging.Logger;

  public class WindowEnumerator {
    public WindowEnumerator() {
      _predicate = new(Save);
    }

    public List<IntPtr> Enumerate() {
      _results = new();
      SetLastErrorEx(0, 0);
      EnumWindows(_predicate, IntPtr.Zero);
      return _results;
    }

    private readonly WNDENUMPROC _predicate;
    private List<IntPtr> _results = new();

    private bool Save(IntPtr windowHandle, IntPtr lParam) {
      _results.Add(windowHandle);
      return true;
    }
  }

  public class InterestWindowEnumerator {
    public record RemarkableWindow(IntPtr Handle, string Title, Process Process);

    public InterestWindowEnumerator(IPALogger logger) {
      _logger = logger;
    }

    public List<RemarkableWindow>? Enumerate() {
      var mainWindows = _enumerator.Enumerate();
      int error = Marshal.GetLastWin32Error();
      if (error != 0) {
        _logger.Info($"EnumWindows error: {Marshal.GetLastWin32Error()}.");
      }

      (var windows, var accessDenieds) = mainWindows.Aggregate((new List<RemarkableWindow>(), new List<IntPtr>()), PartitionAccessible);
      if (accessDenieds.Count != 0) {
        _logger.Trace($"Access denied {accessDenieds.Count} windows.");
      }

      return windows;
    }

    private readonly IPALogger _logger;
    private readonly WindowEnumerator _enumerator = new();

    private (List<RemarkableWindow>, List<IntPtr>) PartitionAccessible((List<RemarkableWindow>, List<IntPtr>) accumulated, IntPtr windowHandle) {
      string? title = null;
      try {
        if (IsWindowVisible(windowHandle)) {
          title = GetWindowText(windowHandle);
          GetWindowThreadProcessId(windowHandle, out int processId);
          var process = Process.GetProcessById(processId);
          accumulated.Item1.Add(new(windowHandle, title, process));
        }
      }
      catch (Win32Exception getWindowTextException) when (getWindowTextException.ErrorCode == (HResult)0x80004005) {
        accumulated.Item2.Add(windowHandle);
      }
      // admin procexp
      catch (ArgumentException getProcessException) when (getProcessException.HResult == (HResult)0x80070057) {
        accumulated.Item2.Add(windowHandle);
      }
      catch (Exception exception) {
        _logger.Error($"window title: {title}, {exception}");
      }
      return accumulated;
    }
  }
}
