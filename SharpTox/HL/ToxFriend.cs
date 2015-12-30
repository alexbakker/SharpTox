using System;
using System.IO;
using SharpTox.Core;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using SharpTox.HL.Transfers;

namespace SharpTox.HL
{
    public class ToxFriend
    {
        public int Number { get; private set; }
        public ToxHL Tox { get; private set; }

        public event EventHandler<ToxTransferEventArgs.RequestEventArgs> TransferRequestReceived;
        public event EventHandler<ToxFriendEventArgs.ConnectionStatusEventArgs> ConnectionStatusChanged;
        public event EventHandler<ToxFriendEventArgs.MessageEventArgs> MessageReceived;
        public event EventHandler<ToxFriendEventArgs.NameChangeEventArgs> NameChanged;
        public event EventHandler<ToxFriendEventArgs.StatusEventArgs> StatusChanged;
        public event EventHandler<ToxFriendEventArgs.StatusMessageEventArgs> StatusMessageChanged;
        public event EventHandler<ToxFriendEventArgs.TypingStatusEventArgs> TypingStatusChanged;
        public event EventHandler<ToxFriendEventArgs.CustomPacketEventArgs> CustomLossyPacketReceived;
        public event EventHandler<ToxFriendEventArgs.CustomPacketEventArgs> CustomLosslessPacketReceived;
        public event EventHandler<ToxFriendEventArgs.ReadReceiptEventArgs> ReadReceiptReceived;

        public IReadOnlyList<ToxTransfer> Transfers
        {
            get { return _transfers; }
        }

        public string Name
        {
            get { return QueryFriend(Tox.Core.GetFriendName, Number); }
        }

        public string StatusMessage
        {
            get { return QueryFriend(Tox.Core.GetFriendStatusMessage, Number); }
        }

        public ToxUserStatus Status
        {
            get { return QueryFriend(Tox.Core.GetFriendStatus, Number); }
        }

        public ToxConnectionStatus ConnectionStatus
        {
            get { return QueryFriend(Tox.Core.GetFriendConnectionStatus, Number); }
        }

        public bool IsOnline
        {
            get { return ConnectionStatus != ToxConnectionStatus.None; }
        }

        public bool IsTyping
        {
            get { return QueryFriend(Tox.Core.GetFriendTypingStatus, Number); }
        }

        public ToxKey PublicKey
        {
            get
            {
                var error = ToxErrorFriendGetPublicKey.Ok;
                var result = Tox.Core.GetFriendPublicKey(Number, out error);

                if (error != ToxErrorFriendGetPublicKey.Ok)
                    throw new ToxException<ToxErrorFriendGetPublicKey>(error);

                return result;
            }
        }

        public DateTime LastOnline
        {
            get
            {
                var error = ToxErrorFriendGetLastOnline.Ok;
                var result = Tox.Core.GetFriendLastOnline(Number, out error);

                if (error != ToxErrorFriendGetLastOnline.Ok)
                    throw new ToxException<ToxErrorFriendGetLastOnline>(error);

                return result;
            }
        }

        private readonly ToxTransferCollection _transfers;

        internal ToxFriend(ToxHL tox, int friendNumber)
        {
            Tox = tox;
            Number = friendNumber;
            
            _transfers = new ToxTransferCollection(Tox, this);

            HookEvents();
        }

        public int SendMessage(string message, ToxMessageType type)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            var error = ToxErrorSendMessage.Ok;
            int messageNumber = Tox.Core.SendMessage(Number, message, type, out error);

            if (error != ToxErrorSendMessage.Ok)
                throw new ToxException<ToxErrorSendMessage>(error);

            return messageNumber;
        }

        public ToxOutgoingTransfer SendFile(Stream stream, string fileName, ToxFileKind kind)
        {
            return _transfers.SendFile(stream, fileName, kind);
        }

        public void ResumeBrokenTransfers(IList<ToxTransferResumeData> resumeDatas)
        {
            foreach (var resumeData in resumeDatas)
            {
                ResumeBrokenTransfer(resumeData);
            }
        }

        public void ResumeBrokenTransfer(ToxTransferResumeData resumeData)
        {
            _transfers.ResumeBrokenTransfer(resumeData);
        }

        public void SendLossyPacket(byte[] packet)
        {
            var error = ToxErrorFriendCustomPacket.Ok;
            Tox.Core.FriendSendLossyPacket(Number, packet, out error);

            if (error != ToxErrorFriendCustomPacket.Ok)
                throw new ToxException<ToxErrorFriendCustomPacket>(error);
        }

        public void SendLosslessPacket(byte[] packet)
        {
            var error = ToxErrorFriendCustomPacket.Ok;
            Tox.Core.FriendSendLosslessPacket(Number, packet, out error);

            if (error != ToxErrorFriendCustomPacket.Ok)
                throw new ToxException<ToxErrorFriendCustomPacket>(error);
        }
        
        private void HookEvents()
        {
            _transfers.TransferRequestReceived += (sender, args) =>
            {
                if (TransferRequestReceived != null)
                    TransferRequestReceived(this, args);
            };

            Tox.Core.OnFriendConnectionStatusChanged += (sender, e) =>
                RaiseFriendEvent(ConnectionStatusChanged, e, () =>
                    new ToxFriendEventArgs.ConnectionStatusEventArgs(e.Status));

            Tox.Core.OnFriendLossyPacketReceived += (sender, e) =>
                RaiseFriendEvent(CustomLossyPacketReceived, e, () =>
                    new ToxFriendEventArgs.CustomPacketEventArgs(e.Data));

            Tox.Core.OnFriendLosslessPacketReceived += (sender, e) =>
                RaiseFriendEvent(CustomLosslessPacketReceived, e, () =>
                    new ToxFriendEventArgs.CustomPacketEventArgs(e.Data));

            Tox.Core.OnFriendMessageReceived += (sender, e) =>
                RaiseFriendEvent(MessageReceived, e, () =>
                    new ToxFriendEventArgs.MessageEventArgs(e.Message, e.MessageType));

            Tox.Core.OnFriendNameChanged += (sender, e) =>
                RaiseFriendEvent(NameChanged, e, () =>
                    new ToxFriendEventArgs.NameChangeEventArgs(e.Name));

            Tox.Core.OnFriendStatusChanged += (sender, e) =>
                RaiseFriendEvent(StatusChanged, e, () =>
                    new ToxFriendEventArgs.StatusEventArgs(e.Status));

            Tox.Core.OnFriendStatusMessageChanged += (sender, e) =>
                RaiseFriendEvent(StatusMessageChanged, e, () =>
                    new ToxFriendEventArgs.StatusMessageEventArgs(e.StatusMessage));

            Tox.Core.OnFriendTypingChanged += (sender, e) =>
                RaiseFriendEvent(TypingStatusChanged, e, () =>
                    new ToxFriendEventArgs.TypingStatusEventArgs(e.IsTyping));

            Tox.Core.OnReadReceiptReceived += (sender, e) =>
                RaiseFriendEvent(ReadReceiptReceived, e, () =>
                    new ToxFriendEventArgs.ReadReceiptEventArgs(e.Receipt));
        }

        private delegate T QueryFriendDelegate<T>(int friendNumber, out ToxErrorFriendQuery error);

        private T QueryFriend<T>(QueryFriendDelegate<T> func, int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            var result = func(friendNumber, out error);

            if (error != ToxErrorFriendQuery.Ok)
                throw new ToxException<ToxErrorFriendQuery>(error);

            return result;
        }

        private void RaiseFriendEvent<T, T2>(EventHandler<T2> handler, T oldArgs, Func<T2> getNewArgs)
            where T : ToxEventArgs.FriendBaseEventArgs
            where T2 : EventArgs
        {
            if (oldArgs.FriendNumber != Number)
                return;

            if (handler != null)
                handler(this, getNewArgs());
        }
    }
}