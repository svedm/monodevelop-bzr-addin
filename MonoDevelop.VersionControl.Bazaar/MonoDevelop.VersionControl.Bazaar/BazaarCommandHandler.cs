using System;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Components.Commands;
using System.Collections.Generic;
using MonoDevelop.Core;

namespace MonoDevelop.VersionControl.Bazaar
{
	public class BazaarCommandHandler : VersionControlCommandHandler
	{
		/// <summary>
		/// Determines whether the selected items can be resolved
		/// </summary>
		[CommandUpdateHandler (BazaarCommands.Resolve)]
		protected void CanResolve (CommandInfo item)
		{
			bool visible = true;

			foreach (VersionControlItem vcitem in GetItems ()) {
				if(!(visible = (vcitem.Repository is BazaarRepository &&
					((BazaarRepository)vcitem.Repository).CanResolve (vcitem.Path))))
					break;
			}

			item.Visible = visible;
		}// CanResolve

		[CommandHandler (BazaarCommands.Resolve)]
		protected void OnResolve()
		{
			List<FilePath> files = null;
			BazaarRepository repo = null;

			foreach (VersionControlItemList repolist in GetItems ().SplitByRepository ()) {
				repo = (BazaarRepository)repolist[0].Repository;
				files = new List<FilePath> (repolist.Count);
				foreach (VersionControlItem item in repolist) {
					files.Add (new FilePath (item.Path));
				}

				BazaarTask worker = new BazaarTask ();
				worker.Description = string.Format ("Resolving {0}", files[0]);
				worker.Operation = delegate{ repo.Resolve (files.ToArray (), true, worker.ProgressMonitor); };
				worker.Start ();
			}
		}// OnResolve
	}

}

