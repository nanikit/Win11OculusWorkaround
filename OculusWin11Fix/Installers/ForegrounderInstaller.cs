namespace OculusWin11Fix.Installers {
  using HarmonyLib;
  using OculusWin11Fix.Core;
  using OculusWin11Fix.External;
  using OculusWin11Fix.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  public class ForegrounderInstaller : Installer {
    public ForegrounderInstaller(IPALogger logger, Harmony harmony) {
      _logger = logger;
      _harmony = harmony;
    }

    public override void InstallBindings() {
      Container.Bind<Harmony>().AsSingle().WithArguments(_harmony.Id);

      Container.BindInterfacesAndSelfTo<PresenceDetector>().AsSingle();
      Container.BindInterfacesAndSelfTo<WindowFocusSource>().AsSingle();
      Container.BindInterfacesAndSelfTo<WindowEnumerator>().AsSingle();

      Container.Bind<bool>().FromMethod(ResolveVRPlatformHelper).AsSingle().WhenInjectedInto<ForegroundMaker>();
      Container.BindInterfacesAndSelfTo<ForegroundMaker>().AsSingle();

      Container.BindInterfacesAndSelfTo<FocusForward>().AsSingle().NonLazy();
      _logger.Trace("Finished installation.");
    }

    private readonly IPALogger _logger;
    private readonly Harmony _harmony;

    private bool ResolveVRPlatformHelper() {
      if (Container.HasBinding<IVRPlatformHelper>()) {
        return Container.Resolve<IVRPlatformHelper>() is OpenVRHelper;
      }
      // Running on test.
      return true;
    }
  }
}
