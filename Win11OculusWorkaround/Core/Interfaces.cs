namespace Win11OculusWorkaround.Core {
  using System;

  public interface IForegroundMaker {
    bool MakeForeground();
    bool MakeBackground();
  }

  public interface IPresenceDetector {
    event Action<bool> OnPresenceChanged;
  }

  public interface IWindowFocusSource {
    event Action OnFocused;
  }
}
