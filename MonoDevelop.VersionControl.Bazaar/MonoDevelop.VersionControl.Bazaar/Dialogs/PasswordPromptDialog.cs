using System;

namespace MonoDevelop.VersionControl.Bazaar
{
	public partial class PasswordPromptDialog : Gtk.Dialog
	{
		public PasswordPromptDialog(string prompt)
		{
			this.Build();
			this.promptLabel.Text = GLib.Markup.EscapeText (prompt);
		}
	}
}

