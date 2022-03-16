namespace OculusWin11Fix.Core {
  using System;
  using System.Collections.Generic;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class FocusForward : IInitializable, IDisposable {
    public FocusForward(IPALogger logger, IPresenceDetector presenceDetector,
      IWindowFocusSource focusSource, List<IForegroundMaker> foregrounders
    ) {
      _logger = logger;
      _presenceDetector = presenceDetector;
      _windowFocusSource = focusSource;
      _foregroundMakers = foregrounders;
    }

    public void Initialize() {
      _presenceDetector.OnPresenceChanged += ToggleFocusForward;
      _windowFocusSource.OnFocused += ForwardFocusIfRequired;
      _logger.Debug($"Focus forward initialized. foregrounders.Count: {_foregroundMakers.Count}");
    }

    public void Dispose() {
      _windowFocusSource.OnFocused -= ForwardFocusIfRequired;
      _presenceDetector.OnPresenceChanged -= ToggleFocusForward;
      _foregroundMakers.ForEach(x => x.MakeBackground());
      _logger.Debug("Focus forward cleared.");
    }

    private readonly IPALogger _logger;
    private readonly List<IForegroundMaker> _foregroundMakers;
    private readonly IPresenceDetector _presenceDetector;
    private readonly IWindowFocusSource _windowFocusSource;
    private bool _isDiving;

    private void ToggleFocusForward(bool isDiving) {
      _logger.Debug($"ToggleFocusForward: {isDiving}");
      _isDiving = isDiving;

      if (isDiving) {
        _foregroundMakers.ForEach(x => x.MakeForeground());
      }
      else {
        _foregroundMakers.ForEach(x => x.MakeBackground());
      }
    }

    private void ForwardFocusIfRequired() {
      _logger.Debug($"ToggleFocusForward: {_isDiving}");
      if (_isDiving) {
        _foregroundMakers.ForEach(x => x.MakeForeground());
      }
    }
  }
}
