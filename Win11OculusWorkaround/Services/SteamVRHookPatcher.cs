namespace Win11OculusWorkaround.Services {
  using HarmonyLib;
  using System;
  using System.Diagnostics.CodeAnalysis;
  using Valve.VR;
  using IPALogger = IPA.Logging.Logger;

  [HarmonyPatch(typeof(CVRSystem), nameof(CVRSystem.PollNextEvent))]
  public static class SteamVRHookPatcher {
    public static event Action OnHmdUnmount = delegate { };

    public static IPALogger? Logger { get; set; }

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    static void Postfix(ref VREvent_t pEvent, uint uncbVREvent) {
      var eventType = (EVREventType)pEvent.eventType;
      if (eventType == EVREventType.VREvent_TrackedDeviceUserInteractionEnded) {
        Logger?.Debug($"VREvent_TrackedDeviceUserInteractionEnded");
        OnHmdUnmount();
      }
    }
  }
}
