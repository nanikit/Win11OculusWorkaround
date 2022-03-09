namespace OculusWin11Fix.Installers {
  using global::SoundSwitch.Audio.Manager;
  using global::SoundSwitch.Audio.Manager.Interop.Com.Threading;
  using OculusWin11Fix.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  public class SoundSwitchInstaller : Installer {
    public SoundSwitchInstaller(IPALogger logger) {
      _logger = logger;
    }

    public override void InstallBindings() {
      Container.Bind<IPALogger>().FromInstance(new CustomLogger(_logger)).AsCached().IfNotBound();
      Container.Bind<ComThread>().AsSingle();
      Container.Bind<AudioSwitcher>().AsSingle();
      Container.BindInterfacesAndSelfTo<DefaultAudioSwitcher>().AsSingle();

      _logger.Trace($"Finished installation.");
    }

    private readonly IPALogger _logger;
  }
}
