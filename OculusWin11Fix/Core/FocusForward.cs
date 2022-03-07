namespace OculusWin11Fix.Core {
  using System;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class FocusForward : IInitializable, IDisposable {
    public FocusForward(IPALogger logger, IPresenceDetector presenceDetector, IWindowFocusSource focusSource, IForegroundMaker foregrounder) {
      _logger = logger;
      _presenceDetector = presenceDetector;
      _windowFocusSource = focusSource;
      _foregroundMaker = foregrounder;
    }

    public void Initialize() {
      _presenceDetector.OnPresenceChanged += ToggleFocusForward;
      _windowFocusSource.OnFocused += ForwardFocusIfRequired;
      _logger.Debug("Focus forward initialized.");
    }

    public void Dispose() {
      _windowFocusSource.OnFocused -= ForwardFocusIfRequired;
      _presenceDetector.OnPresenceChanged -= ToggleFocusForward;
      _foregroundMaker.MakeBackground();
      _logger.Debug("Focus forward cleared.");
    }

    private readonly IPALogger _logger;
    private readonly IForegroundMaker _foregroundMaker;
    private readonly IPresenceDetector _presenceDetector;
    private readonly IWindowFocusSource _windowFocusSource;
    private bool _isDiving;

    private void ToggleFocusForward(bool isDiving) {
      _logger.Debug($"ToggleFocusForward: {isDiving}");
      _isDiving = isDiving;

      if (isDiving) {
        _foregroundMaker.MakeForeground();
      }
      else {
        _foregroundMaker.MakeBackground();
      }
    }

    private void ForwardFocusIfRequired() {
      _logger.Debug($"ToggleFocusForward: {_isDiving}");
      if (_isDiving) {
        _foregroundMaker.MakeForeground();
      }
    }
  }
}
