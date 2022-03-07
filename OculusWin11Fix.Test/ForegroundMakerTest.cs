namespace OculusWin11Fix.Test {
  using ModestTree;
  using Nanikit.Test;
  using OculusWin11Fix.Core;
  using OculusWin11Fix.External;
  using OculusWin11Fix.Services;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class ForegourndMakerTest {
    public ForegourndMakerTest(IPALogger logger) {
      _logger = logger;

      DiContainer container = new();
      container.BindInterfacesAndSelfTo<IPALogger>().FromInstance(logger).AsSingle();
      container.BindInterfacesAndSelfTo<WindowEnumerator>().AsSingle();
      container.BindInterfacesAndSelfTo<ForegroundMaker>().AsSingle().WithArguments(true);
      _foregroundMaker = container.Resolve<IForegroundMaker>();
    }

    [Test]
    public void TestToggle() {
      Assert.That(_foregroundMaker.MakeForeground(), "MakeForeground() failed");
      Assert.That(_foregroundMaker.MakeBackground(), "MakeBackground() failed");
      _logger.Info("done");
    }

    private readonly IPALogger _logger;
    private readonly IForegroundMaker _foregroundMaker;
  }
}
