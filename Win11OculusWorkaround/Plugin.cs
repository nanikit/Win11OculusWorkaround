namespace Win11OculusWorkaround {
  using IPA;
  using IPA.Config.Stores;
  using IPA.Loader;
  using Win11OculusWorkaround.Installers;
  using Win11OculusWorkaround.Services;
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
    public void Setup(IPALogger logger, Zenjector zenjector, PluginMetadata metadata, IPA.Config.Config ipaStore) {
      _logger = logger;
      _metadata = metadata;

      _configuration = ipaStore.Generated<Configuration>();

#if PRE_1_19
      zenjector.OnApp<AppInstaller>();
#else
      zenjector.Install<AppInstaller>(Location.App);
      zenjector.UseLogger(logger);
      zenjector.UseMetadataBinder<Plugin>();
#endif

      _logger?.Info("Finished setup.");
    }

    [OnStart]
    public void Initialize() {
      _logger?.Info("Initialized.");
    }

    [OnExit]
    public void OnExit() {
      // No op, just for suppressing BSIPA confirm.
    }

    static internal IPALogger? _logger;
    static internal PluginMetadata? _metadata;
    static internal Configuration? _configuration;
  }
}
