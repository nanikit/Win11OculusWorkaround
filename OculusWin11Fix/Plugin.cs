namespace OculusWin11Fix {
  using HarmonyLib;
  using IPA;
  using IPA.Loader;
  using OculusWin11Fix.Installers;
  using OculusWin11Fix.Services;
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
    public void Setup(IPALogger logger, Zenjector zenjector, PluginMetadata metadata) {
      _logger = new CustomLogger(logger);
      Harmony harmony = new(metadata.Id);

      zenjector.Install<AppInstaller>(Location.App, logger, harmony);

      zenjector.UseLogger(logger);
      zenjector.UseMetadataBinder<Plugin>();

      _logger?.Info("Installed.");
    }

    [OnStart]
    public void Initialize() {
      _logger?.Info("Initialized.");
    }

    [OnExit]
    public void OnExit() {
      // No op, just for suppressing BSIPA confirm.
    }

    private IPALogger? _logger;
  }
}
