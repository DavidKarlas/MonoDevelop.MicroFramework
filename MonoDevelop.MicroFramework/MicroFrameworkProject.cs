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
using MonoDevelop.Ide;

namespace MonoDevelop.MicroFramework
{
	public class MicroFrameworkProject : DotNetProjectExtension
	{
		protected override void OnEndLoad ()
		{
			base.OnEndLoad ();
			if (Project.CompileTarget != CompileTarget.Library)
				ExecutionTargetsManager.DeviceListChanged += OnExecutionTargetsChanged;
		}

		public override void Dispose ()
		{
			base.Dispose ();
			ExecutionTargetsManager.DeviceListChanged -= OnExecutionTargetsChanged;
		}

		private void OnExecutionTargetsChanged (object dummy)
		{
			Runtime.RunInMainThread (delegate {
				base.OnExecutionTargetsChanged ();
			});
		}

		protected override IEnumerable<ExecutionTarget> OnGetExecutionTargets (ConfigurationSelector configuration)
		{
			return ExecutionTargetsManager.Targets;
		}

		protected override bool OnGetSupportsFormat (Projects.MSBuild.MSBuildFileFormat format)
		{
			return format.Id == "MSBuild10" || format.Id == "MSBuild12";
		}

		protected override bool OnGetSupportsFramework (TargetFramework framework)
		{
			return framework.Id.Identifier == ".NETMicroFramework";
		}

		protected override bool OnGetCanExecute (ExecutionContext context, ConfigurationSelector configuration)
		{
			if (IdeApp.Workspace.GetAllSolutions ().Any ((s) => s.StartupItem == this.Project)) {
				return context.ExecutionTarget is MicroFrameworkExecutionTarget && base.OnGetCanExecute (context, configuration);
			} else {
				return base.OnGetCanExecute (context, configuration);
			}
		}

		protected override TargetFrameworkMoniker OnGetDefaultTargetFrameworkId ()
		{
			return new TargetFrameworkMoniker (".NETMicroFramework", "4.3");
		}

		protected override TargetFrameworkMoniker OnGetDefaultTargetFrameworkForFormat (string toolsVersion)
		{
			//Keep default version invalid(1.0) or MonoDevelop will omit from serialization
			return new TargetFrameworkMoniker (".NETMicroFramework", "1.0");
		}

		protected override void Initialize ()
		{
			base.Initialize ();
			Project.ProjectProperties.SetValue ("NetMfTargetsBaseDir", "$(MSBuildExtensionsPath32)\\Microsoft\\.NET Micro Framework\\", condition: "'$(NetMfTargetsBaseDir)'==''");
			Project.RemoveImport ("$(MSBuildBinPath)\\Microsoft.CSharp.targets");
			Project.AddImportIfMissing ("$(NetMfTargetsBaseDir)$(TargetFrameworkVersion)\\CSharp.Targets", "");
		}

		protected override ExecutionCommand OnCreateExecutionCommand (ConfigurationSelector configSel, DotNetProjectConfiguration configuration)
		{
			var references = Project.GetReferencedAssemblies (configSel, true).ContinueWith (t => {
				return t.Result.Select<AssemblyReference, string> ((r) => {
					if (r.FilePath.IsAbsolute)
						return r.FilePath;
					return Project.GetAbsoluteChildPath (r.FilePath).FullPath;
				}).ToList();});
			return new MicroFrameworkExecutionCommand () {
				OutputDirectory = configuration.OutputDirectory,
				ReferencedAssemblies = references
			};
		}
	}
}
