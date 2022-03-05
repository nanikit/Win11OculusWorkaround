namespace OculusWin11Fix.External {
  using OculusWin11Fix.Core;
  using OculusWin11Fix.Services;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.InteropServices;
  using static PInvoke.User32;
  using IPALogger = IPA.Logging.Logger;

  public class ForegroundMaker : IForegroundMaker {
    public const string ProcessName = "OVRServer_x64";

    public ForegroundMaker(IPALogger logger, MainWindowFinder finder) {
      _logger = logger;
      _mainWindowFinder = finder;
    }

    public bool MakeForeground() {
      IntPtr windowHandle = GetOvrConsoleHandle();
      if (windowHandle == IntPtr.Zero) {
        return false;
      }

      if (!SetWindowPos(windowHandle, SpecialWindowHandles.HWND_TOPMOST,
        0, 0, 0, 0, _commonPosFlags | SetWindowPosFlags.SWP_SHOWWINDOW)
      ) {
        _logger.Warn($"SetWindowPos error {Marshal.GetLastWin32Error()}");
        return false;
      }

      _logger.Info($"MakeForeground success");
      return true;
    }

    public bool MakeBackground() {
      IntPtr windowHandle = GetOvrConsoleHandle();
      if (windowHandle == IntPtr.Zero) {
        return false;
      }

      if (!SetWindowPos(windowHandle, SpecialWindowHandles.HWND_NOTOPMOST,
        0, 0, 0, 0, _commonPosFlags | SetWindowPosFlags.SWP_NOACTIVATE)
      ) {
        _logger.Warn($"SetWindowPos error {Marshal.GetLastWin32Error()}");
        return false;
      }

      _logger.Info($"MakeBackground success");
      return true;
    }

    private static readonly SetWindowPosFlags _commonPosFlags = SetWindowPosFlags.SWP_NOMOVE
      | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_ASYNCWINDOWPOS;
    private readonly IPALogger _logger;
    private readonly MainWindowFinder _mainWindowFinder;

    private IntPtr GetOvrConsoleHandle() {
      Process? ovrServer = GetOvrServerProcess();
      if (ovrServer == null) {
        return IntPtr.Zero;
      }

      // We can't use Process.MainWindowHandle here because Unity doesn't support it.
      return _mainWindowFinder.GetMainWindowHandle(ovrServer.Id);
    }

    private Process? GetOvrServerProcess() {
      var processes = FindProcessByName(ProcessName).ToList();
      if (processes.Count == 0) {
        string list = Process.GetProcesses().Select(p => $"{p.ProcessName}\t{p.Id}").Aggregate((a, b) => $"{a}\n{b}");
        _logger.Warn($"No action due to no OVRServer console window is found.");
        _logger.Trace(list);
        return null;
      }
      if (processes.Count > 1) {
        string list = processes.Select(p => $"{p.ProcessName}\t{p.Id}").Aggregate((a, b) => $"{a}, {b}");
        _logger.Warn($"There are multiple processes targeted: {list}");
      }
      return processes[0];
    }

    private static IEnumerable<Process> FindProcessByName(string name) {
      return Process.GetProcesses().Where(p => p.ProcessName == name);
    }
  }
}
