using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTox.HL
{
    public static class ToxFriendEventArgs
    {
        public class ConnectionStatusEventArgs : EventArgs
        {
            public ToxConnectionStatus Status { get; private set; }

            public ConnectionStatusEventArgs(ToxConnectionStatus status)
            {
                Status = status;
            }
        }

        public class MessageEventArgs : EventArgs
        {
            public string Message { get; private set; }

            public ToxMessageType MessageType { get; private set; }

            public MessageEventArgs(string message, ToxMessageType type)
            {
                Message = message;
                MessageType = type;
            }
        }

        public class NameChangeEventArgs : EventArgs
        {
            public string Name { get; private set; }

            public NameChangeEventArgs(string name)
            {
                Name = name;
            }
        }

        public class StatusMessageEventArgs : EventArgs
        {
            public string StatusMessage { get; private set; }

            public StatusMessageEventArgs(string statusMessage)
            {
                StatusMessage = statusMessage;
            }
        }

        public class StatusEventArgs : EventArgs
        {
            public ToxUserStatus Status { get; private set; }

            public StatusEventArgs(ToxUserStatus status)
            {
                Status = status;
            }
        }

        public class TypingStatusEventArgs : EventArgs
        {
            public bool IsTyping { get; private set; }

            public TypingStatusEventArgs(bool isTyping)
            {
                IsTyping = isTyping;
            }
        }

        public class CustomPacketEventArgs : EventArgs
        {
            public byte[] Packet { get; private set; }

            public CustomPacketEventArgs(byte[] packet)
            {
                Packet = packet;
            }
        }

        public class ReadReceiptEventArgs : EventArgs
        {
            public int Receipt { get; private set; }

            public ReadReceiptEventArgs(int receipt)
            {
                Receipt = receipt;
            }
        }
    }
}
