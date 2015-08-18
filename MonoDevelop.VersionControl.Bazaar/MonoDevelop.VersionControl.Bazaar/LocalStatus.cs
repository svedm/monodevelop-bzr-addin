using System;
using System.Collections.Generic;

using MonoDevelop.Core;

namespace MonoDevelop.VersionControl.Bazaar
{
	public class LocalStatus {
		public readonly string Revision;
		public readonly string Filename;
		public ItemStatus Status;

		public LocalStatus (string revision, string filename, ItemStatus status) {
			Revision = revision;
			Filename = filename;
			Status = status;
		}// constructor
	}

}
