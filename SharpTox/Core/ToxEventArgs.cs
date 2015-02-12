using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTox.Core
{
    public class ToxEventArgs
    {
        #region Base Classes
        public abstract class FriendBaseEventArgs : EventArgs
        {
            public ToxFriend Friend { get; private set; }

            protected FriendBaseEventArgs(ToxFriend friend)
            {
                Friend = friend;
            }
        }

        public abstract class GroupBaseEventArgs : EventArgs
        {
            public ToxGroup Group { get; private set; }
            public ToxGroupPeer Peer { get; private set; }

            protected GroupBaseEventArgs(ToxGroup group, ToxGroupPeer peer)
            {
                Group = group;
                Peer = peer;
            }
        }

        public abstract class FileBaseEventArgs : FriendBaseEventArgs
        {
            public ToxFileSender FileSender { get; private set; }

            protected FileBaseEventArgs(ToxFriend friend, ToxFileSender fileSender)
                : base(friend)
            {
                FileSender = fileSender;
            }
        }
        #endregion

        public class FriendRequestEventArgs : EventArgs
        {
            public string Id { get; private set; }
            public string Message { get; private set; }

            public FriendRequestEventArgs(string id, string message)
            {
                Id = id;
                Message = message;
            }
        }

        public class ConnectionStatusEventArgs : FriendBaseEventArgs
        {
            public ToxFriendConnectionStatus Status { get; private set; }

            public ConnectionStatusEventArgs(ToxFriend friend, ToxFriendConnectionStatus status)
                : base(friend)
            {
                Status = status;
            }
        }

        public class FriendMessageEventArgs : FriendBaseEventArgs
        {
            public string Message { get; private set; }

            public FriendMessageEventArgs(ToxFriend friend, string message)
                : base(friend)
            {
                Message = message;
            }
        }

        public class FriendActionEventArgs : FriendBaseEventArgs
        {
            public string Action { get; private set; }

            public FriendActionEventArgs(ToxFriend friend, string action)
                : base(friend)
            {
                Action = action;
            }
        }

        public class NameChangeEventArgs : FriendBaseEventArgs
        {
            public string Name { get; private set; }

            public NameChangeEventArgs(ToxFriend friend, string name)
                : base(friend)
            {
                Name = name;
            }
        }

        public class StatusMessageEventArgs : FriendBaseEventArgs
        {
            public string StatusMessage { get; private set; }

            public StatusMessageEventArgs(ToxFriend friend, string statusMessage)
                : base(friend)
            {
                StatusMessage = statusMessage;
            }
        }

        public class UserStatusEventArgs : FriendBaseEventArgs
        {
            public ToxUserStatus UserStatus { get; private set; }

            public UserStatusEventArgs(ToxFriend friend, ToxUserStatus userStatus)
                : base(friend)
            {
                UserStatus = userStatus;
            }
        }

        public class TypingStatusEventArgs : FriendBaseEventArgs
        {
            public bool IsTyping { get; private set; }

            public TypingStatusEventArgs(ToxFriend friend, bool isTyping)
                : base(friend)
            {
                IsTyping = isTyping;
            }
        }

        public class GroupInviteEventArgs : FriendBaseEventArgs
        {
            public ToxGroupInvite GroupInvite { get; private set; }

            public GroupInviteEventArgs(ToxGroupInvite groupInvite)
                : base(groupInvite.Friend)
            {
                GroupInvite = groupInvite;
            }
        }

        public class GroupMessageEventArgs : GroupBaseEventArgs
        {
            public string Message { get; private set; }

            public GroupMessageEventArgs(ToxGroup group, ToxGroupPeer peer, string message)
                : base(group, peer)
            {
                Message = message;
            }
        }

        public class GroupActionEventArgs : GroupBaseEventArgs
        {
            public string Action { get; private set; }

            public GroupActionEventArgs(ToxGroup group, ToxGroupPeer peer, string action)
                : base(group, peer)
            {
                Action = action;
            }
        }

        public class GroupNamelistChangeEventArgs : GroupBaseEventArgs
        {
            public ToxChatChange Change { get; private set; }

            public GroupNamelistChangeEventArgs(ToxGroup group, ToxGroupPeer peer, ToxChatChange change)
                : base(group, peer)
            {
                Change = change;
            }
        }

        public class FileControlEventArgs : FileBaseEventArgs
        {
            public ToxFileControl Control{ get;private set; }

            public bool IsSend { get; private set; }

            public byte[] Data { get; private set; }

            public FileControlEventArgs(ToxFriend friend, ToxFileSender fileSender, bool isSend, ToxFileControl control, byte[] data)
                : base(friend, fileSender)
            {
                IsSend = isSend;
                Control = control;
                Data = (byte[])data.Clone();
            }
        }

        public class FileDataEventArgs : FileBaseEventArgs
        {
            public byte[] Data { get; private set; }

            public FileDataEventArgs(ToxFriend friend, ToxFileSender fileSender, byte[] data)
                : base(friend, fileSender)
            {
                Data = (byte[])data.Clone();
            }
        }

        public class FileSendRequestEventArgs : FileBaseEventArgs
        {
            public FileSendRequestEventArgs(ToxFriend friend, ToxFileSender fileSender)
                : base(friend, fileSender)
            {
            }
        }

        public class ReadReceiptEventArgs : FriendBaseEventArgs
        {
            public int Receipt { get; private set; }

            public ReadReceiptEventArgs(ToxFriend friend, int receipt)
                : base(friend)
            {
                Receipt = receipt;
            }
        }

        public class CustomPacketEventArgs : FriendBaseEventArgs
        {
            public byte[] Packet { get; private set; }

            public CustomPacketEventArgs(ToxFriend friend, byte[] packet)
                : base(friend)
            {
                Packet = (byte[])packet.Clone();
            }
        }

        public class ConnectionEventArgs : EventArgs
        {
            public bool IsConnected { get; private set; }

            public ConnectionEventArgs(bool isConnected)
            {
                IsConnected = isConnected;
            }
        }

        public class AvatarInfoEventArgs : FriendBaseEventArgs
        {
            public ToxAvatarFormat Format { get; private set; }

            public byte[] Hash { get; private set; }

            public AvatarInfoEventArgs(ToxFriend friend, ToxAvatarFormat format, byte[] hash)
                : base(friend)
            {
                Format = format;
                Hash = (byte[])hash.Clone();
            }
        }

        public class AvatarDataEventArgs : FriendBaseEventArgs
        {
            public ToxAvatar Avatar { get; private set; }

            public AvatarDataEventArgs(ToxFriend friend, ToxAvatar avatar)
                : base(friend)
            {
                Avatar = avatar;
            }
        }

        public class GroupTitleEventArgs : GroupBaseEventArgs
        {
            public string Title { get; private set; }

            public GroupTitleEventArgs(ToxGroup group, ToxGroupPeer peer, string title)
                : base(group, peer)
            {
                Title = title;
            }
        }
    }
}
