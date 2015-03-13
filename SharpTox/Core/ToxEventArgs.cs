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

            public FriendMessageEventArgs(int friendNumber, string message)
                : base(friendNumber)
            {
                Message = message;
            }
        }

        public class FriendActionEventArgs : FriendBaseEventArgs
        {
            public string Action { get; private set; }

            public FriendActionEventArgs(int friendNumber, string action)
                : base(friendNumber)
            {
                Action = action;
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
            public ToxStatus Status { get; private set; }

            public StatusEventArgs(int friendNumber, ToxStatus status)
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
            public ulong Position { get; set; }

            public int Length { get; set; }

            public FileRequestChunkEventArgs(int friendNumber, int fileNumber, ulong position, int length)
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

            public ulong Position { get; private set; }

            public FileChunkEventArgs(int friendNumber, int fileNumber, byte[] data, ulong position)
                : base(friendNumber, fileNumber)
            {
                Data = data;
                Position = position;
            }
        }

        public class FileSendRequestEventArgs : FileBaseEventArgs
        {
            public ulong FileSize { get; private set; }

            public string FileName { get; private set; }

            public FileSendRequestEventArgs(int friendNumber, int fileNumber, ulong fileSize, string fileName)
                : base(friendNumber, fileNumber)
            {
                FileSize = fileSize;
                FileName = fileName;
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
