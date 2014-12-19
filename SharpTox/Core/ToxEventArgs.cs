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
            public int PeerNumber { get; private set; }

            protected GroupBaseEventArgs(ToxGroup group, int peerNumber)
            {
                Group = group;
                PeerNumber = peerNumber;
            }
        }

        public abstract class FileBaseEventArgs : FriendBaseEventArgs
        {
            public int FileNumber { get; private set; }

            protected FileBaseEventArgs(ToxFriend friend, int fileNumber)
                : base(friend)
            {
                FileNumber = fileNumber;
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
            public byte[] Data { get; private set; }

            public ToxGroupType GroupType { get; private set; }

            public GroupInviteEventArgs(ToxFriend friend, ToxGroupType type, byte[] data)
                : base(friend)
            {
                Data = (byte[])data.Clone();
                GroupType = type; 
            }
        }

        public class GroupMessageEventArgs : GroupBaseEventArgs
        {
            public string Message { get; private set; }

            public GroupMessageEventArgs(ToxGroup group, int peerNumber, string message)
                : base(group, peerNumber)
            {
                Message = message;
            }
        }

        public class GroupActionEventArgs : GroupBaseEventArgs
        {
            public string Action { get; private set; }

            public GroupActionEventArgs(ToxGroup group, int peerNumber, string action)
                : base(group, peerNumber)
            {
                Action = action;
            }
        }

        public class GroupNamelistChangeEventArgs : GroupBaseEventArgs
        {
            public ToxChatChange Change { get; private set; }

            public GroupNamelistChangeEventArgs(ToxGroup group, int peerNumber, ToxChatChange change)
                : base(group, peerNumber)
            {
                Change = change;
            }
        }

        public class FileControlEventArgs : FileBaseEventArgs
        {
            public ToxFileControl Control{get;private set;}

            public bool IsSend { get; private set; }

            public byte[] Data { get; private set; }

            public FileControlEventArgs(ToxFriend friend, int fileNumber, bool isSend, ToxFileControl control, byte[] data)
                : base(friend, fileNumber)
            {
                IsSend = isSend;
                Control = control;
                Data = (byte[])data.Clone();
            }
        }

        public class FileDataEventArgs : FileBaseEventArgs
        {
            public byte[] Data { get; private set; }

            public FileDataEventArgs(ToxFriend friend, int fileNumber, byte[] data)
                : base(friend, fileNumber)
            {
                Data = (byte[])data.Clone();
            }
        }

        public class FileSendRequestEventArgs : FileBaseEventArgs
        {
            public ulong FileSize { get; private set; }

            public string FileName { get; private set; }

            public FileSendRequestEventArgs(ToxFriend friend, int fileNumber, ulong fileSize, string fileName)
                : base(friend, fileNumber)
            {
                FileSize = fileSize;
                FileName = fileName;
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

            public GroupTitleEventArgs(ToxGroup group, int peerNumber, string title)
                : base(group, peerNumber)
            {
                Title = title;
            }
        }
    }
}
