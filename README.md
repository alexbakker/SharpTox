SharpTox [![Build Status](http://jenkins.impy.me/job/SharpTox/badge/icon)](http://jenkins.impy.me/job/SharpTox/)
========

This project aims to provide a simple library that wraps all of the functions found in the [Tox library](https://github.com/irungentoo/ProjectTox-Core "ProjectTox GitHub repo").
Tox is a free (as in freedom) Skype replacement.

Note: SharpTox is far from perfect. Feel free to contribute!

### Basic Usage
```csharp
using System;
using SharpTox;

class Program
{
    static Tox tox;

    static void Main(string[] args)
    {
        tox = new Tox(false);
        tox.OnFriendRequest += tox_OnFriendRequest;
        tox.OnFriendMessage += tox_OnFriendMessage;

        foreach (ToxNode node in Nodes)
            tox.BootstrapFromNode(node);

        tox.SetName("SharpTox");
        tox.SetStatusMessage("Testing SharpTox");

        string id = tox.GetAddress();
        Console.WriteLine("ID: {0}", id);

        tox.Start();
    }

    //check https://wiki.tox.im/Nodes for an up-to-date list of nodes
    static ToxNode[] Nodes = new ToxNode[]
    {
        new ToxNode("192.254.75.98", 33445, "951C88B7E75C867418ACDB5D273821372BB5BD652740BCDF623A4FA293E75D2F", false),
        new ToxNode("144.76.60.215", 33445, "04119E835DF3E78BACF0F84235B300546AF8B936F035185E2A8E9E0A67C8924F", false)
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

* The libtoxcore library, you should compile that yourself from the [ProjectTox GitHub repo](https://github.com/irungentoo/ProjectTox-Core "Tox Github repo").

* Depending on your operating system, Tox's core library could have more dependencies apart from libsodium (which you should already have compiled while following the compilation guidelines on the ProjectTox repo page). For Windows, this dependency is libgcc_s_dw2-1.dll.

Please note that at this time, this project hasn't been tested on any operating system other than Windows. You are of course, free to give it a try on the operating system of your choice. (some changes to the ToxFunctions class are needed for that though)

Toxy
-------
Toxy is basically just a WinForms client that can be used as an example to show how SharpTox works.
If you're considering using SharpTox for your project, you should definitely have a look at that. It should have most of the functionality of Tox implemented.

Toxy is available [here](https://github.com/Impyy/Toxy).

Contact
-------
* Join the official IRC channel #tox on freenode
[![Official Tox IRC Channel](https://kiwiirc.com/buttons/irc.freenode.net/tox.png)](https://kiwiirc.com/client/irc.freenode.net/?theme=basic#tox)
* Send me an email: [admin@impy.me](mailto:admin@impy.me)
