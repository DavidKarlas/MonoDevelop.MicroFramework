using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using Microsoft.Win32;
using System.Reflection;
using System.IO;

namespace MonoDevelop.MicroFramework
{
	class StartupHandler : CommandHandler
	{
		static void DirectoryCopy(string sourceDirName, string destDirName)
		{
			//TNX TO: http://stackoverflow.com/a/8865284/661901
			MonoDevelop.MacInterop.AppleScript.Run("do shell script \"cp -R \\\"" + sourceDirName + "\\\" \\\"" + destDirName + "\\\"\" with administrator privileges");
		}

		protected override void Run()
		{
			if(Platform.IsMac)
			{
				string addInFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:", "");
				var registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\.NETMicroFramework\\v4.3");
				if(registryKey.GetValue("InstallRoot") == null)
				{
					registryKey.SetValue("BuildNumber", "1");
					registryKey.SetValue("RevisionNumber", "0");
					registryKey.SetValue("InstallRoot", "/Library/Frameworks/Microsoft .NET Micro Framework/");
				}

				if(!Directory.Exists("/Library/Frameworks/Mono.framework/External/xbuild-frameworks/.NETMicroFramework"))
				{
					DirectoryCopy(Path.Combine(addInFolder, "files", "xbuild-framework/"), "/Library/Frameworks/Mono.framework/External/xbuild-frameworks/");
				}

				if(!Directory.Exists("/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild/Microsoft/.NET Micro Framework"))
				{
					DirectoryCopy(Path.Combine(addInFolder, "files", "xbuild/"),
						"/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild/");
				}

				if(!Directory.Exists("/Library/Frameworks/Microsoft .NET Micro Framework/"))
				{
					DirectoryCopy(Path.Combine(addInFolder, "files", "sdk/"),
						"/Library/Frameworks/Microsoft .NET Micro Framework/");
				}
			}
		}
	}
}

