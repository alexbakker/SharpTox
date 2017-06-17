SharpTox [![Build Status](https://jenkins.impy.me/job/SharpTox%20x86/badge/icon)](https://jenkins.impy.me/job/SharpTox%20x86/)
========

This project aims to provide a simple library that wraps all of the functions found in the [Tox library](https://github.com/irungentoo/ProjectTox-Core "ProjectTox GitHub repo").
Tox is a free (as in freedom) Skype replacement.

Feel free to contribute!

### Things you'll need

* The libtox(core, av and dns) library, you should compile that yourself from the [ProjectTox GitHub repo](https://github.com/irungentoo/ProjectTox-Core "Tox Github repo"). Guidelines on how to do this can be found [here](https://github.com/irungentoo/toxcore/blob/master/INSTALL.md "Crosscompile guidelines"). If you don't feel like compiling this yourself, you can find automatic builds for windows here: [x86](https://build.tox.chat/view/libtoxcore/job/libtoxcore_build_windows_x86_shared_release/ "x86 dll") or [x64](https://build.tox.chat/view/libtoxcore/job/libtoxcore_build_windows_x86_64_shared_release/ "x64 dll")

Depending on how you compiled the core libraries, the names of those may differ from the defaults in SharpTox. Be sure to change the value of the const string **dll** in ToxFunctions.cs, ToxAvFunctions.cs, ToxDnsFunctions.cs and ToxEncryptionFunctions.cs accordingly if needed.

### Compiling and Testing
Assuming you have the libraries mentioned above installed, it's time to compile (and test) SharpTox.
##### Windows
* Clone this repository.
* Open SharpTox.sln in Visual Studio.
* Let Visual Studio restore the NuGet packages and build the solution.
* Optionally, run the tests by clicking 'Run All' in the Test Explorer.

Or from the command line (Be sure to install [NUnit](http://www.nunit.org/index.php?p=download) and [NuGet](https://nuget.codeplex.com/) first):

```
git clone https://github.com/alexbakker/SharpTox
nuget restore
msbuild /p:Configuration:Debug
nunit-console-x86 SharpTox.Tests/bin/Debug/SharpTox.Tests.dll
```

##### Linux
* Install 'mono-complete' (this should include nunit), grab the latest version of [NuGet](https://nuget.codeplex.com/) and execute the following commands:
```
git clone https://github.com/alexbakker/SharpTox
mono NuGet.exe restore
xbuild /p:Configuration="Debug POSIX"
nunit-console4 SharpTox.Tests/bin/Debug/SharpTox.Tests.dll
```
If you're having issues obtaining the NuGet packages, try executing:
```mozroots --import --sync```

**Looking for precompiled binaries? [Check this](https://jenkins.impy.me/ "SharpTox Binaries").**

### Basic Usage
```csharp
using System;
using SharpTox.Core;

class Program
{
    static Tox tox;

    static void Main(string[] args)
    {
        ToxOptions options = new ToxOptions(true, true);

        tox = new Tox(options);
        tox.OnFriendRequestReceived += tox_OnFriendRequestReceived;
        tox.OnFriendMessageReceived += tox_OnFriendMessageReceived;

        foreach (ToxNode node in Nodes)
            tox.Bootstrap(node);

        tox.Name = "SharpTox";
        tox.StatusMessage = "Testing SharpTox";

        tox.Start();

        string id = tox.Id.ToString();
        Console.WriteLine("ID: {0}", id);

        Console.ReadKey();
        tox.Dispose();
    }

    //check https://wiki.tox.im/Nodes for an up-to-date list of nodes
    static ToxNode[] Nodes = new ToxNode[]
    {
        new ToxNode("192.254.75.98", 33445, new ToxKey(ToxKeyType.Public, "951C88B7E75C867418ACDB5D273821372BB5BD652740BCDF623A4FA293E75D2F")),
        new ToxNode("144.76.60.215", 33445, new ToxKey(ToxKeyType.Public, "04119E835DF3E78BACF0F84235B300546AF8B936F035185E2A8E9E0A67C8924F"))
    };

    static void tox_OnFriendMessageReceived(object sender, ToxEventArgs.FriendMessageEventArgs e)
    {
        //get the name associated with the friendnumber
        string name = tox.GetFriendName(e.FriendNumber);

        //print the message to the console
        Console.WriteLine("<{0}> {1}", name, e.Message);
    }

    static void tox_OnFriendRequestReceived(object sender, ToxEventArgs.FriendRequestEventArgs e)
    {
        //automatically accept every friend request we receive
        tox.AddFriendNoRequest(e.PublicKey);
    }
}

```

Contact
-------
* Join the official IRC channel #tox-dev on freenode
[![Official Tox Dev IRC Channel](https://kiwiirc.com/buttons/irc.freenode.net/tox-dev.png)](https://kiwiirc.com/client/irc.freenode.net/?theme=basic#tox-dev)
