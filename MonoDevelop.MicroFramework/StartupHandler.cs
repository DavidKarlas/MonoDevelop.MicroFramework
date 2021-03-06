﻿using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using MonoDevelop.Ide;
using System.Security.Cryptography;

namespace MonoDevelop.MicroFramework
{
	class StartupHandler : CommandHandler
	{
		static MethodInfo AppleScript;

		static void RunAppleScript(string str)
		{
			if(AppleScript == null)
			{
				var type = Type.GetType("MonoDevelop.MacInterop.AppleScript, MacPlatform");
				AppleScript = type.GetMethod("Run", new []{ typeof(string) });
			}
			AppleScript.Invoke(null, new object[]{ str });
		}

		static void DirectoryCopy(string sourceDirName, string destDirName)
		{
			//TNX TO: http://stackoverflow.com/a/8865284/661901
			RunAppleScript("do shell script \"cp -R \\\"" + sourceDirName + "\\\" \\\"" + destDirName + "\\\"\" with administrator privileges");
		}

		private static string GetChecksum(string file)
		{
			if(!File.Exists(file))
				return "";
			using(var stream = File.OpenRead(file))
			using(var sha = new SHA256Managed())
			{
				byte[] checksum = sha.ComputeHash(stream);
				return BitConverter.ToString(checksum);
			}
		}

		protected override void Run()
		{
			if(Platform.IsMac)
			{
				string addInFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:", "");
				var registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\.NETMicroFramework\\v4.3", true);
				if(registryKey == null)
				{
					registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\.NETMicroFramework\\v4.3");
				}
				if(registryKey.GetValue("InstallRoot") == null)
				{
					registryKey.SetValue("BuildNumber", "1");
					registryKey.SetValue("RevisionNumber", "0");
					registryKey.SetValue("InstallRoot", "/Library/Frameworks/Microsoft .NET Micro Framework/v4.3");
				}
				var assFolderKey = registryKey.OpenSubKey("AssemblyFolder", true);
				if(assFolderKey == null)
				{
					assFolderKey = registryKey.CreateSubKey("AssemblyFolder");
					assFolderKey.SetValue("", "");
				}
				bool newlyInstalled = false;
				if(!Directory.Exists("/Library/Frameworks/Mono.framework/External/xbuild-frameworks/.NETMicroFramework") ||
				   !File.Exists("/Library/Frameworks/Mono.framework/External/xbuild-frameworks/.NETMicroFramework/v4.3/Microsoft.SPOT.Hardware.PWM.dll"))
				{
					DirectoryCopy(Path.Combine(addInFolder, "files", "xbuild-framework/"), "/Library/Frameworks/Mono.framework/External/xbuild-frameworks/");
					newlyInstalled = true;
				}

				if(!Directory.Exists("/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild/Microsoft/.NET Micro Framework") ||
				   (GetChecksum("/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild/Microsoft/.NET Micro Framework/v4.3/CSharp.targets") != GetChecksum(Path.Combine(addInFolder, "files/xbuild/Microsoft/.NET Micro Framework/v4.3/CSharp.targets"))))
				{
					DirectoryCopy(Path.Combine(addInFolder, "files", "xbuild/"),
						"/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild/");
					newlyInstalled = true;
				}

				if(!Directory.Exists("/Library/Frameworks/Microsoft .NET Micro Framework/v4.3/") ||
				   (GetChecksum("/Library/Frameworks/Microsoft .NET Micro Framework/v4.3/Tools/MetaDataProcessor.exe") != GetChecksum(Path.Combine(addInFolder, "files/frameworks/Microsoft .NET Micro Framework/v4.3/Tools/MetaDataProcessor.exe"))) ||
				   !File.Exists("/Library/Frameworks/Microsoft .NET Micro Framework/v4.3/Tools/MetaDataProcessor"))
				{
					DirectoryCopy(Path.Combine(addInFolder, "files", "frameworks/"),
						"/Library/Frameworks/");
					RunAppleScript("do shell script \"chmod +x \\\"/Library/Frameworks/Microsoft .NET Micro Framework/v4.3/Tools/MetaDataProcessor.exe\\\"\" with administrator privileges");
					RunAppleScript("do shell script \"chmod +x \\\"/Library/Frameworks/Microsoft .NET Micro Framework/v4.3/Tools/MetaDataProcessor\\\"\" with administrator privileges");
					newlyInstalled = true;
				}
				if(newlyInstalled)
				{
					MessageService.ShowMessage("MicroFramework .Net AddIn succesfully installed. Please restart Xamarin Studio to finish installation.");
				}
			}
		}
	}
}

