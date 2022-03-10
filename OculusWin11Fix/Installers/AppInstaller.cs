namespace OculusWin11Fix.Installers {
  using HarmonyLib;
  using OculusWin11Fix.Core;
  using OculusWin11Fix.External;
  using OculusWin11Fix.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  public class AppInstaller : Installer {
    public AppInstaller(IPALogger logger, Harmony harmony) {
      _logger = logger;
      _harmony = harmony;
    }

    public override void InstallBindings() {
      Container.Bind<IPALogger>().FromInstance(new CustomLogger(_logger)).AsCached();

      Container.Bind<Harmony>().AsSingle().WithArguments(_harmony.Id);
      Container.BindInterfacesAndSelfTo<PresenceDetector>().AsSingle();

      Container.Install<ForegrounderInstaller>(new object[] { _logger });

      Container.BindInterfacesAndSelfTo<DefaultAudioSwitcher>().AsSingle();

      Container.BindInterfacesAndSelfTo<WindowFocusSource>().AsSingle();
      Container.BindInterfacesAndSelfTo<FocusForward>().AsSingle().NonLazy();
      _logger.Trace($"Finished installation.");
    }

    private readonly IPALogger _logger;
    private readonly Harmony _harmony;
  }
}
