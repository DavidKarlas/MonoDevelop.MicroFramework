using Microsoft.SPOT.Debugger;
using MonoDevelop.Core;
using MonoDevelop.Core.Assemblies;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Projects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MonoDevelop.Core.Serialization;

namespace MonoDevelop.MicroFramework
{
	public class MicroFrameworkProject : DotNetProject
	{
		System.Threading.Timer timer;
		public override void Dispose()
		{
			if(timer != null)
			{
				timer.Dispose();
				timer = null;
			}
			base.Dispose();
		}

		private void Init()
		{
			timer = new System.Threading.Timer(new System.Threading.TimerCallback(updateTargetsList), null, 0, 1000);
		}
		List<MicroFrameworkExecutionTarget> targetsList=new List<MicroFrameworkExecutionTarget>();
		private void updateTargetsList(object state)
		{
			var devices = PortDefinition.Enumerate(PortFilter.Usb);
			var targetsToKeep = new List<MicroFrameworkExecutionTarget>();
			bool changed = false;
			foreach(var device in devices)
			{
				bool targetExist = false;
				foreach(var target in targetsList)
				{
					if(target.PortDefinition.UniqueId == (device as PortDefinition).UniqueId)
					{
						targetsToKeep.Add(target);
						targetExist = true;
						break;
					}
				}
				if(!targetExist)
				{
					changed = true;
					var newTarget = new MicroFrameworkExecutionTarget(device as PortDefinition);
					targetsList.Add(newTarget);
					targetsToKeep.Add(newTarget);
				}
			}
			changed |= targetsList.RemoveAll((target) => !targetsToKeep.Contains(target)) > 0;
			if(changed)
				OnExecutionTargetsChanged();
		}

		public MicroFrameworkProject()
			: base()
		{
			Init();
		}

		public MicroFrameworkProject(string languageName)
			: base(languageName)
		{
			Init();
		}

		public MicroFrameworkProject(string languageName, ProjectCreateInformation projectCreateInfo, XmlElement projectOptions)
			: base(languageName, projectCreateInfo, projectOptions)
		{
			Init();
		}

		protected override IEnumerable<ExecutionTarget> OnGetExecutionTargets(ConfigurationSelector configuration)
		{
			return targetsList;
		}

		public override bool SupportsFramework (TargetFramework framework)
		{
			return framework.Id.Identifier == ".NETMicroFramework";
		}

		public override bool SupportsFormat (FileFormat format)
		{
			return format.Id == "MSBuild10" || format.Id == "MSBuild12";
		}

		protected override bool OnGetCanExecute(ExecutionContext context, ConfigurationSelector configuration)
		{
			return context.ExecutionTarget is MicroFrameworkExecutionTarget;
		}

		public override TargetFrameworkMoniker GetDefaultTargetFrameworkForFormat(FileFormat format)
		{
			//Keep default version invalid(1.0) or MonoDevelop will omit from serialization
			return new TargetFrameworkMoniker(".NETMicroFramework", "1.0");
		}

		public override TargetFrameworkMoniker GetDefaultTargetFrameworkId()
		{
			return new TargetFrameworkMoniker(".NETMicroFramework", "4.3");
		}
		//Seems like VS is ignoring this
		//So we won't implement it my guess is they removed becauese it was causing
		//problems with version control and multi users projects
		//<DeployDevice>Netduino</DeployDevice>
		//<DeployTransport>USB</DeployTransport>

		//TODO: Add attribute Condition="'$(NetMfTargetsBaseDir)'==''"
		[ItemProperty("NetMfTargetsBaseDir")]
		string netMfTargetsBaseDir = "$(MSBuildExtensionsPath32)\\Microsoft\\.NET Micro Framework\\";

		public string NetMfTargetsBaseDir
		{
			get
			{
				return netMfTargetsBaseDir;
			}
			set
			{
				if(netMfTargetsBaseDir == value)
					return;
				netMfTargetsBaseDir = value;
				NotifyModified("NetMfTargetsBaseDir");
			}
		}

		protected override ExecutionCommand CreateExecutionCommand(ConfigurationSelector configSel, DotNetProjectConfiguration configuration)
		{
			return new MicroFrameworkExecutionCommand() {
				OutputDirectory = configuration.OutputDirectory
			};
		}
	}
}
