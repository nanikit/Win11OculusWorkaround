namespace OculusWin11Fix.Installers {
  using BeatSaberMarkupLanguage.Settings;
  using HarmonyLib;
  using IPA.Loader;
  using OculusWin11Fix.Core;
  using OculusWin11Fix.External;
  using OculusWin11Fix.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  public class AppInstaller : Installer {
    public AppInstaller(IPALogger logger, PluginMetadata metadata, Configuration configuration) {
      _logger = new CustomLogger(logger);
      _metadata = metadata;
      _configuration = configuration;
    }

    public override void InstallBindings() {
      Container.Bind<IPALogger>().FromInstance(_logger).AsCached().IfNotBound();

      Container.BindInstance(_configuration).AsSingle();
      BSMLSettings.instance.AddSettingsMenu(_metadata.Name, "OculusWin11Fix.Services.Views.settings.bsml", _configuration);

      Container.BindInstance(new Harmony(_metadata.Id)).AsSingle();
      Container.BindInterfacesAndSelfTo<PresenceDetector>().AsSingle();

      Container.Install<ForegrounderInstaller>(new object[] { _logger });

      Container.BindInterfacesAndSelfTo<DefaultAudioSwitcher>().AsSingle();

      Container.BindInterfacesAndSelfTo<WindowFocusSource>().AsSingle();
      Container.BindInterfacesAndSelfTo<FocusForward>().AsSingle().NonLazy();

      _logger.Trace($"Finished installation.");
    }

    private readonly IPALogger _logger;
    private readonly PluginMetadata _metadata;
    private readonly Configuration _configuration;
  }
}
