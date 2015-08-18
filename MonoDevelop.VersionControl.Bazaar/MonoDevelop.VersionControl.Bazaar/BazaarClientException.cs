using System;
using System.Collections.Generic;
using System.IO;
using MonoDevelop.Core;

namespace MonoDevelop.VersionControl.Bazaar
{
	public class BazaarClientException : Exception
	{
		public BazaarClientException(string message)
			: base(message)
		{
		}
	}

}