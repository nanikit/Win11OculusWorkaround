namespace OculusWin11Fix.Installers {
  using OculusWin11Fix.Core;
  using OculusWin11Fix.External;
  using OculusWin11Fix.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  public class AppInstaller : Installer {
    public static string[] TargetProcessNames { get; } = new[] { "steam", "OVRServer_x64" };

    public AppInstaller(IPALogger logger) {
      _logger = logger;
    }

    public override void InstallBindings() {
      Container.BindInstance(_logger).AsSingle();
      Container.BindInterfacesAndSelfTo<PresenceDetector>().AsSingle();
      Container.BindInterfacesAndSelfTo<WindowFocusSource>().AsSingle();
      Container.BindInterfacesAndSelfTo<WindowEnumerator>().AsSingle();
      Container.BindInterfacesAndSelfTo<ForegroundMaker>().AsCached().WithArguments(true).When(x => x.Container.Resolve<IVRPlatformHelper>() is OpenVRHelper);
      Container.BindInterfacesAndSelfTo<ForegroundMaker>().AsCached().WithArguments(false).When(x => x.Container.Resolve<IVRPlatformHelper>() is not OpenVRHelper);
      Container.BindInterfacesAndSelfTo<FocusForward>().AsSingle().NonLazy();
    }

    private readonly IPALogger _logger;
  }
}
