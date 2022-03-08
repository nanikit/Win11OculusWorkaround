namespace OculusWin11Fix.Services {
  using System;
  using System.Runtime.InteropServices;
  using static PInvoke.User32;
  using IPALogger = IPA.Logging.Logger;

  public class WindowEnumerator {

    public WindowEnumerator(IPALogger logger) {
      _logger = logger;
      _enumerator = new(FindWindow);
    }

    public bool Enumerate(Func<IntPtr, bool> predicate) {
      _predicate = predicate;
      if (!EnumWindows(_enumerator, IntPtr.Zero)) {
        int error = Marshal.GetLastWin32Error();
        bool isQuit = error == 0;
        if (!isQuit) {
          _logger.Trace($"EnumWindows error {error}");
          return false;
        }
      }
      return true;
    }

    private readonly IPALogger _logger;
    private readonly WNDENUMPROC _enumerator;
    private Func<IntPtr, bool> _predicate = (_) => false;

    private bool FindWindow(IntPtr hWnd, IntPtr lParam) {
      return _predicate(hWnd);
    }
  }
}
