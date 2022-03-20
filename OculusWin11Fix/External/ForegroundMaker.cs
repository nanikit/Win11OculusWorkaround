namespace OculusWin11Fix.External {
  using OculusWin11Fix.Core;
  using OculusWin11Fix.Services;
  using System;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.InteropServices;
  using static PInvoke.User32;
  using IPALogger = IPA.Logging.Logger;

  public class ForegroundMaker : IForegroundMaker, IDisposable {

    public ForegroundMaker(IPALogger logger, InterestWindowEnumerator finder) {
      _logger = logger;
      _windowEnumerator = finder;
    }

    public bool MakeForeground() {
      return ScanAndApply(MakeTopmost);
    }

    public bool MakeBackground() {
      return true;
    }

    public void Dispose() {
      ScanAndApply(ReleaseTopmost);
    }

    private static readonly SetWindowPosFlags _commonPosFlags =
      SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE;
    private readonly IPALogger _logger;
    private readonly InterestWindowEnumerator _windowEnumerator;
    private readonly int _currentPid = Process.GetCurrentProcess().Id;

    private bool ScanAndApply(Func<IntPtr, string, bool> action) {
      var windows = _windowEnumerator.Enumerate();
      if (windows == null) {
        return false;
      }

      var processToWindows = windows
        .Where(x => !x.Process.ProcessName.ToLower().StartsWith("explorer") && IsNotTrivialTitle(x.Title))
        .GroupBy(x => x.Process)
        .OrderBy(x => GetProcessPriority(x.Key));

      foreach (var group in processToWindows) {
        string name = group.Key.ProcessName;
        _logger.Trace($"{name} -> {string.Join(", ", group.Select(x => x.Title))}");
        bool isCurrentProcess = group.Key.Id == _currentPid;
        if (name == Target.OVRServer.GetProcessName() || isCurrentProcess
          || name == "vrmonitor" || name == "VirtualMotionCapture" || name.Contains("obs64")) {
          action(group.First().Handle, name);
        }
      }

      _logger.Info("Window scan finished");
      return true;
    }

    private int GetProcessPriority(Process process) {
      if (process.Id == _currentPid) {
        return 0;
      }
      if (process.ProcessName == Target.OVRServer.GetProcessName()) {
        return 2;
      }
      return 1;
    }

    private bool MakeTopmost(IntPtr windowHandle, string name) {
      ShowWindow(windowHandle, WindowShowStyle.SW_RESTORE);
      SetForegroundWindow(windowHandle);
      if (!SetWindowPos(windowHandle, SpecialWindowHandles.HWND_TOPMOST,
        0, 0, 0, 0, _commonPosFlags | SetWindowPosFlags.SWP_SHOWWINDOW)
      ) {
        _logger.Warn($"{name} SetWindowPos error {Marshal.GetLastWin32Error()}");
        return false;
      }

      _logger.Debug($"{name} MakeForeground success.");
      return true;
    }

    private bool ReleaseTopmost(IntPtr windowHandle, string name) {
      if (!SetWindowPos(windowHandle, SpecialWindowHandles.HWND_NOTOPMOST,
        0, 0, 0, 0, _commonPosFlags | SetWindowPosFlags.SWP_NOACTIVATE)
      ) {
        _logger.Warn($"{name} SetWindowPos error {Marshal.GetLastWin32Error()}");
        return false;
      }

      _logger.Info($"{name} MakeBackground success.");
      return true;
    }
    private static bool IsNotTrivialTitle(string title) {
      bool isTrivial = title == "Default IME" || title == "MSCTFIME UI";
      return !isTrivial;
    }
  }

  enum Target {
    OVRServer,
    Steam,
  };

  static class TargetExtension {
    public static string GetProcessName(this Target target) => target switch {
      Target.OVRServer => "OVRServer_x64",
      Target.Steam => "steam",
      _ => "",
    };

    public static string GetName(this Target target) => target switch {
      Target.OVRServer => "OVRServer",
      Target.Steam => "Steam",
      _ => "",
    };
  }
}
