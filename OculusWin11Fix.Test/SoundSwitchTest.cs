namespace OculusWin11Fix.Test {
  using Nanikit.Test;
  using OculusWin11Fix.Installers;
  using OculusWin11Fix.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class SoundSwitchTest {
    public SoundSwitchTest(IPALogger logger) {
      _logger = new CustomLogger(logger);

      DiContainer container = new();
      container.Install<SoundSwitchInstaller>(new object[] { _logger });
      _switcher = container.Resolve<DefaultAudioSwitcher>();
    }

    [Test]
    public void TestToggle() {
      _switcher.Initialize();
      _switcher.Dispose();
      _logger.Debug("done");
    }

    private readonly IPALogger _logger;
    private readonly DefaultAudioSwitcher _switcher;
  }
}
