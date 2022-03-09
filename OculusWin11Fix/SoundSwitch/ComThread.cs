namespace SoundSwitch.Audio.Manager.Interop.Com.Threading {
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using IPALogger = IPA.Logging.Logger;

  public class ComThread {
    public ComThread(IPALogger logger) {
      _logger = logger;
    }

    private bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != Scheduler.ThreadId;

    private ComTaskScheduler Scheduler { get; } = new ComTaskScheduler();

    /// <summary>
    /// Asserts that the execution following this statement is running on the ComThreads
    /// <exception cref="InvalidThreadException">Thrown if the assertion fails</exception>
    /// </summary>
    public void Assert() {
      if (InvokeRequired)
        throw new InvalidOperationException($"This operation must be run on the ComThread ThreadId: {Scheduler.ThreadId}");
    }

    public void Invoke(Action action) {
      if (!InvokeRequired) {
        action();
        return;
      }

      BeginInvoke(action).Wait();
    }

    private Task BeginInvoke(Action action) {
      return Task.Factory.StartNew(() => {
        try {
          action();
        }
        catch (Exception e) {
          _logger.Warn($"Issue while running action in {nameof(ComThread)}: {e}");
        }
      }, CancellationToken.None, TaskCreationOptions.None, Scheduler);
    }

    public T Invoke<T>(Func<T> func) {
      return !InvokeRequired ? func() : BeginInvoke(func).GetAwaiter().GetResult();
    }

    private readonly IPALogger _logger;

    private Task<T> BeginInvoke<T>(Func<T> func) {
      return Task<T>.Factory.StartNew(() => {
        try {
          return func();
        }
        catch (Exception e) {
          _logger.Warn($"Issue while running func in {nameof(ComThread)}: {e}");
          return default;
        }

      }, CancellationToken.None, TaskCreationOptions.None, Scheduler);
    }
  }
}
