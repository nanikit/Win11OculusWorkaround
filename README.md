# Win11OculusWorkaround

[한국어](./docs/README.ko.md)

Oculus PC runtime has not fixed stuttering issue for a loooong time.

This mod applies current workaround automatically when running Beat saber (foreground console).

## How to use

1. Do [this](https://www.reddit.com/r/oculus/comments/qq4b0h/windows_11_stutter_fix_finally_found_a_solution/)

2. Install this mod.<br />
Copy .dll file to Plugins directory, and it requires SiraUtil.dll and BSML.dll.

3. Activate beat saber window after first launch and whenever activate other windows.
    - Without SteamVR: Activate beat saber window before quitting oculus desktop.
    - SteamVR: Activate beat saber window before quitting steamvr desktop. Doing this in oculus desktop won't work.

4. After step 3, this mod will automatically distribute focus to ovrconsole, vmc, obs window.