MonoDevelop.MicroFramework is AddIn for [MonoDevelop/Xamarin Studio](https://github.com/mono/monodevelop) which adds support for developing and deploying to [.Net MicroFramework](http://www.netmf.com/) devices.

This AddIn is in very early stages of development it's mostly in prototype state at this moment. If you want to try it:
* Install latest Xamarin Studio(Windows only atm)
* Enable AddIn called "Addin Maker"(Tools->Add-in Manager->Gallery->Addin Development->Addin Maker->Install)
* Clone project from GitHub and open solution with Xamarin Studio
* Run project will open new Xamarin Studio which has MicroFramework AddIn enabled
* With new instance of Xmarain Studio open some MicroFramework project

## What it does ATM
* User can open MicroFramework projects
* Write code with MicroFramework libraries which limits IntelliSense only to possibilities available in MicroFramework.
* Compile, deploy and execute code with single F5(Run) click
* Displaying Debug.Print("Hello world!") in Application output window.

## TODO list
### Templates(Library/Executable) for creating new project
Nice job for anyone who wants to help ;)
### User interface for selecting USB/Serial/TCP and which device
This interface should be very similar to MonoTouch interface for selecting devices(so next to Run button and not in project properties)(Atm it picks up 1st MF.Net device on USB). Pull requests very welcom.
### Debugging support
Mostly what has to be done here is filling NotImplemented methods in [MicroFrameworkDebuggerSession](https://github.com/DavidKarlas/MonoDevelop.MicroFramework/blob/master/MonoDevelop.MicroFramework/MicroFrameworkDebuggerSession.cs) to see how to set breakpoints, see variable values, list threads, appDomains, processes, ILOffsets for breakpoints... See how it's done for VisualStudio plugin on [codeplex](http://netmf.codeplex.com/SourceControl/latest#client_v4_3/Framework/CorDebug/) which is under Apache 2.0 license. I will be focusing on this part atm.
### Port SDK Installer to Unix
I haven't investigated this much but I imagine some libraries have to be added to GAC, USB Drivers, MicroFramework.CSharp.Targets and "Registry" entries  for location of MetaDataProcessor and other tools needed for compiling.
### Port MetaDataProcessor.exe to Unix
MetaDataProcessor is used to convert full .Net exe/dll to MicroFramework exe/dll for example it removes Windows/DOS PE header but it does much more. Since MetaDataProcessor is written in C++ there are two paths to doing this. One is port C++ to Unix. Second option is to rewrite in C# where [Mono.Cecil](https://github.com/jbevain/cecil) will do all heavy lifting of parsing full .Net exe/dll. Temporary solution could be using Wine to run MetaDataProcessor.exe as seen [here](http://forums.netduino.com/index.php?/topic/1062-metadataprocessorexe-wine-notes/)
### Switch from WinUSB to LibUsbDotNet to be able to support Unix
I'm not sure about drivers required for MF.Net devices but I imagine it must be some generic driver since most communication is "serial". Temporary solution for Unix OSs could be using SerialPort instead of USB like seen on [NetDuino forums](http://forums.netduino.com/index.php?/topic/285-mfdeploy-v41-for-mac-os-x-and-linux-alpha-1/).
