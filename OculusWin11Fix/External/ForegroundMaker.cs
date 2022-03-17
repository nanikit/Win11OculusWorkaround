namespace OculusWin11Fix.External {
  using OculusWin11Fix.Core;
  using OculusWin11Fix.Services;
  using PInvoke;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.InteropServices;
  using static PInvoke.User32;
  using IPALogger = IPA.Logging.Logger;

  public class ForegroundMaker : IForegroundMaker, IDisposable {

    public ForegroundMaker(IPALogger logger, WindowEnumerator finder) {
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
    private readonly WindowEnumerator _windowEnumerator;
    private readonly List<IntPtr> _accessDenieds = new();
    private readonly List<IntPtr> _mainWindows = new();

    private bool ScanAndApply(Func<IntPtr, string, bool> action) {
      _accessDenieds.Clear();
      _mainWindows.Clear();

      bool isSuccess = _windowEnumerator.Enumerate(RecordWindow);
      if (!isSuccess && Marshal.GetLastWin32Error() != 1300) {
        _logger.Warn($"EnumWindows failed: {Marshal.GetLastWin32Error()}.");
        return false;
      }

      if (_accessDenieds.Count != 0) {
        _logger.Trace($"Access denied {_accessDenieds.Count} windows.");
      }

      var current = Process.GetCurrentProcess();
      _logger.Debug($"Current process ID: {current.Id}");
      var processToWindows = _mainWindows.GroupBy(handle => {
        GetWindowThreadProcessId(handle, out int processId);
        try {
          var process = Process.GetProcessById(processId);
          return process;
        }
        catch {
          return null;
        }
      }).OrderBy(x => (x.Key?.Id) != current.Id);

      foreach (var process in processToWindows) {
        if (process.Key == null) {
          continue;
        }

        _logger.Debug($"{process.Key} -> {string.Join(", ", process.Select(GetWindowText))}");
        string name = process.Key.ProcessName;
        bool isCurrentProcess = process.Key.Id == current.Id;
        if (name == Target.OVRServer.GetProcessName() || isCurrentProcess) {
          action(process.First(), name);
        }
        else if (name == "vrmonitor") {
          action(process.First(), name);
        }
        else if (name.Contains("VirtualMotionCapture") || name.Contains("obs64")) {
          action(process.First(), name);
        }
      }

      _logger.Info("Scan finished");
      return true;
    }


    private bool RecordWindow(IntPtr windowHandle) {
      try {
        bool isTopLevel = GetWindow(windowHandle, GetWindowCommands.GW_OWNER) == IntPtr.Zero;
        if (!isTopLevel) {
          return true;
        }
        if (!IsWindowVisible(windowHandle)) {
          return true;
        }

        // Access can be denied.
        GetWindowText(windowHandle);

        _mainWindows.Add(windowHandle);
        return true;
      }
      catch (Win32Exception exception) when (exception.ErrorCode == (HResult)0x80004005) {
        _accessDenieds.Add(windowHandle);
        return true;
      }
      catch (Exception exception) {
        _logger.Error(exception);
        return true;
      }
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

      _logger.Info($"{name} MakeForeground success.");
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
