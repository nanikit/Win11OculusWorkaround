namespace OculusWin11Fix.External {
  using IPA.Utilities;
  using Newtonsoft.Json;
  using OculusWin11Fix.Core;
  using OculusWin11Fix.Services;
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Runtime.Serialization;
  using System.Threading.Tasks;
  using static OculusWin11Fix.External.SoundVolumeViewRow;
  using static SimpleExec.Command;
  using IPALogger = IPA.Logging.Logger;

  public class DefaultAudioSwitcher : IForegroundMaker {
    public DefaultAudioSwitcher(IPALogger logger, Configuration configuration) {
      _logger = logger;
      _configuration = configuration;
    }

    public bool MakeForeground() {
      if (_configuration.EnableSoundWorkaround) {
        _ = ExcludeOculusWithGuard();
      }
      else {
        _logger.Debug("Skip audio workaround by configuration.");
      }
      return true;
    }

    public bool MakeBackground() {
      return true;
    }

    public async Task ExcludeOculusWithGuard() {
      try {
        await ExcludeOculusFromDefault().ConfigureAwait(false);
      }
      catch (Exception exception) {
        _logger.Error(exception);
      }
    }

    public async Task ExcludeOculusFromDefault() {
      string temporaryPath = Path.GetTempFileName();
      await RunSoundVolumeViewRow($"/SaveFileEncoding 3 /sjson \"{temporaryPath}\"").ConfigureAwait(false);
      string json = await Utilities.ReadAllTextAsync(temporaryPath).ConfigureAwait(false);
      var rows = JsonConvert.DeserializeObject<List<SoundVolumeViewRow>>(json);

      var (oculus, nonOculus) = PartitionDevices(rows, DeviceDirection.Render);
      if (nonOculus == null) {
        _logger.Warn($"Cannot find alternative output audio.");
      }
      else if (IsDefault(nonOculus)) {
        await RunSoundVolumeViewRow($"/SetDefault \"{nonOculus.Name}\" all").ConfigureAwait(false);
        _logger.Notice($"Set default audio output device to {nonOculus.Name} {nonOculus.DeviceName}.");
      }
      else {
        _logger.Info($"Leave default audio output device {nonOculus.Name} {nonOculus.DeviceName}.");
      }

      (oculus, nonOculus) = PartitionDevices(rows, DeviceDirection.Capture);
      if (nonOculus == null) {
        _logger.Warn($"Cannot find alternative input audio.");
      }
      else if (IsDefault(nonOculus)) {
        await RunSoundVolumeViewRow($"/SetDefault \"{nonOculus.Name}\" all").ConfigureAwait(false);
        _logger.Notice($"Set default audio input device to {nonOculus.Name} {nonOculus.DeviceName}.");
      }
      else {
        _logger.Info($"Leave default audio input device {nonOculus.Name} {nonOculus.DeviceName}.");
      }

      if (oculus == null) {
        _logger.Warn($"Cannot find oculus output audio.");
      }
      else {
        await RunSoundVolumeViewRow($"/SetAppDefault \"{oculus.DeviceName}\" all {Process.GetCurrentProcess().Id}");
        _logger.Debug($"Set Beat saber output audio to oculus.");
      }

      await Task.Run(() => File.Delete(temporaryPath)).ConfigureAwait(false);
    }

    private static readonly string _svvPath = Path.Combine(UnityGame.LibraryPath, "SoundVolumeView.exe");

    private readonly IPALogger _logger;
    private readonly Configuration _configuration;

    private async Task RunSoundVolumeViewRow(string args) {
      try {
        await RunAsync(_svvPath, args, createNoWindow: true).ConfigureAwait(false);
      }
      // The system cannot find the file specified
      catch (Win32Exception exception) when ((uint)exception.ErrorCode == 0x80004005) {
        throw new Exception($"Cannot find the executable: {_svvPath}", exception);
      }
      catch (Exception e) {
        _logger.Error(e);
      }
    }

    private (SoundVolumeViewRow?, SoundVolumeViewRow?) PartitionDevices(IEnumerable<SoundVolumeViewRow> rows, DeviceDirection direction) {
      var activeDevices = rows.Where(x => x.State == DeviceState.Active);
      var directedDevices = activeDevices.Where(x => x.Direction == direction);
      (var oculus, var nonOculus) = directedDevices.Aggregate(
        (new List<SoundVolumeViewRow>(), new List<SoundVolumeViewRow>()),
        (acc, device) => {
          (IsOculus(device) ? acc.Item1 : acc.Item2).Add(device);
          return acc;
        });

      nonOculus.Sort(PreferRealDevice);
      _logger.Debug($"Default {direction} devices:{directedDevices.Where(IsDefault).Aggregate("", (acc, x) => $"{acc}, {x.Name}_{x.DeviceName}")}");
      _logger.Debug($"All {direction} device candidates:{nonOculus.Aggregate("", (acc, x) => $"{acc}, {x.Name}_{x.DeviceName}")}");
      return (oculus.FirstOrDefault(), nonOculus.FirstOrDefault());
    }

    private static int PreferRealDevice(SoundVolumeViewRow a, SoundVolumeViewRow b) {
      return GetPreference(b) - GetPreference(a);
    }

    private static int GetPreference(SoundVolumeViewRow row) {
      string id = $"{row.Name}|{row.DeviceName}";
      if (IsDefault(row)) {
        return 1;
      }
      if (id.Contains("Digital")) {
        return -2;
      }
      if (id.Contains("NVIDIA")) {
        return -1;
      }
      return 0;
    }

    private static bool IsOculus(SoundVolumeViewRow row) {
      return row.DeviceName?.Contains("Oculus Virtual Audio Device") ?? false;
    }

    private static bool IsDefault(SoundVolumeViewRow row) {
      return !string.IsNullOrEmpty(row.Default)
        || !string.IsNullOrEmpty(row.DefaultMultimedia)
        || !string.IsNullOrEmpty(row.DefaultCommunications);
    }
  }

  class SoundVolumeViewRow {
#pragma warning disable 0649

    [JsonProperty("Name")]
    public string? Name;
    [JsonProperty("Type")]
    public string? Type;
    [JsonProperty("Direction")]
    public DeviceDirection? Direction;
    [JsonProperty("Device Name")]
    public string? DeviceName;
    [JsonProperty("Default")]
    public string? Default;
    [JsonProperty("Default Multimedia")]
    public string? DefaultMultimedia;
    [JsonProperty("Default Communications")]
    public string? DefaultCommunications;
    [JsonProperty("Device State")]
    public DeviceState State;
    [JsonProperty("Muted")]
    public string? Muted;
    [JsonProperty("Volume dB")]
    public string? VolumedB;
    [JsonProperty("Volume Percent")]
    public string? VolumePercent;
    [JsonProperty("Min Volume dB")]
    public string? MinVolumedB;
    [JsonProperty("Max Volume dB")]
    public string? MaxVolumedB;
    [JsonProperty("Volume Step")]
    public string? VolumeStep;
    [JsonProperty("Channels Count")]
    public string? ChannelsCount;
    [JsonProperty("Channels dB")]
    public string? ChannelsdB;
    [JsonProperty("Channels  Percent")]
    public string? ChannelsPercent;
    [JsonProperty("Item ID")]
    public string? ItemId;
    [JsonProperty("Command-Line Friendly ID")]
    public string? CommandLineFriendlyId;
    [JsonProperty("Process Path")]
    public string? ProcessPath;
    [JsonProperty("Process ID")]
    public string? ProcessID;
    [JsonProperty("Window Title")]
    public string? WindowTitle;
    [JsonProperty("Registry Key")]
    public string? RegistryKey;

#pragma warning restore 0649

    [JsonConverter(typeof(SafeStringEnumConverter<DeviceState>), Unknown)]
    public enum DeviceState {
      Active,
      [EnumMember(Value = "")]
      Unknown,
    }

    [JsonConverter(typeof(SafeStringEnumConverter<DeviceDirection>), Unknown)]
    public enum DeviceDirection {
      Render,
      Capture,
      [EnumMember(Value = "")]
      Unknown,
    }
  }
}
