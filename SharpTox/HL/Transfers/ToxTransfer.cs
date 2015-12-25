using System;
using System.IO;
using System.Threading;
using SharpTox.Core;

namespace SharpTox.HL.Transfers
{
    public class ToxTransfer
    {
        public ToxHL Tox { get; private set; }
        public ToxFileInfo Info { get; private set; }
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

        private long _transferredBytes;

        public long TransferredBytes
        {
            get { return _transferredBytes; }
            protected set
            {
                if (value == _transferredBytes)
                    return;

                // We fire the ProgressChanged event if the progress goes up by at least a full percent.
                var oldProgressPercentIntPart = (int)Math.Truncate(Progress * 100);
                _transferredBytes = value;
                var newProgressPercentIntPart = (int)Math.Truncate(Progress * 100);

                if (ProgressChanged != null && oldProgressPercentIntPart != newProgressPercentIntPart)
                {
                    ProgressChanged(this, new ToxTransferEventArgs.ProgressEventArgs(Progress));
                }
            }
        }

        public float Progress
        {
            get { return TransferredBytes / (float)Size; }
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
            Speed = (_transferredBytes - _lastTransferredBytes) * 2;
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
                return TimeSpan.FromSeconds((Size - _transferredBytes) / Speed);
            }
        }

        public event EventHandler<ToxTransferEventArgs.StateEventArgs> StateChanged;
        public event EventHandler<ToxTransferEventArgs.ErrorEventArgs> Errored;
        public event EventHandler<ToxTransferEventArgs.ProgressEventArgs> ProgressChanged;
        public event EventHandler<ToxTransferEventArgs.SpeedEventArgs> SpeedChanged;

        protected Stream _stream;

        internal ToxTransfer(ToxHL tox, ToxFriend friend, ToxFileInfo info, string name, long size, ToxFileKind kind)
        {
            State = ToxTransferState.Pending;

            Tox = tox;
            Friend = friend;
            Info = info;
            Name = name;
            Size = size;
            Kind = kind;

            Tox.Core.OnFileControlReceived += OnFileControlReceived;

            _speedUpdater = new Timer(SpeedUpdaterCallback, null,
                Timeout.Infinite, Timeout.Infinite);
        }
        internal ToxTransfer(ToxHL tox, Stream stream, ToxFriend friend, ToxFileInfo info, string name, ToxFileKind kind) : this(tox, friend, info, name, stream.Length, kind)
        {
            _stream = stream;
        }
        
        private void OnFileControlReceived(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            if (e.FileNumber != Info.Number || e.FriendNumber != Friend.Number)
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

        public void Pause()
        {
            SendControl(ToxFileControl.Pause);
            State = ToxTransferState.PausedByUser;
        }

        public void Resume()
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
                throw new ToxException<ToxErrorFileControl>(error);
        }
    }
}
