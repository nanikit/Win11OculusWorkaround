namespace OculusWin11Fix.Test {
  using Nanikit.Test;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class MainTest {
    public MainTest(IPALogger logger) {
      _logger = logger;

      DiContainer container = new();
      container.BindInterfacesAndSelfTo<IPALogger>().FromInstance(logger).AsSingle();
      _container = container;
    }

    [Test]
    public void TestOneshotSort() {
      _container.Resolve<IPALogger>();
      _logger.Info("Test run");
    }

    private readonly IPALogger _logger;
    private readonly DiContainer _container;
  }
}
