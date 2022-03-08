namespace OculusWin11Fix.Services {
  using System.Diagnostics;
  using System.Reflection;
  using IPALogger = IPA.Logging.Logger;

  public class CustomLogger : IPALogger {
    public CustomLogger(IPALogger baseLogger) {
      _baseLogger = baseLogger;
    }

    public override void Log(Level level, string message) {
      switch (level) {
      case Level.Debug:
      case Level.Trace:
        _baseLogger.Log(level, $"{Caller}: {message}");
        break;
      default:
        _baseLogger.Log(level, message);
        break;
      }
    }

    private readonly IPALogger _baseLogger;

    private static string Caller {
      get {
        MethodBase method = new StackFrame(2).GetMethod();
        var result = $"{method.DeclaringType.Name}.{method.Name}";
        return result;
      }
    }
  }
}
