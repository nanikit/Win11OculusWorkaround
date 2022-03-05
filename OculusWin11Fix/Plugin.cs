namespace OculusWin11Fix {
  using IPA;
  using OculusWin11Fix.Installers;
  using SiraUtil.Zenject;
  using IPALogger = IPA.Logging.Logger;

  [Plugin(RuntimeOptions.SingleStartInit)]
  public class Plugin {
    /// <summary>
    /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
    /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
    /// Only use [Init] with one Constructor.
    /// </summary>
    [Init]
    public void Setup(IPALogger logger, Zenjector zenjector) {
      _logger = logger;

      zenjector.UseLogger(logger);
      zenjector.Install<AppInstaller>(Location.App, logger);
    }

    [OnStart]
    public void Initialize() {
      if (_logger == null) {
        return;
      }
      _logger.Info("Initialized.");
    }

    [OnExit]
    public void OnExit() {
      // No op, just for suppressing BSIPA confirm.
    }

    private IPALogger? _logger;
  }
}
