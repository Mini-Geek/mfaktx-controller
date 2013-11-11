mfaktx-controller
=================
Download binaries (Windows): <a href="https://github.com/Mini-Geek/mfaktx-controller/blob/master/MfaktXController-win.zip?raw=true">MfaktXController-win.zip</a>

http://www.mersenneforum.org/showthread.php?t=18088 has more details.

Building instructions:
 - I used Visual Studio Express 2013 for Windows Desktop. Anything that supports building .NET 4.5 Windows Applications should work.
 - Dependencies:
  - <a href="http://visualstudiogallery.msdn.microsoft.com/27077b70-9dad-4c64-adcf-c7cf6bc9970c">NuGet</a>, if not already installed
  - <a href="https://nuget.org/packages/Fody/">Fody</a> (NuGet should automatically get this when you build)
  - <a href="https://nuget.org/packages/PropertyChanged.Fody">PropertyChanged.Fody</a> (NuGet should automatically get this when you build)
  - <a href="http://www.7-zip.org/download.html">7-Zip</a>, installed at "C:\Program Files\7-Zip\7z.exe" (not strictly necessary, part of the default post-build event)
 - If you contribute a new build, you should build with the Release configuration to automatically update the binaries zip. (it automatically updates every time you build, not just on Release, so try not to check in a Debug build)
