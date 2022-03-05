namespace OculusWin11Fix.Services {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.InteropServices;
  using static PInvoke.User32;
  using IPALogger = IPA.Logging.Logger;

  public class MainWindowFinder {

    public MainWindowFinder(IPALogger logger) {
      _logger = logger;
      _enumerator = new(FindMainWindow);
    }

    public IntPtr GetMainWindowHandle(int processId) {
      _targetProcessId = processId;
      _handles.Clear();

      if (!EnumWindows(_enumerator, IntPtr.Zero)) {
        _logger.Warn($"EnumWindows error {Marshal.GetLastWin32Error()}");
      }
      if (_handles.Count == 0) {
        return IntPtr.Zero;
      }
      if (_handles.Count > 1) {
        string details = _handles.Select(x => $"{x}").Aggregate((acc, x) => $"{acc}, {x}");
        _logger.Warn($"Process {processId} has more than one windows: {details}");
        return IntPtr.Zero;
      }
      return _handles[0];
    }

    private readonly IPALogger _logger;
    private readonly WNDENUMPROC _enumerator;
    private readonly List<IntPtr> _handles = new();

    private int _targetProcessId;

    private bool FindMainWindow(IntPtr hWnd, IntPtr lParam) {
      GetWindowThreadProcessId(hWnd, out int processId);
      bool isProcessSame = processId == _targetProcessId;
      bool isTopLevel = GetWindow(hWnd, GetWindowCommands.GW_OWNER) == IntPtr.Zero;
      if (isProcessSame && isTopLevel && IsWindowVisible(hWnd)) {
        _handles.Add(hWnd);
      }
      return true;
    }
  }
}
