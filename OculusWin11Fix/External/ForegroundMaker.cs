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
      return ScanAndApply(MakeTopmost);
    }

    public bool MakeBackground() {
      return ScanAndApply(ReleaseTopmost);
    }

    private static readonly string[] _steamFriendWindowTitles = {
      "Amici", "Amigos", "Arkadaşlar", "Bạn bè", "Barátok", "Contacts", "Freunde",
      "Friends", "Kaverit", "Přátelé", "Prieteni", "Vänner", "Venner", "Vrienden",
      "Znajomi", "Φίλοι", "Друзі", "Друзья", "Приятели", "เพื่อน", "친구", "フレンド", "好友",
    };
    private static readonly SetWindowPosFlags _commonPosFlags = SetWindowPosFlags.SWP_NOMOVE
      | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_ASYNCWINDOWPOS;
    private readonly IPALogger _logger;
    private readonly WindowEnumerator _windowEnumerator;
    private readonly bool _isSteam;
    private IntPtr _steamWindowHandle = IntPtr.Zero;
    private IntPtr _ovrConsoleHandle = IntPtr.Zero;
    private int? _steamProcessId;
    private int? _ovrServerId;

    private bool ScanAndApply(Func<IntPtr, string, bool> action) {
      if (!FindTargetProcessIds()) {
        return false;
      }

      _ovrConsoleHandle = IntPtr.Zero;
      _steamWindowHandle = IntPtr.Zero;

      bool isSuccess = _windowEnumerator.Enumerate(RecordWindow);
      if (!isSuccess && Marshal.GetLastWin32Error() != 1300) {
        _logger.Warn($"EnumWindows failed: {Marshal.GetLastWin32Error()}.");
        return false;
      }

      if (_ovrConsoleHandle == IntPtr.Zero) {
        _logger.Warn($"Cannot find the {Target.OVRServer.GetName()} window.");
      }
      else {
        action(_ovrConsoleHandle, Target.OVRServer.GetName());
      }

      if (_isSteam && _steamWindowHandle == IntPtr.Zero) {
        _logger.Warn($"Cannot find the {Target.Steam.GetName()} window.");
      }
      else {
        action(_steamWindowHandle, Target.Steam.GetName());
      }

      return true;
    }


    private bool RecordWindow(IntPtr windowHandle) {
      try {
        bool isTopLevel = GetWindow(windowHandle, GetWindowCommands.GW_OWNER) == IntPtr.Zero;
        if (!isTopLevel) {
          return true;
        }

        GetWindowThreadProcessId(windowHandle, out int processId);
        if (processId == _ovrServerId && IsWindowVisible(windowHandle)) {
          _ovrConsoleHandle = windowHandle;
        }
        else if (processId == _steamProcessId) {
          try {
            string title = GetWindowText(windowHandle);
            if (!string.IsNullOrEmpty(title) && _steamFriendWindowTitles.Contains(title)) {
              _steamWindowHandle = windowHandle;
            }
          }
          catch (Win32Exception) {
            // Ignore, I don't know why this occurs.
          }
        }

        return _ovrConsoleHandle == IntPtr.Zero || (_isSteam && _steamWindowHandle == IntPtr.Zero);
      }
      catch (Exception exception) {
        _logger.Error(exception);
        throw exception;
      }
    }

    private bool MakeTopmost(IntPtr windowHandle, string name) {
      ShowWindow(windowHandle, WindowShowStyle.SW_RESTORE);
      if (!SetWindowPos(windowHandle, SpecialWindowHandles.HWND_TOP,
        0, 0, 0, 0, _commonPosFlags | SetWindowPosFlags.SWP_SHOWWINDOW)
      ) {
        _logger.Warn($"{name} SetWindowPos error {Marshal.GetLastWin32Error()}");
        return false;
      }
      SetForegroundWindow(windowHandle);

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

    private bool FindTargetProcessIds() {
      _ovrServerId = GetProcessByName(Target.OVRServer.GetProcessName())?.Id;
      if (_ovrServerId == null) {
        _logger.Warn($"Cannot find the {Target.OVRServer.GetProcessName()} process.");
        return false;
      }

      if (_isSteam) {
        _steamProcessId = GetProcessByName(Target.Steam.GetProcessName())?.Id;
        if (_steamProcessId == null) {
          _logger.Warn($"Cannot find the {Target.Steam.GetProcessName()} process.");
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

    public static string GetName(this Target target) => target switch {
      Target.OVRServer => "OVRServer",
      Target.Steam => "Steam",
      _ => "",
    };
  }
}
