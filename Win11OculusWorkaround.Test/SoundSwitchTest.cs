namespace Win11OculusWorkaround.Test {
  using Nanikit.Test;
  using Win11OculusWorkaround.External;
  using Win11OculusWorkaround.Services;
  using System.Threading.Tasks;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class SoundSwitchTest {
    public SoundSwitchTest(IPALogger logger) {
      _logger = new CustomLogger(logger);

      DiContainer container = new();
      container.Bind<IPALogger>().FromInstance(_logger);
      container.Bind<Configuration>().FromInstance(new Configuration() { EnableSoundWorkaround = true });
      container.BindInterfacesAndSelfTo<DefaultAudioSwitcher>().AsSingle();
      _switcher = container.Resolve<DefaultAudioSwitcher>();
    }

    [Test]
    public async Task TestToggle() {
      await _switcher.ExcludeOculusFromDefault().ConfigureAwait(false);
      _logger.Debug("done");
    }

    private readonly IPALogger _logger;
    private readonly DefaultAudioSwitcher _switcher;
  }
}
