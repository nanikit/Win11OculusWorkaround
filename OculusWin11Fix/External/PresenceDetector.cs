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
      _vrPlatform.inputFocusWasCapturedEvent += OnInputFocusWasCaptured;
      _vrPlatform.hmdUnmountedEvent += OnHmdUnmounted;
      _vrPlatform.inputFocusWasReleasedEvent += OnInputFocusWasReleased;
      _vrPlatform.hmdMountedEvent += OnHmdMounted;
    }

    public void Dispose() {
      _logger.Debug("PresenceDetector Dispose();");
      _vrPlatform.inputFocusWasCapturedEvent -= OnInputFocusWasCaptured;
      _vrPlatform.hmdUnmountedEvent -= OnHmdUnmounted;
      _vrPlatform.inputFocusWasReleasedEvent -= OnInputFocusWasReleased;
      _vrPlatform.hmdMountedEvent -= OnHmdMounted;
    }

    private readonly IPALogger _logger;
    private readonly IVRPlatformHelper _vrPlatform;
    private bool _isDiving;

    private void OnInputFocusWasCaptured() {
      _logger.Debug(nameof(_vrPlatform.inputFocusWasCapturedEvent));
      DispatchDiveEnd();
    }

    private void OnInputFocusWasReleased() {
      _logger.Debug(nameof(_vrPlatform.inputFocusWasReleasedEvent));
      DispatchDiveStart();
    }

    private void OnHmdUnmounted() {
      _logger.Debug(nameof(_vrPlatform.hmdUnmountedEvent));
      DispatchDiveEnd();
    }

    private void OnHmdMounted() {
      _logger.Debug(nameof(_vrPlatform.hmdMountedEvent));
      DispatchDiveStart();
    }

    private void DispatchDiveStart() {
      _logger.Debug(nameof(_vrPlatform.hmdMountedEvent));
      if (!_isDiving) {
        _logger.Debug($"{nameof(_vrPlatform.hmdMountedEvent): toggle}");
        _isDiving = true;
        OnPresenceChanged(true);
        _logger.Debug($"{nameof(_vrPlatform.hmdMountedEvent): end}");
      }
    }

    private void DispatchDiveEnd() {
      if (_isDiving) {
        _isDiving = false;
        OnPresenceChanged(false);
      }
    }
  }
}
