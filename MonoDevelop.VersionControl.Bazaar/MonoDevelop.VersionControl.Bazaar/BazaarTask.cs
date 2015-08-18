using System;
using System.Threading;
using MonoDevelop.Ide;
using MonoDevelop.Core;

namespace MonoDevelop.VersionControl.Bazaar
{
	public delegate void BazaarOperation();

	public class BazaarTask
	{
		public string Description{ get; set; }

		public BazaarOperation Operation{ get; set; }

		public IProgressMonitor ProgressMonitor{ get; protected set; }

		public BazaarTask()
			: this(string.Empty, null)
		{
		}

		public BazaarTask(string description, BazaarOperation operation)
		{
			ProgressMonitor = IdeApp.Workbench.ProgressMonitors.GetOutputProgressMonitor("Version Control", null, true, true);
			Description = description;
			Operation = operation;
		}

		public void Start()
		{
			ThreadPool.QueueUserWorkItem(delegate
				{
					try
					{
						ProgressMonitor.BeginTask(Description, 0);
						Operation();
						ProgressMonitor.ReportSuccess(GettextCatalog.GetString("Done."));
					}
					catch (Exception e)
					{
						ProgressMonitor.ReportError(e.Message, e);
					}
					finally
					{
						ProgressMonitor.EndTask();
						ProgressMonitor.Dispose();
					}
				});
		}
	}
}

