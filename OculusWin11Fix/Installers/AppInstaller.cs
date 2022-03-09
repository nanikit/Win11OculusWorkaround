namespace OculusWin11Fix.Installers {
  using HarmonyLib;
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

      Container.Install<ForegrounderInstaller>(new object[] { _logger, _harmony });
      Container.Install<SoundSwitchInstaller>(new object[] { _logger });

      _logger.Trace($"Finished installation.");
    }

    private readonly IPALogger _logger;
    private readonly Harmony _harmony;
  }
}
