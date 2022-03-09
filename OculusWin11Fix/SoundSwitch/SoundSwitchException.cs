namespace OculusWin11Fix.SoundSwitch {
  using System;

  internal class SoundSwitchException : Exception {
    public SoundSwitchException(string message) : base(message) { }
    public SoundSwitchException(string message, Exception innerException) : base(message, innerException) { }
  }
}
