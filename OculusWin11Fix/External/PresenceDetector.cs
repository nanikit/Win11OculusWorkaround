namespace OculusWin11Fix.External {
  using HarmonyLib;
  using OculusWin11Fix.Core;
  using OculusWin11Fix.Services;
  using System;
  using Valve.VR;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class PresenceDetector : IPresenceDetector, IInitializable, IDisposable {
    public event Action<bool> OnPresenceChanged = delegate { };

    public PresenceDetector(IPALogger logger, IVRPlatformHelper vrPlatform, [Inject(Optional = true)] Harmony? harmony) {
      _logger = logger;
      _vrPlatform = vrPlatform;
      _harmony = harmony;
      _logger.Debug($"PresenceDetector Constructor(): harmony {(_harmony == null ? "disabled" : "enabled")}");
    }

    public void Initialize() {
      _logger.Debug("PresenceDetector Initialize()");
      _vrPlatform.inputFocusWasCapturedEvent += OnInputFocusWasCaptured;
      _vrPlatform.hmdUnmountedEvent += OnHmdUnmounted;
      _vrPlatform.inputFocusWasReleasedEvent += OnInputFocusWasReleased;
      _vrPlatform.hmdMountedEvent += OnHmdMounted;

      SteamVRHookPatcher.Logger = _logger;
      SteamVRHookPatcher.OnHmdUnmount += OnHmdUnmounted;
      _harmony?.PatchAll(typeof(SteamVRHookPatcher));
    }

    public void Dispose() {
      _logger.Debug("PresenceDetector Dispose()");

      _harmony?.Unpatch(
        AccessTools.Method(typeof(CVRSystem), nameof(CVRSystem.PollNextEvent)),
        AccessTools.Method(typeof(SteamVRHookPatcher), "Postfix"));
      SteamVRHookPatcher.OnHmdUnmount -= OnHmdUnmounted;

      _vrPlatform.inputFocusWasCapturedEvent -= OnInputFocusWasCaptured;
      _vrPlatform.hmdUnmountedEvent -= OnHmdUnmounted;
      _vrPlatform.inputFocusWasReleasedEvent -= OnInputFocusWasReleased;
      _vrPlatform.hmdMountedEvent -= OnHmdMounted;
    }

    private readonly IPALogger _logger;
    private readonly IVRPlatformHelper _vrPlatform;
    private readonly Harmony? _harmony;
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
