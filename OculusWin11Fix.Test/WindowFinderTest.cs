namespace OculusWin11Fix.Test {
  using ModestTree;
  using Nanikit.Test;
  using OculusWin11Fix.External;
  using OculusWin11Fix.Services;
  using System;
  using System.Diagnostics;
  using System.Linq;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class WindowFinderTest {
    public WindowFinderTest(IPALogger logger) {
      _logger = logger;

      DiContainer container = new();
      container.BindInterfacesAndSelfTo<IPALogger>().FromInstance(logger).AsSingle();
      container.BindInterfacesAndSelfTo<MainWindowFinder>().AsSingle();
      _windowFinder = container.Resolve<MainWindowFinder>();
    }

    [Test]
    public void TestMainWindowFind() {
      Process server = Process.GetProcesses().Where(p => p.ProcessName == ForegroundMaker.ProcessName).First();
      IntPtr handle = _windowFinder.GetMainWindowHandle(server.Id);
      _logger.Info($"handle: {handle}");
      Assert.IsNotEqual(handle, IntPtr.Zero);
    }

    private readonly IPALogger _logger;
    private readonly MainWindowFinder _windowFinder;
  }
}
