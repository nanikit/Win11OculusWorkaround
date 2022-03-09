namespace OculusWin11Fix.Services {
  using global::SoundSwitch.Audio.Manager;
  using global::SoundSwitch.Audio.Manager.Interop.Com.Threading;
  using global::SoundSwitch.Audio.Manager.Interop.Enum;
  using NAudio.CoreAudioApi;
  using System;
  using IPALogger = IPA.Logging.Logger;
  using System.Linq;
  using Zenject;

  public class DefaultAudioSwitcher : IInitializable, IDisposable {

    public DefaultAudioSwitcher(AudioSwitcher switcher, ComThread comThread, IPALogger logger) {
      _switcher = switcher;
      _comThread = comThread;
      _logger = logger;
    }

    public void Initialize() {
      _comThread.Invoke(ExcludeOculusFromDefault);
    }

    public void Dispose() {
    }

    private readonly AudioSwitcher _switcher;
    private readonly ComThread _comThread;
    private readonly IPALogger _logger;

    private void ExcludeOculusFromDefault() {
      MMDevice? device = _switcher.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

      _logger.Debug($"Default multimedia audio: {device?.FriendlyName ?? "null"}, {device}, {device?.DeviceFriendlyName}");
      if (IsOculusDevice(device)) {
        var enumerator = new MMDeviceEnumerator();
        var outputDevices = enumerator
          .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
          .Where<MMDevice>(x => !IsOculusDevice(x))
          .ToList();
        _logger.Trace($"output devices: {outputDevices.Aggregate("", (acc, x) => $"{acc}, {x}")}");
        if (outputDevices.Any()) {
          MMDevice target = outputDevices.First();
          _switcher.SwitchTo(target.ID, ERole.ERole_enum_count);
          _logger.Debug($"Switched output to {target.FriendlyName}");
        }
        else {
          _logger.Debug($"Found no alternative for output audio");
        }

        var inputDevices = enumerator
          .EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
          .Where<MMDevice>(x => !IsOculusDevice(x)).ToList();
        _logger.Trace($"input devices: {inputDevices.Aggregate("", (acc, x) => $"{acc}, {x}")}");
        if (inputDevices.Any()) {
          MMDevice target = inputDevices.First();
          _switcher.SwitchTo(target.ID, ERole.ERole_enum_count);
          _logger.Debug($"Switched input to {target.FriendlyName}");
        }
        else {
          _logger.Debug($"Found no alternative for input audio");
        }
      }
    }

    private static bool IsOculusDevice(MMDevice? device) {
      return device?.FriendlyName.Contains("Oculus") == true;
    }
  }
}
