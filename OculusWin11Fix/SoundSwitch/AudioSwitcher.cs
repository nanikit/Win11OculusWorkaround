#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using NAudio.CoreAudioApi;
using PInvoke;
using SoundSwitch.Audio.Manager.Interop.Client;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Enum;
using IPALogger = IPA.Logging.Logger;

namespace SoundSwitch.Audio.Manager {
  public class AudioSwitcher {
    public AudioSwitcher(IPALogger logger, ComThread comThread) {
      _comThread = comThread;
      _logger = logger;
    }

    private readonly ComThread _comThread;
    private readonly IPALogger _logger;
    private PolicyClient? _policyClient;
    private EnumeratorClient? _enumerator;

    private ExtendedPolicyClient? _extendedPolicyClient;

    private EnumeratorClient EnumeratorClient {
      get {
        if (_enumerator != null)
          return _enumerator;

        return _enumerator = _comThread.Invoke(() => new EnumeratorClient());
      }
    }

    private PolicyClient PolicyClient {
      get {
        if (_policyClient != null)
          return _policyClient;

        return _policyClient = _comThread.Invoke(() => new PolicyClient());
      }
    }

    private ExtendedPolicyClient ExtendPolicyClient {
      get {
        if (_extendedPolicyClient != null) {
          return _extendedPolicyClient;
        }

        return _extendedPolicyClient = _comThread.Invoke(() => new ExtendedPolicyClient(_logger));
      }
    }

    /// <summary>
    /// Switch the default audio device to the one given
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="role"></param>
    public void SwitchTo(string deviceId, ERole role) {
      if (role != ERole.ERole_enum_count) {
        _comThread.Invoke(() => {
          if (EnumeratorClient.IsDefault(deviceId, EDataFlow.eRender, role) || EnumeratorClient.IsDefault(deviceId, EDataFlow.eCapture, role)) {
            _logger.Trace($"Default endpoint already {deviceId}");
            return;
          }

          PolicyClient.SetDefaultEndpoint(deviceId, role);
        });

        return;
      }

      SwitchTo(deviceId, ERole.eConsole);
      SwitchTo(deviceId, ERole.eMultimedia);
      SwitchTo(deviceId, ERole.eCommunications);
    }

    /// <summary>
    /// Switch the audio endpoint of the given process
    /// </summary>
    /// <param name="deviceId">Id of the device</param>
    /// <param name="role">Which role to switch</param>
    /// <param name="flow">Which flow to switch</param>
    /// <param name="processId">ProcessID of the process</param>
    public void SwitchProcessTo(string deviceId, ERole role, EDataFlow flow, uint processId) {
      var processName = "";
      try {
        var process = Process.GetProcessById((int)processId);
        processName = process.ProcessName;
      }
      catch (Exception e) {
        Trace.TraceError($"Can't get process info: {e}");
      }

      Trace.TraceInformation($"Attempt to switch [{processId}:{processName}] to {deviceId}");
      var roles = new[]
      {
                ERole.eConsole,
                ERole.eCommunications,
                ERole.eMultimedia
            };

      if (role != ERole.ERole_enum_count) {
        roles = new[]
        {
                    role
                };
      }

      _comThread.Invoke((() => {
        var currentEndpoint = roles.Select(eRole => ExtendPolicyClient.GetDefaultEndPoint(flow, eRole, processId)).FirstOrDefault(endpoint => !string.IsNullOrEmpty(endpoint));
        if (deviceId.Equals(currentEndpoint)) {
          Trace.WriteLine($"Default endpoint for [{processId}:{processName}] already {deviceId}");
          return;
        }

        ExtendPolicyClient.SetDefaultEndPoint(deviceId, flow, roles, processId);
      }));
    }

    /// <summary>
    /// Switch the audio device of the Foreground Process
    /// </summary>
    /// <param name="deviceId">Id of the device</param>
    /// <param name="role">Which role to switch</param>
    /// <param name="flow">Which flow to switch</param>
    public void SwitchForegroundProcessTo(string deviceId, ERole role, EDataFlow flow) {
      var processId = _comThread.Invoke(GetForegroundProcessId);
      SwitchProcessTo(deviceId, role, flow, (uint)processId);
    }

    private static int GetForegroundProcessId() {
      IntPtr activeWindowHandle = User32.GetForegroundWindow();
      User32.GetWindowThreadProcessId(activeWindowHandle, out int processId);
      return processId;
    }

    /// <summary>
    /// Is the given deviceId the default audio device in the system
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="flow"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    public bool IsDefault(string deviceId, EDataFlow flow, ERole role) {
      return _comThread.Invoke(() => EnumeratorClient.IsDefault(deviceId, flow, role));
    }

    /// <summary>
    /// Get the device used by the given process
    /// </summary>
    /// <param name="flow"></param>
    /// <param name="role"></param>
    /// <param name="processId"></param>
    /// <returns></returns>
    public string? GetUsedDevice(EDataFlow flow, ERole role, uint processId) {
      return _comThread.Invoke(() => ExtendPolicyClient.GetDefaultEndPoint(flow, role, processId));
    }

    /// <summary>
    /// Get the current default endpoint
    /// </summary>
    /// <param name="flow"></param>
    /// <param name="role"></param>
    /// <returns>Null if no default device is defined</returns>
    public MMDevice? GetDefaultAudioEndpoint(EDataFlow flow, ERole role) => _comThread.Invoke(() => {
      return EnumeratorClient.GetDefaultEndpoint(flow, role);
    });

    /// <summary>
    /// Used to interact directly with a <see cref="MMDevice"/>
    /// </summary>
    /// <param name="device"></param>
    /// <param name="interaction"></param>
    /// <typeparam name="T"></typeparam>
    public T InteractWithMmDevice<T>(MMDevice device, Func<MMDevice, T> interaction) => _comThread.Invoke(() => interaction(device));

    /// <summary>
    /// Get the current default endpoint
    /// </summary>
    /// <param name="flow"></param>
    /// <param name="role"></param>
    /// <returns>Null if no default device is defined</returns>
    public MMDevice? GetDefaultMmDevice(EDataFlow flow, ERole role) => _comThread.Invoke(() => EnumeratorClient.GetDefaultEndpoint(flow, role));

    /// <summary>
    /// Get a device with the given id, returns null if not present
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    public MMDevice? GetDevice(string deviceId) => _comThread.Invoke(() => EnumeratorClient.GetDevice(deviceId));

    /// <summary>
    /// Reset Windows configuration for the process that had their audio device changed
    /// </summary>
    public void ResetProcessDeviceConfiguration() {
      _comThread.Invoke(() => ExtendPolicyClient.ResetAllSetEndpoint());
    }
  }
}
