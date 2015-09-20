using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTox.Core
{
    public class ToxGroupEventArgs
    {
        public abstract class GroupBaseEventArgs : EventArgs
        {
            public int GroupNumber { get; private set; }

            protected GroupBaseEventArgs(int groupNumber)
            {
                GroupNumber = groupNumber;
            }
        }

        public abstract class GroupPeerBaseEventArgs : GroupBaseEventArgs
        {
            public int PeerNumber { get; private set; }

            protected GroupPeerBaseEventArgs(int groupNumber, int peerNumber)
                : base(groupNumber)
            {
                PeerNumber = peerNumber;
            }
        }

        public class PeerNameEventArgs : GroupPeerBaseEventArgs
        {
            public string Name { get; private set; }

            public PeerNameEventArgs(int groupNumber, int peerNumber, string name)
                : base(groupNumber, peerNumber)
            {
                Name = name;
            }
        }

        public class PeerStatusEventArgs : GroupPeerBaseEventArgs
        {
            public ToxUserStatus Status { get; private set; }

            public PeerStatusEventArgs(int groupNumber, int peerNumber, ToxUserStatus status)
                : base(groupNumber, peerNumber)
            {
                Status = status;
            }
        }

        public class TopicEventArgs : GroupPeerBaseEventArgs
        {
            public string Name { get; private set; }

            public TopicEventArgs(int groupNumber, int peerNumber, string name)
                : base(groupNumber, peerNumber)
            {
                Name = name;
            }
        }

        public class PrivacyStateEventArgs : GroupBaseEventArgs
        {
            public ToxGroupPrivacyState State { get; private set; }

            public PrivacyStateEventArgs(int groupNumber, ToxGroupPrivacyState state)
                : base(groupNumber)
            {
                State = state;
            }
        }

        public class PeerLimitEventArgs : GroupBaseEventArgs
        {
            public int Limit { get; private set; }

            public PeerLimitEventArgs(int groupNumber, int limit)
                : base(groupNumber)
            {
                Limit = limit;
            }
        }

        public class PasswordEventArgs : GroupBaseEventArgs
        {
            public string Password { get; private set; }

            public PasswordEventArgs(int groupNumber, string password)
                : base(groupNumber)
            {
                Password = password;
            }
        }

        public class MessageEventArgs : GroupPeerBaseEventArgs
        {
            public ToxMessageType Type { get; private set; }
            public string Message { get; private set; }

            public MessageEventArgs(int groupNumber, int peerNumber, ToxMessageType type, string message)
                : base(groupNumber, peerNumber)
            {
                Type = type;
                Message = message;
            }
        }

        public class PrivateMessageEventArgs : GroupPeerBaseEventArgs
        {
            public string Message { get; private set; }

            public PrivateMessageEventArgs(int groupNumber, int peerNumber, string message)
                : base(groupNumber, peerNumber)
            {
                Message = message;
            }
        }

        public class InviteEventArgs : ToxEventArgs.FriendBaseEventArgs
        {
            public byte[] InviteData { get; private set; }

            public InviteEventArgs(int friendNumber, byte[] inviteData)
                : base(friendNumber)
            {
                InviteData = inviteData;
            }
        }

        public class PeerJoinEventArgs : GroupPeerBaseEventArgs
        {
            public PeerJoinEventArgs(int groupNumber, int peerNumber)
                : base(groupNumber, peerNumber)
            { }
        }

        public class SelfJoinEventArgs : GroupBaseEventArgs
        {
            public SelfJoinEventArgs(int groupNumber)
                : base(groupNumber)
            { }
        }

        public class PeerExitEventArgs : GroupPeerBaseEventArgs
        {
            public string Message { get; private set; }

            public PeerExitEventArgs(int groupNumber, int peerNumber, string message)
                : base(groupNumber, peerNumber)
            {
                Message = message;
            }
        }

        public class JoinFailEventArgs : GroupBaseEventArgs
        {
            public ToxGroupJoinFail Error { get; private set; }

            public JoinFailEventArgs(int groupNumber, ToxGroupJoinFail failType)
                : base(groupNumber)
            {
                Error = failType;
            }
        }

        public class ModActionEventArgs : GroupPeerBaseEventArgs
        {
            public int TargetPeerNumber { get; private set; }
            public ToxGroupModEvent Event { get; private set; }

            public ModActionEventArgs(int groupNumber, int peerNumber, int targetPeerNumber, ToxGroupModEvent modEvent)
                : base(groupNumber, peerNumber)
            {
                TargetPeerNumber = targetPeerNumber;
                Event = modEvent;
            }
        }
    }
}
