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

  public class ForegroundMaker : IForegroundMaker {

    public ForegroundMaker(IPALogger logger, WindowEnumerator finder, bool isSteam) {
      _logger = logger;
      _windowEnumerator = finder;
      _isSteam = isSteam;
      logger.Info($"isSteam: {isSteam}");
    }

    public bool MakeForeground() {
      if (!FindTargetProcessIds()) {
        return false;
      }

      return _windowEnumerator.Enumerate(ShowTargetWindow);
    }

    public bool MakeBackground() {
      if (!FindTargetProcessIds()) {
        return false;
      }

      return _windowEnumerator.Enumerate(HideTargetWindow);
    }

    private static readonly SetWindowPosFlags _commonPosFlags = SetWindowPosFlags.SWP_NOMOVE
      | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_ASYNCWINDOWPOS;
    private readonly IPALogger _logger;
    private readonly WindowEnumerator _windowEnumerator;
    private readonly bool _isSteam;
    private int? _steamProcessId;
    private int? _ovrServerId;

    private bool ShowTargetWindow(IntPtr windowHandle) {
      switch (DetermineWindow(windowHandle)) {
      case Target.OVRServer:
        MakeTopmost(windowHandle, nameof(Target.OVRServer));
        break;
      case Target.Steam:
        MakeTopmost(windowHandle, nameof(Target.Steam));
        break;
      };
      return ShouldEnumeratorContinue();
    }

    private bool HideTargetWindow(IntPtr windowHandle) {
      switch (DetermineWindow(windowHandle)) {
      case Target.OVRServer:
        ReleaseTopmost(windowHandle, nameof(Target.OVRServer));
        break;
      case Target.Steam:
        ReleaseTopmost(windowHandle, nameof(Target.Steam));
        break;
      };
      return ShouldEnumeratorContinue();
    }

    private Target? DetermineWindow(IntPtr windowHandle) {
      bool isTopLevel = GetWindow(windowHandle, GetWindowCommands.GW_OWNER) == IntPtr.Zero;
      if (!isTopLevel) {
        return null;
      }

      GetWindowThreadProcessId(windowHandle, out int processId);
      if (processId == _ovrServerId && IsWindowVisible(windowHandle)) {
        return Target.OVRServer;
      }
      else if (processId == _steamProcessId) {
        try {
          if (GetWindowText(windowHandle) == "Steam") {
            return Target.Steam;
          }
        }
        catch (Win32Exception) {
          // Ignore, I don't know why this occurs.
        }
      }
      return null;
    }

    private bool ShouldEnumeratorContinue() {
      return _ovrServerId != null || _steamProcessId != null;
    }

    private bool MakeTopmost(IntPtr windowHandle, string name) {
      bool isPreviouslyVisible = ShowWindow(windowHandle, WindowShowStyle.SW_RESTORE);
      if (!SetWindowPos(windowHandle, SpecialWindowHandles.HWND_TOPMOST,
        0, 0, 0, 0, _commonPosFlags | SetWindowPosFlags.SWP_SHOWWINDOW)
      ) {
        _logger.Warn($"{name} SetWindowPos error {Marshal.GetLastWin32Error()}");
        return false;
      }

      _logger.Info($"{name} MakeForeground success{(isPreviouslyVisible ? "" : " with restoration")}");
      return true;
    }

    private bool ReleaseTopmost(IntPtr windowHandle, string name) {
      if (!SetWindowPos(windowHandle, SpecialWindowHandles.HWND_NOTOPMOST,
        0, 0, 0, 0, _commonPosFlags | SetWindowPosFlags.SWP_NOACTIVATE)
      ) {
        _logger.Warn($"{name} SetWindowPos error {Marshal.GetLastWin32Error()}");
        return false;
      }

      _logger.Info($"{name} MakeBackground success");
      return true;
    }

    private bool FindTargetProcessIds() {
      _ovrServerId = GetProcessByName(Target.OVRServer.GetProcessName())?.Id;
      if (_ovrServerId == null) {
        _logger.Warn("Cannot find the OVRServer.");
        return false;
      }

      if (_isSteam) {
        _steamProcessId = GetProcessByName(Target.Steam.GetProcessName())?.Id;
        if (_steamProcessId == null) {
          _logger.Warn("Cannot find the steam process.");
        }
      }
      else {
        _steamProcessId = null;
      }

      return true;
    }

    private Process? GetProcessByName(string processName) {
      var processes = FindProcessByName(processName).ToList();
      if (processes.Count == 0) {
        string list = Process.GetProcesses().Select(p => $"{p.ProcessName}\t{p.Id}").Aggregate((a, b) => $"{a}\n{b}");
        _logger.Trace($"No process {processName} found.\n${list}");
        return null;
      }
      if (processes.Count > 1) {
        string list = processes.Select(p => $"{p.ProcessName}\t{p.Id}").Aggregate((a, b) => $"{a}, {b}");
        _logger.Trace($"There are multiple processes having name {processName}: {list}");
      }
      return processes[0];
    }

    private static IEnumerable<Process> FindProcessByName(string name) {
      return Process.GetProcesses().Where(p => p.ProcessName == name);
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
  }
}
