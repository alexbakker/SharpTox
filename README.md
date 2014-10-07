SharpTox [![Build Status](https://jenkins.impy.me/job/SharpTox%20x86/badge/icon)](https://jenkins.impy.me/job/SharpTox%20x86/)
========

This project aims to provide a simple library that wraps all of the functions found in the [Tox library](https://github.com/irungentoo/ProjectTox-Core "ProjectTox GitHub repo").
Tox is a free (as in freedom) Skype replacement.

Feel free to contribute!

### Basic Usage
```csharp
using System;
using SharpTox.Core;

class Program
{
    static Tox tox;

    static void Main(string[] args)
    {
        ToxOptions options = new ToxOptions(true, false);

        tox = new Tox(options);
        tox.OnFriendRequest += tox_OnFriendRequest;
        tox.OnFriendMessage += tox_OnFriendMessage;

        foreach (ToxNode node in Nodes)
            tox.BootstrapFromNode(node);

        tox.SetName("SharpTox");
        tox.SetStatusMessage("Testing SharpTox");

        tox.Start();

        string id = tox.GetAddress();
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

    static void tox_OnFriendMessage(int friendnumber, string message)
    {
        //get the name associated with the friendnumber
        string name = tox.GetName(friendnumber);

        //print the message to the console
        Console.WriteLine("<{0}> {1}", name, message);
    }

    static void tox_OnFriendRequest(string id, string message)
    {
        //automatically accept every friend request we receive
        tox.AddFriendNoRequest(id);
    }
}
```

### Things you'll need

* The libtox(core, av and dns) library, you should compile that yourself from the [ProjectTox GitHub repo](https://github.com/irungentoo/ProjectTox-Core "Tox Github repo"). Guidelines on how to do this can be found [here](https://github.com/irungentoo/toxcore/blob/master/INSTALL.md "Crosscompile guidelines"). If you don't feel like compiling this yourself, you can find automatic builds for windows here: [x86](https://jenkins.libtoxcore.so/job/toxcore_win32_dll/ "x86 dll") or [x64](https://jenkins.libtoxcore.so/job/toxcore_win64_dll/ "x64 dll")

* Depending on how you compiled the core libraries, the names of those may differ from the defaults in SharpTox. Be sure to change the value of the const string **dll** in ToxFunctions.cs, ToxAvFunctions.cs and ToxDnsFunctions.cs accordingly.

Please note that at this time, this project hasn't been tested on any operating system other than Windows. You are of course, free to give it a try on the operating system of your choice.

Toxy
-------
If you're considering using SharpTox for your project, you should definitely have a look at Toxy. Toxy is a metro-style Tox client for windows. It should have most of the functionality of Tox implemented.

Toxy is available [here](https://github.com/Reverp/Toxy-WPF).

Contact
-------
* Join the official IRC channel #tox on freenode
[![Official Tox IRC Channel](https://kiwiirc.com/buttons/irc.freenode.net/tox.png)](https://kiwiirc.com/client/irc.freenode.net/?theme=basic#tox)
* Send me an email: [admin@impy.me](mailto:admin@impy.me)
