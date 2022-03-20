namespace Win11OculusWorkaround.Installers {
  using Win11OculusWorkaround.External;
  using Win11OculusWorkaround.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  public class ForegrounderInstaller : Installer {
    public ForegrounderInstaller(IPALogger logger) {
      _logger = logger;
    }

    public override void InstallBindings() {
      Container.Bind<IPALogger>().FromInstance(_logger).AsCached().IfNotBound();

      Container.BindInterfacesAndSelfTo<InterestWindowEnumerator>().AsSingle();
      Container.BindInterfacesAndSelfTo<ForegroundMaker>().AsSingle();

      _logger.Trace("Finished installation.");
    }

    private readonly IPALogger _logger;
  }
}
