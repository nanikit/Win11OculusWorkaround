namespace OculusWin11Fix.External {
  using OculusWin11Fix.Core;
  using System;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class PresenceDetector : IPresenceDetector, IInitializable, IDisposable {
    public event Action<bool> OnPresenceChanged = delegate { };

    public PresenceDetector(IPALogger logger, IVRPlatformHelper vrPlatform) {
      _logger = logger;
      _vrPlatform = vrPlatform;
      _logger.Debug("PresenceDetector Constructor();");
    }

    public void Initialize() {
      _logger.Debug("PresenceDetector Initialize();");
      _vrPlatform.inputFocusWasCapturedEvent += DispatchDiveEnd;
      _vrPlatform.hmdUnmountedEvent += DispatchDiveEnd;
      _vrPlatform.inputFocusWasReleasedEvent += DispatchDiveStart;
      _vrPlatform.hmdMountedEvent += DispatchDiveStart;
    }

    public void Dispose() {
      _logger.Debug("PresenceDetector Dispose();");
      _vrPlatform.inputFocusWasCapturedEvent -= DispatchDiveEnd;
      _vrPlatform.hmdUnmountedEvent -= DispatchDiveEnd;
      _vrPlatform.inputFocusWasReleasedEvent -= DispatchDiveStart;
      _vrPlatform.hmdMountedEvent -= DispatchDiveStart;
    }

    private readonly IPALogger _logger;
    private readonly IVRPlatformHelper _vrPlatform;
    private bool _isDiving;

    private void DispatchDiveStart() {
      _logger.Info("Detected HMD focus gain.");
      if (!_isDiving) {
        _isDiving = true;
        OnPresenceChanged(true);
      }
    }

    private void DispatchDiveEnd() {
      _logger.Info("Detected HMD focus lost.");
      if (_isDiving) {
        _isDiving = false;
        OnPresenceChanged(false);
      }
    }
  }
}
