mfaktx-controller
=================
Download binaries (Windows): <a href="https://github.com/Mini-Geek/mfaktx-controller/blob/master/MfaktXController-win.zip?raw=true">MfaktXController-win.zip</a>

http://www.mersenneforum.org/showthread.php?t=18088 has more details.

Building instructions:
 - I used Visual Studio 2012 Express for Windows Desktop. Anything that supports building .NET 4.5 Windows Applications should work.
 - Dependencies:
  - https://nuget.org/packages/Fody/
  - https://nuget.org/packages/PropertyChanged.Fody
  - 7-Zip, installed at "C:\Program Files\7-Zip\7z.exe" (not strictly necessary, part of the default post-build event)
 - If you contribute a new build, you should build with the Release configuration to automatically update the binaries zip. (it automatically updates every time you build, not just on Release, so try not to check in a Debug build)
