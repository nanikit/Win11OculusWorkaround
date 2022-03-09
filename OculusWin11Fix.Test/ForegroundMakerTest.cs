namespace OculusWin11Fix.Test {
  using HarmonyLib;
  using ModestTree;
  using Nanikit.Test;
  using OculusWin11Fix.Core;
  using OculusWin11Fix.Installers;
  using OculusWin11Fix.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class ForegroundMakerTest {
    public ForegroundMakerTest(IPALogger logger) {
      _logger = new CustomLogger(logger);

      DiContainer container = new();
      container.Install<ForegrounderInstaller>(new object[] { _logger, new Harmony("OculusWin11Fix.Test") });
      _foregroundMaker = container.Resolve<IForegroundMaker>();
    }

    //[Test]
    public void TestToggle() {
      Assert.That(_foregroundMaker.MakeForeground(), "MakeForeground() failed");
      Assert.That(_foregroundMaker.MakeBackground(), "MakeBackground() failed");
      _logger.Debug("done");
    }

    private readonly IPALogger _logger;
    private readonly IForegroundMaker _foregroundMaker;
  }
}
