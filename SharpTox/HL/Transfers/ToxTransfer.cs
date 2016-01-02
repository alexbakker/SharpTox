using System;
using System.IO;
using System.Threading;
using SharpTox.Core;

namespace SharpTox.HL.Transfers
{
    public class ToxTransfer
    {
        public ToxHL Tox { get; private set; }
        public ToxFileInfo Info { get; protected set; }
        public ToxFriend Friend { get; private set; }
        public string Name { get; private set; }
        public long Size { get; private set; }
        public ToxFileKind Kind { get; private set; }

        private ToxTransferState _state;

        public ToxTransferState State
        {
            get { return _state; }
            protected set
            {
                if (value == _state)
                    return;

                switch (value)
                {
                    case ToxTransferState.PausedByUser:
                    case ToxTransferState.PausedByFriend:
                    case ToxTransferState.Broken:
                        Speed = 0;
                        _speedUpdater.Change(Timeout.Infinite, Timeout.Infinite);

                        _elapsedTimeOnLastStop = ElapsedTime;
                        break;
                    case ToxTransferState.InProgress:
                        _speedUpdater.Change(500, 500); // Update speed twice every second.

                        _lastResume = DateTime.Now;
                        break;
                    case ToxTransferState.Finished:
                    case ToxTransferState.Canceled:
                        Speed = 0;
                        _speedUpdater.Dispose();

                        _elapsedTimeOnLastStop = ElapsedTime;
                        break;
                }

                _state = value;

                if (StateChanged != null)
                    StateChanged(this, new ToxTransferEventArgs.StateEventArgs(value));
            }
        }

        protected bool IsActive
        {
            get { return State != ToxTransferState.Finished && State != ToxTransferState.Canceled; }
        }

        private long _transferredBytes;

        public long TransferredBytes
        {
            get { return _transferredBytes; }
            protected set
            {
                if (value == _transferredBytes)
                    return;

                // We fire the ProgressChanged event if the progress goes up by at least a full percent.
                var oldProgressPercentIntPart = (int) Math.Truncate(Progress*100);
                _transferredBytes = value;
                var newProgressPercentIntPart = (int) Math.Truncate(Progress*100);

                if (ProgressChanged != null && oldProgressPercentIntPart != newProgressPercentIntPart)
                {
                    ProgressChanged(this, new ToxTransferEventArgs.ProgressEventArgs(Progress));
                }
            }
        }

        public float Progress
        {
            get { return TransferredBytes/(float) Size; }
        }

        private float _speed;

        /// <summary>
        /// Measured in byte/sec.
        /// </summary>
        public float Speed
        {
            get { return _speed; }
            private set
            {
                if (value.Equals(_speed))
                    return;

                _speed = value;

                if (SpeedChanged != null)
                    SpeedChanged(this, new ToxTransferEventArgs.SpeedEventArgs(value));
            }
        }

        private long _lastTransferredBytes;
        private readonly Timer _speedUpdater;

        private void SpeedUpdaterCallback(object state)
        {
            // We know that the callback is called twice per second,
            // so the speed equals to the double of the amount of transferred bytes during this period:
            Speed = (_transferredBytes - _lastTransferredBytes)*2;
            _lastTransferredBytes = _transferredBytes;
        }

        private TimeSpan _elapsedTimeOnLastStop;
        private DateTime _lastResume;

        public TimeSpan ElapsedTime
        {
            get
            {
                if (State == ToxTransferState.InProgress)
                {
                    return _elapsedTimeOnLastStop + (DateTime.Now - _lastResume);
                }
                return _elapsedTimeOnLastStop;
            }
        }

        /// <summary>
        /// If it's null, then we couldn't estimate it due to 0 speed.
        /// </summary>
        public TimeSpan? RemainingTime
        {
            get
            {
                if (Speed.Equals(0))
                {
                    return null;
                }
                return TimeSpan.FromSeconds((Size - _transferredBytes)/Speed);
            }
        }

        public event EventHandler<ToxTransferEventArgs.StateEventArgs> StateChanged;
        public event EventHandler<ToxTransferEventArgs.ErrorEventArgs> Errored;
        public event EventHandler<ToxTransferEventArgs.ProgressEventArgs> ProgressChanged;
        public event EventHandler<ToxTransferEventArgs.SpeedEventArgs> SpeedChanged;

        protected Stream _stream;

        protected ToxTransfer(ToxHL tox, ToxFriend friend, ToxFileInfo info, string name, long size, ToxFileKind kind)
        {
            Tox = tox;
            Friend = friend;
            Info = info;
            Name = name;
            Size = size;
            Kind = kind;

            Tox.Core.OnFileControlReceived += OnFileControlReceived;

            Friend.ConnectionStatusChanged += OnFriendConnectionStatusChanged;
            Tox.ConnectionStatusChanged += OnUserConnectionStatusChanged;

            _speedUpdater = new Timer(SpeedUpdaterCallback, null,
                Timeout.Infinite, Timeout.Infinite);

            State = ToxTransferState.Pending;
        }

        protected ToxTransfer(ToxHL tox, Stream stream, ToxFriend friend, ToxFileInfo info, string name,
            ToxFileKind kind)
            : this(tox, friend, info, name, stream.Length, kind)
        {
            _stream = stream;
        }

        protected ToxTransfer(ToxHL tox, ToxFriend friend, ToxTransferResumeData resumeData, Stream stream)
            : this(tox, stream, friend, resumeData.Info, resumeData.Name, resumeData.Kind)
        {
            TransferredBytes = resumeData.TransferredBytes;
            State = ToxTransferState.Broken;
        }

        private void OnFileControlReceived(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            if (ShouldntHandle(e))
                return;

            switch (e.Control)
            {
                case ToxFileControl.Pause:
                    State = ToxTransferState.PausedByFriend;
                    break;
                case ToxFileControl.Resume:
                    State = ToxTransferState.InProgress;
                    break;
                case ToxFileControl.Cancel:
                    State = ToxTransferState.Canceled;
                    break;
                default:
                    OnError(new ToxTransferError(string.Format("Unknown file control received: {0}", e.Control)), false);
                    return;
            }
        }

        protected virtual void OnFriendConnectionStatusChanged(object sender,
            ToxFriendEventArgs.ConnectionStatusEventArgs connectionStatusEventArgs)
        {
            if (!Friend.IsOnline && IsActive)
            {
                State = ToxTransferState.Broken;
            }
        }

        private void OnUserConnectionStatusChanged(object sender,
            ToxEventArgs.ConnectionStatusEventArgs connectionStatusEventArgs)
        {
            if (!Tox.IsConnected && IsActive)
            {
                State = ToxTransferState.Broken;
            }
        }

        public void Pause()
        {
            SendControl(ToxFileControl.Pause);
            State = ToxTransferState.PausedByUser;
        }

        public virtual void Resume()
        {
            SendControl(ToxFileControl.Resume);
            State = ToxTransferState.InProgress;
        }

        public void Cancel()
        {
            SendControl(ToxFileControl.Cancel);
            State = ToxTransferState.Canceled;
        }

        protected void Finish()
        {
            State = ToxTransferState.Finished;
        }

        protected void OnError(ToxTransferError error, bool fatal)
        {
            if (Errored != null)
                Errored(this, new ToxTransferEventArgs.ErrorEventArgs(error));

            if (fatal)
                Cancel();
        }

        protected void SendControl(ToxFileControl control)
        {
            var error = ToxErrorFileControl.Ok;
            Tox.Core.FileControl(Friend.Number, Info.Number, control, out error);

            if (error != ToxErrorFileControl.Ok)
                OnError(new ToxTransferError(control + " control send failed! Error:" + error), false);
        }

        protected bool ShouldntHandle(ToxEventArgs.FileBaseEventArgs e)
        {
            return e.FriendNumber != Friend.Number || e.FileNumber != Info.Number || !IsActive;
        }
        
        public virtual ToxTransferResumeData GetResumeData()
        {
            return new ToxTransferResumeData
            {
                FriendNumber = Friend.Number,
                Info = Info,
                Name = Name,
                Kind = Kind,
                TransferredBytes = TransferredBytes
            };
        }
    }
}