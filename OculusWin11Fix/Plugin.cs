namespace OculusWin11Fix {
  using IPA;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  [Plugin(RuntimeOptions.SingleStartInit)]
  public class Plugin {
    /// <summary>
    /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
    /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
    /// Only use [Init] with one Constructor.
    /// </summary>
    [Init]
    public void Setup(IPALogger logger) {
      _logger = logger;
    }

    [OnStart]
    public void Initialize() {
      if (_logger == null) {
        return;
      }
      _logger.Debug("Initialize()");

      DiContainer container = ProjectContext.Instance.Container.CreateSubContainer();
      container.BindInterfacesAndSelfTo<IPALogger>().FromInstance(_logger).AsSingle();

      _logger.Info("Initialized.");
    }

    [OnExit]
    public void OnExit() {
      // No op, just for suppressing BSIPA confirm.
    }

    private IPALogger? _logger;
  }
}
