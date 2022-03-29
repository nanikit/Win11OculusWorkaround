namespace Win11OculusWorkaround.Installers {
  using BeatSaberMarkupLanguage.Settings;
  using HarmonyLib;
  using IPA.Loader;
  using Win11OculusWorkaround.Core;
  using Win11OculusWorkaround.External;
  using Win11OculusWorkaround.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  public class AppInstaller : Installer {
    public AppInstaller() {
      _logger = new CustomLogger(Plugin._logger!);
      _metadata = Plugin._metadata!;
      _configuration = Plugin._configuration!;
    }

    public override void InstallBindings() {
      Container.Bind<IPALogger>().FromInstance(_logger).AsCached().IfNotBound();

      Container.BindInstance(_configuration).AsSingle();
      BSMLSettings.instance.AddSettingsMenu(_metadata.Name, "Win11OculusWorkaround.Services.Views.settings.bsml", _configuration);

      Container.BindInstance(new Harmony(_metadata.Id)).AsSingle();
      Container.BindInterfacesAndSelfTo<PresenceDetector>().AsSingle();

      Container.Install<ForegrounderInstaller>(new object[] { _logger });

      Container.BindInterfacesAndSelfTo<DefaultAudioSwitcher>().AsSingle();

      Container.BindInterfacesAndSelfTo<WindowFocusSource>().AsSingle();
      Container.BindInterfacesAndSelfTo<FocusForward>().AsSingle().NonLazy();

      _logger.Trace($"Finished installation.");
    }

    private readonly CustomLogger _logger;
    private readonly PluginMetadata _metadata;
    private readonly Configuration _configuration;
  }
}
