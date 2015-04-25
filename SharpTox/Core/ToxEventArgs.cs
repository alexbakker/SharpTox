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
            public int FriendNumber { get; private set; }

            protected FriendBaseEventArgs(int friendNumber)
            {
                FriendNumber = friendNumber;
            }
        }

        public abstract class GroupBaseEventArgs : EventArgs
        {
            public int GroupNumber { get; private set; }
            public int PeerNumber { get; private set; }

            protected GroupBaseEventArgs(int groupNumber, int peerNumber)
            {
                GroupNumber = groupNumber;
                PeerNumber = peerNumber;
            }
        }

        public abstract class FileBaseEventArgs : FriendBaseEventArgs
        {
            public int FileNumber { get; private set; }

            protected FileBaseEventArgs(int friendNumber, int fileNumber)
                : base(friendNumber)
            {
                FileNumber = fileNumber;
            }
        }
        #endregion

        public class FriendRequestEventArgs : EventArgs
        {
            public ToxKey PublicKey { get; private set; }
            public string Message { get; private set; }

            public FriendRequestEventArgs(ToxKey publicKey, string message)
            {
                PublicKey = publicKey;
                Message = message;
            }
        }

        public class ConnectionStatusEventArgs : EventArgs
        {
            public ToxConnectionStatus Status { get; private set; }

            public ConnectionStatusEventArgs(ToxConnectionStatus status)
            {
                Status = status;
            }
        }

        public class FriendConnectionStatusEventArgs : FriendBaseEventArgs
        {
            public ToxConnectionStatus Status { get; private set; }

            public FriendConnectionStatusEventArgs(int friendNumber, ToxConnectionStatus status)
                : base(friendNumber)
            {
                Status = status;
            }
        }

        public class FriendMessageEventArgs : FriendBaseEventArgs
        {
            public string Message { get; private set; }

            public ToxMessageType MessageType { get; private set; }

            public FriendMessageEventArgs(int friendNumber, string message, ToxMessageType type)
                : base(friendNumber)
            {
                Message = message;
                MessageType = type;
            }
        }

        public class NameChangeEventArgs : FriendBaseEventArgs
        {
            public string Name { get; private set; }

            public NameChangeEventArgs(int friendNumber, string name)
                : base(friendNumber)
            {
                Name = name;
            }
        }

        public class StatusMessageEventArgs : FriendBaseEventArgs
        {
            public string StatusMessage { get; private set; }

            public StatusMessageEventArgs(int friendNumber, string statusMessage)
                : base(friendNumber)
            {
                StatusMessage = statusMessage;
            }
        }

        public class StatusEventArgs : FriendBaseEventArgs
        {
            public ToxUserStatus Status { get; private set; }

            public StatusEventArgs(int friendNumber, ToxUserStatus status)
                : base(friendNumber)
            {
                Status = status;
            }
        }

        public class TypingStatusEventArgs : FriendBaseEventArgs
        {
            public bool IsTyping { get; private set; }

            public TypingStatusEventArgs(int friendNumber, bool isTyping)
                : base(friendNumber)
            {
                IsTyping = isTyping;
            }
        }

        public class FileControlEventArgs : FileBaseEventArgs
        {
            public ToxFileControl Control{get;private set;}

            public FileControlEventArgs(int friendNumber, int fileNumber, ToxFileControl control)
                : base(friendNumber, fileNumber)
            {
                Control = control;
            }
        }

        public class FileRequestChunkEventArgs : FileBaseEventArgs
        {
            public long Position { get; set; }

            public int Length { get; set; }

            public FileRequestChunkEventArgs(int friendNumber, int fileNumber, long position, int length)
                : base(friendNumber, fileNumber)
            {
                Position = position;
                Length = length;
            }
        }

        public class FriendPacketEventArgs : FriendBaseEventArgs
        {
            public byte[] Data{get;set;}

            public FriendPacketEventArgs(int friendNumber, byte[] data)
                : base(friendNumber)
            {
                Data = data;
            }
        }

        public class FileChunkEventArgs : FileBaseEventArgs
        {
            public byte[] Data { get; private set; }

            public long Position { get; private set; }

            public FileChunkEventArgs(int friendNumber, int fileNumber, byte[] data, long position)
                : base(friendNumber, fileNumber)
            {
                Data = data;
                Position = position;
            }
        }

        public class FileSendRequestEventArgs : FileBaseEventArgs
        {
            public long FileSize { get; private set; }

            public string FileName { get; private set; }

            public ToxFileKind FileKind { get; private set; }

            public FileSendRequestEventArgs(int friendNumber, int fileNumber, ToxFileKind kind, long fileSize, string fileName)
                : base(friendNumber, fileNumber)
            {
                FileSize = fileSize;
                FileName = fileName;
                FileKind = kind;
            }
        }

        public class ReadReceiptEventArgs : FriendBaseEventArgs
        {
            public int Receipt { get; private set; }

            public ReadReceiptEventArgs(int friendNumber, int receipt)
                : base(friendNumber)
            {
                Receipt = receipt;
            }
        }

        public class CustomPacketEventArgs : FriendBaseEventArgs
        {
            public byte[] Packet { get; private set; }

            public CustomPacketEventArgs(int friendNumber, byte[] packet)
                : base(friendNumber)
            {
                Packet = packet;
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

        public class GroupInviteEventArgs : FriendBaseEventArgs
        {
            public byte[] Data { get; private set; }

            public ToxGroupType GroupType { get; private set; }

            public GroupInviteEventArgs(int friendNumber, ToxGroupType type, byte[] data)
                : base(friendNumber)
            {
                if (data == null)
                    throw new ArgumentNullException("data");

                Data = (byte[])data.Clone();
                GroupType = type;
            }
        }

        public class GroupMessageEventArgs : GroupBaseEventArgs
        {
            public string Message { get; private set; }

            public GroupMessageEventArgs(int groupNumber, int peerNumber, string message)
                : base(groupNumber, peerNumber)
            {
                Message = message;
            }
        }

        public class GroupActionEventArgs : GroupBaseEventArgs
        {
            public string Action { get; private set; }

            public GroupActionEventArgs(int groupNumber, int peerNumber, string action)
                : base(groupNumber, peerNumber)
            {
                Action = action;
            }
        }

        public class GroupNamelistChangeEventArgs : GroupBaseEventArgs
        {
            public ToxChatChange Change { get; private set; }

            public GroupNamelistChangeEventArgs(int groupNumber, int peerNumber, ToxChatChange change)
                : base(groupNumber, peerNumber)
            {
                Change = change;
            }
        }

        public class GroupTitleEventArgs : GroupBaseEventArgs
        {
            public string Title { get; private set; }

            public GroupTitleEventArgs(int groupNumber, int peerNumber, string title)
                : base(groupNumber, peerNumber)
            {
                Title = title;
            }
        }
    }
}
