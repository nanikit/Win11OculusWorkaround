namespace OculusWin11Fix.Installers {
  using OculusWin11Fix.Core;
  using OculusWin11Fix.External;
  using OculusWin11Fix.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class AppInstaller : Installer {
    public AppInstaller(IPALogger logger) {
      _logger = logger;
    }

    public override void InstallBindings() {
      Container.BindInstance(_logger).AsSingle();
      Container.BindInterfacesAndSelfTo<PresenceDetector>().AsSingle();
      Container.BindInterfacesAndSelfTo<WindowFocusSource>().AsSingle();
      Container.BindInterfacesAndSelfTo<MainWindowFinder>().AsSingle();
      Container.BindInterfacesAndSelfTo<ForegroundMaker>().AsSingle();
      Container.BindInterfacesAndSelfTo<FocusForward>().AsSingle().NonLazy();
    }

    private readonly IPALogger _logger;
  }
}
