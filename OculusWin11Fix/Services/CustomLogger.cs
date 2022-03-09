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
        // 0: StackFrame constructor, 1: this.Callter, 2: this.Log
        MethodBase method = new StackFrame(3).GetMethod();
        return $"{method.DeclaringType.Name}.{method.Name}";
      }
    }
  }
}
