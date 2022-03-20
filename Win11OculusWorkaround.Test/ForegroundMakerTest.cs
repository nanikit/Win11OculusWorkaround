namespace Win11OculusWorkaround.Test {
  using ModestTree;
  using Nanikit.Test;
  using Win11OculusWorkaround.Core;
  using Win11OculusWorkaround.Installers;
  using Win11OculusWorkaround.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class ForegroundMakerTest {
    public ForegroundMakerTest(IPALogger logger) {
      _logger = new CustomLogger(logger);

      DiContainer container = new();
      container.Install<ForegrounderInstaller>(new object[] { _logger });
      _foregroundMaker = container.Resolve<IForegroundMaker>();
    }

    [Test]
    public void TestToggle() {
      Assert.That(_foregroundMaker.MakeForeground(), "MakeForeground() failed");
      Assert.That(_foregroundMaker.MakeBackground(), "MakeBackground() failed");
      _logger.Debug("done");
    }

    private readonly IPALogger _logger;
    private readonly IForegroundMaker _foregroundMaker;
  }
}
