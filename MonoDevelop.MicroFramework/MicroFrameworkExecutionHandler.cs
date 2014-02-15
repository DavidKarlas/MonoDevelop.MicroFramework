using Microsoft.SPOT.Debugger;
using Mono.Debugging.Client;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Debugger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace MonoDevelop.MicroFramework
{
	class MicroFrameworkExecutionHandler : IExecutionHandler
	{
		public bool CanExecute(ExecutionCommand command)
		{
			return command as MicroFrameworkExecutionCommand != null;
		}

		public IProcessAsyncOperation Execute(ExecutionCommand command, IConsole console)
		{
			var cmd = command as MicroFrameworkExecutionCommand;
//			using(Engine eng = new Engine((cmd.Target as MicroFrameworkExecutionTarget).PortDefinition))
//			{
//				eng.Start();
//				Engine.MessageHandler messageHandler = new Engine.MessageHandler((str) =>
//				{
//					console.Log.WriteLine(str);
//				});
//				var listOfAseemblies = new ArrayList();
//
//				//TODO: Check if this is robust enough will "be" and "le" really always be in output folder?
//				string dir = cmd.OutputDirectory;
//				if(eng.IsTargetBigEndian)
//					dir = Path.Combine(dir, "be");
//				else
//					dir = Path.Combine(dir, "le");
//
//				string[] files = Directory.GetFiles(dir, "*.pe");
//				foreach(var v in files)
//				{
//					using(var fs = new FileStream(v, FileMode.Open))
//					{
//						byte[] data = new byte[fs.Length];
//						fs.Read(data, 0, data.Length);
//						listOfAseemblies.Add(data);
//					}
//				}
//
//				eng.Deployment_Execute(listOfAseemblies, true, messageHandler);
//			}
			return new DebugExecutionHandler(null).Execute(cmd, console);
		}

		class DebugExecutionHandler : IProcessAsyncOperation
		{
			bool done;
			ManualResetEvent stopEvent;

			public DebugExecutionHandler(DebuggerEngine factory)
			{
				DebuggingService.StoppedEvent += new EventHandler(OnStopDebug);
			}

			public IProcessAsyncOperation Execute(ExecutionCommand command, IConsole console)
			{
				MethodInfo InternalRun = typeof(DebuggingService).GetMethod("InternalRun", BindingFlags.NonPublic | BindingFlags.Static);
				InternalRun.Invoke(null, new object[] { command, null, console });
//				DebuggingService.Run("", console);
				return this;
			}

			static DebuggerEngine GetFactoryForCommand(ExecutionCommand cmd)
			{
				foreach(DebuggerEngine factory in DebuggingService.GetDebuggerEngines())
				{
					if(factory.CanDebugCommand(cmd))
						return factory;
				}
				return null;
			}

			public void Cancel()
			{
				DebuggingService.Stop();
			}

			public void WaitForCompleted()
			{
				lock(this)
				{
					if(done)
						return;
					if(stopEvent == null)
						stopEvent = new ManualResetEvent(false);
				}
				stopEvent.WaitOne();
			}

			public int ExitCode
			{
				get { return 0; }
			}

			public bool IsCompleted
			{
				get { return done; }
			}

			public bool Success
			{
				get { return true; }
			}

			public bool SuccessWithWarnings
			{
				get { return true; }
			}

			void OnStopDebug(object sender, EventArgs args)
			{
				lock(this)
				{
					done = true;
					if(stopEvent != null)
						stopEvent.Set();
					if(completedEvent != null)
						completedEvent(this);
				}

				DebuggingService.StoppedEvent -= new EventHandler(OnStopDebug);
			}

			event OperationHandler completedEvent;

			event OperationHandler IAsyncOperation.Completed
			{
				add
				{
					bool raiseNow = false;
					lock(this)
					{
						if(done)
							raiseNow = true;
						else
							completedEvent += value;
					}
					if(raiseNow)
						value(this);
				}
				remove
				{
					lock(this)
					{
						completedEvent -= value;
					}
				}
			}

			public int ProcessId
			{
				get { return -1; }
			}

			void IDisposable.Dispose()
			{
			}
		}
	}
}
