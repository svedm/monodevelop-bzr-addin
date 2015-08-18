using System;
using System.Collections.Generic;

using MonoDevelop.Core;

namespace MonoDevelop.VersionControl.Bazaar
{
	public enum ItemStatus
	{
		Unversioned,
		Unchanged = ' ',
		Added = 'N',
		Conflicted = 'C',
		Deleted = 'D',
		Ignored = 'I',
		Modified = 'M',
		Replaced = 'R'
	}


}
