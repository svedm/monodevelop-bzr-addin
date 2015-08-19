# Bazaar support

[![Join the chat at https://gitter.im/svedm/monodevelop-bzr-addin](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/svedm/monodevelop-bzr-addin?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
Bazaar support for the MonoDevelop Version Control Add-in

[![Build Status](https://travis-ci.org/svedm/monodevelop-bzr-addin.svg?branch=master)](https://travis-ci.org/svedm/monodevelop-bzr-addin)

#Requirements
### On Mac OS X Yosemite
After installing bazaar for it normal working you should:

1. Edit the file `/usr/local/bin/bzr` (with sudo) and complete the first line:
`#!/usr/bin/python`
to
`#!/usr/bin/python2.6`

2. Run `sudo ln -s /Applications/Xcode.app/Contents/Developer/usr/lib/* /usr/lib`

### On Windows x86
Install [python2.6 based version of bazaar](http://wiki.bazaar.canonical.com/WindowsDownloads)

### On Windows x64
This OS is unsupported because bazaar api does not work properly on it.

# Instalation
Most users should install this from the MD/XS Addin Gallery from alpha channel. However, if you wish to contribute to the addin, you will need to build it from source. This may also be necessary if you wish to use it with some unreleased version of MonoDevelop or Xamarin Studio.

# Building
1. Clone `git clone https://github.com/svedm/monodevelop-bzr-addin.git --recursive`
2. [Build MonoDevelop](http://www.monodevelop.com/developers/building-monodevelop/) from External/MonoDevelop folder
3. Install [AddinMaker](https://github.com/mhutch/MonoDevelop.AddinMaker) from Addin Galery
4. Open MonoDevelop.VersionControl.Mercurial.sln from MonoDevelop.VersionControl.Mercurial with MD/XS
5. Build
