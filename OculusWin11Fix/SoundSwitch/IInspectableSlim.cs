using System;
using System.Runtime.InteropServices;
using PInvoke;

namespace SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended {
  [Guid("00000000-0000-0000-C000-000000000046")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IInspectableSlim {
    [PreserveSig]
    HResult GetIids(IntPtr count, ref IntPtr iids);
  }
}
