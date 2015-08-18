using System;

namespace MonoDevelop.VersionControl.Bazaar
{
	public class BazaarRevision : Revision
	{
		public readonly string Rev;

		public static readonly string HEAD = "-1";
		public static readonly string FIRST = "1";
		public static readonly string NONE = "NONE";

		public BazaarRevision(Repository repo, string rev)
			: base(repo)
		{
			Rev = rev;
		}

		public BazaarRevision(Repository repo, string rev, DateTime time, string author, string message)
			: base(repo, time, author, message)
		{
			Rev = rev;
		}


		#region implemented abstract members of Revision

		public override Revision GetPrevious()
		{
			throw new NotImplementedException();
		}

		#endregion
		
	}
}
