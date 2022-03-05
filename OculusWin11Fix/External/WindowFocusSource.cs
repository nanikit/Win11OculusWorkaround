namespace OculusWin11Fix.External {
  using OculusWin11Fix.Core;
  using System;
  using Zenject;
  using static PInvoke.User32;
  using IPALogger = IPA.Logging.Logger;

  class WindowFocusSource : IWindowFocusSource, IInitializable, IDisposable {
    public WindowFocusSource(IPALogger logger) {
      _logger = logger;
      _procDelegate = new(WinEventProc);
    }

    public event Action OnFocused = delegate { };

    public void Initialize() {
      // Listen for name change changes across all processes/threads on current desktop...
      _hookHandle = SetWinEventHook(
        WindowsEventHookType.EVENT_SYSTEM_FOREGROUND, WindowsEventHookType.EVENT_SYSTEM_FOREGROUND,
        IntPtr.Zero, _procDelegate, 0, 0, WindowsEventHookFlags.WINEVENT_INCONTEXT);
    }

    public void Dispose() {
      _hookHandle?.Dispose();
    }

    private readonly IPALogger _logger;
    // Need to ensure delegate is not collected while we're using it,
    // storing it in a class field is simplest way to do this.
    private readonly WinEventProc _procDelegate;
    private SafeEventHookHandle? _hookHandle;

    void WinEventProc(
      IntPtr hWinEventHook, WindowsEventHookType type, IntPtr hwnd,
      int idObject, int idChild, int dwEventThread, uint dwmsEventTime
    ) {
      _logger.Info("WM_WINDOWPOSCHANGING fired");
      OnFocused();
    }
  }
}
