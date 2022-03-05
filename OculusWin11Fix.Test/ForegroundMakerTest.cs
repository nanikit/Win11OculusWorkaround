namespace OculusWin11Fix.Test {
  using ModestTree;
  using Nanikit.Test;
  using OculusWin11Fix.External;
  using OculusWin11Fix.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class ForegourndMakerTest {
    public ForegourndMakerTest(IPALogger logger) {
      _logger = logger;

      DiContainer container = new();
      container.BindInterfacesAndSelfTo<IPALogger>().FromInstance(logger).AsSingle();
      container.BindInterfacesAndSelfTo<MainWindowFinder>().AsSingle();
      container.BindInterfacesAndSelfTo<ForegroundMaker>().AsSingle();
      _foregroundMaker = container.Resolve<ForegroundMaker>();
    }

    [Test]
    public void TestToggle() {
      Assert.That(_foregroundMaker.MakeForeground(), "MakeForeground() failed");
      Assert.That(_foregroundMaker.MakeBackground(), "MakeBackground() failed");
      _logger.Info("done");
    }

    private readonly IPALogger _logger;
    private readonly ForegroundMaker _foregroundMaker;
  }
}
