#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OculusWin11Fix.SoundSwitch;
using SoundSwitch.Audio.Manager.Interop.Client.ClientException;
using SoundSwitch.Audio.Manager.Interop.Client.Extended;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;
using IPALogger = IPA.Logging.Logger;

namespace SoundSwitch.Audio.Manager.Interop.Client {
  internal class ExtendedPolicyClient {
    private const string DEVINTERFACE_AUDIO_RENDER = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";
    private const string DEVINTERFACE_AUDIO_CAPTURE = "#{2eef81be-33fa-4800-9670-1cd474972c3f}";
    private const string MMDEVAPI_TOKEN = @"\\?\SWD#MMDEVAPI#";

    private IAudioPolicyConfig? _sharedPolicyConfig;
    private readonly IPALogger _logger;

    public ExtendedPolicyClient(IPALogger logger) {
      _logger = logger;
    }

    private IAudioPolicyConfig PolicyConfig {
      get {
        if (_sharedPolicyConfig != null) {
          return _sharedPolicyConfig;
        }

        return _sharedPolicyConfig = new AudioPolicyConfig();
      }
    }

    private static string GenerateDeviceId(string deviceId, EDataFlow flow) {
      return $"{MMDEVAPI_TOKEN}{deviceId}{(flow == EDataFlow.eRender ? DEVINTERFACE_AUDIO_RENDER : DEVINTERFACE_AUDIO_CAPTURE)}";
    }

    private static string? UnpackDeviceId(string? deviceId) {
      if (deviceId == null) {
        return null;
      }
      if (deviceId.StartsWith(MMDEVAPI_TOKEN)) deviceId = deviceId.Remove(0, MMDEVAPI_TOKEN.Length);
      if (deviceId.EndsWith(DEVINTERFACE_AUDIO_RENDER)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_RENDER.Length);
      if (deviceId.EndsWith(DEVINTERFACE_AUDIO_CAPTURE)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_CAPTURE.Length);
      return deviceId;
    }

    public void SetDefaultEndPoint(string deviceId, EDataFlow flow, IEnumerable<ERole> roles, uint processId) {
      _logger.Info($"ExtendedPolicyClient SetDefaultEndPoint {deviceId} [{flow}] {processId}");
      try {
        if (string.IsNullOrEmpty(deviceId)) {
          return;
        }

        var deviceIdStr = GenerateDeviceId(deviceId, flow);
        foreach (var eRole in roles) {
          PolicyConfig.SetPersistedDefaultAudioEndpoint(processId, flow, eRole, deviceIdStr);
        }
      }
      catch (COMException e) when ((e.ErrorCode & ErrorConst.COM_ERROR_MASK) == ErrorConst.COM_ERROR_NOT_FOUND) {
        throw new SoundSwitchException($"Can't set default as {deviceId}", e);
      }
      catch (Exception ex) {
        _logger.Error($"Can't set the default endpoint: {ex}");
      }
    }

    /// <summary>
    /// Get the deviceId of the current DefaultEndpoint
    /// </summary>
    public string? GetDefaultEndPoint(EDataFlow flow, ERole role, uint processId) {
      try {
        var deviceId = PolicyConfig.GetPersistedDefaultAudioEndpoint(processId, flow, role);
        var unpacked = UnpackDeviceId(deviceId);
        return unpacked;
      }
      catch (Exception ex) {
        _logger.Error($"Can't get the default endpoint: {ex}");
      }

      return null;
    }

    public void ResetAllSetEndpoint() {
      try {
        PolicyConfig.ClearAllPersistedApplicationDefaultEndpoints();
      }
      catch (Exception ex) {
        _logger.Error($"Can't reset all endpoints: {ex}");
      }
    }
  }
}
