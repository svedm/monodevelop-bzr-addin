using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly:Addin("VersionControl.Bazaar", 
	Namespace = "MonoDevelop",
	Version = "0.1",
	Category = "Version Control")]

[assembly:AddinName("Bazaar support")]
[assembly:AddinDescription("Bazaar Mercurial support for the Version Control Add-in")]
[assembly:AddinUrl("https://github.com/svedm/monodevelop-bzr-addin")]
[assembly:AddinAuthor("Svetoslav Karasev")]

[assembly:AddinDependency ("Core", MonoDevelop.BuildInfo.Version)]
[assembly:AddinDependency ("Ide", MonoDevelop.BuildInfo.Version)]
[assembly:AddinDependency ("VersionControl", MonoDevelop.BuildInfo.Version)]