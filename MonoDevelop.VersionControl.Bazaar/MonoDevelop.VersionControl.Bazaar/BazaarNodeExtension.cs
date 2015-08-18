using System;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;

namespace MonoDevelop.VersionControl.Bazaar
{
	public class BazaarNodeExtension : NodeBuilderExtension
	{
		public override bool CanBuildNode (Type dataType)
		{
			return typeof(ProjectFile).IsAssignableFrom (dataType)
				|| typeof(SystemFile).IsAssignableFrom (dataType)
				|| typeof(ProjectFolder).IsAssignableFrom (dataType)
				|| typeof(IWorkspaceObject).IsAssignableFrom (dataType);
		}

		public override Type CommandHandlerType {
			get { return typeof(BazaarCommandHandler); }
		}
	}
}

