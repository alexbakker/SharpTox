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

                _state = value;
                
                switch (value)
                {
                    case ToxTransferState.Pending:
                        break;
                    case ToxTransferState.PausedByUser:
                        Speed = 0;
                        _elapsedTimeCounter.Change(Timeout.Infinite, Timeout.Infinite);
                        break;
                    case ToxTransferState.PausedByFriend:
                        Speed = 0;
                        _elapsedTimeCounter.Change(Timeout.Infinite, Timeout.Infinite);
                        break;
                    case ToxTransferState.InProgress:
                        _speedLastMeasured = DateTime.Now;
                        _elapsedTimeCounter.Change(1000, 1000);
                        break;
                    case ToxTransferState.Finished:
                        Speed = 0;
                        _elapsedTimeCounter.Dispose();
                        break;
                    case ToxTransferState.Canceled:
                        Speed = 0;
                        _elapsedTimeCounter.Dispose();
                        break;
                }

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

                // We fire the ProgressChanged event and update speed only if the progress goes up by at least a full percent.
                var oldProgressPercentIntPart = (int) Math.Truncate(Progress*100);
                _transferredBytes = value;
                var newProgressPercentIntPart = (int) Math.Truncate(Progress*100);

                if (oldProgressPercentIntPart != newProgressPercentIntPart)
                {
                    if (ProgressChanged != null)
                        ProgressChanged(this, new ToxTransferEventArgs.ProgressEventArgs(Progress));

                    UpdateSpeed();
                }
            }
        }

        private long _lastTransferredBytes;
        private DateTime _speedLastMeasured;

        private void UpdateSpeed()
        {
            var byteDiff = _transferredBytes - _lastTransferredBytes;
        
            var now = DateTime.Now;
            var timeDiff = (now - _speedLastMeasured).Milliseconds/1000.0f;
           
            if (timeDiff.Equals(0))
                return;
            
            Speed = byteDiff/timeDiff;
            
            _lastTransferredBytes = _transferredBytes;
            _speedLastMeasured = now;
        }

        private float _speed;

        // byte/sec
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

        public float Progress
        {
            get { return TransferredBytes/(float) Size; }
        }

        private readonly Timer _elapsedTimeCounter;
        private DateTime _elapsedTime;

        // Seconds
        public DateTime ElapsedTime
        {
            get { return _elapsedTime; }
            private set
            {
                if (Equals(value, _elapsedTime))
                    return;

                _elapsedTime = value;

                if (ElapsedTimeChanged != null)
                    ElapsedTimeChanged(this, new ToxTransferEventArgs.TimeEventArgs(value));
            }
        }

        public event EventHandler<ToxTransferEventArgs.StateEventArgs> StateChanged;
        public event EventHandler<ToxTransferEventArgs.ErrorEventArgs> Errored;
        public event EventHandler<ToxTransferEventArgs.ProgressEventArgs> ProgressChanged;
        public event EventHandler<ToxTransferEventArgs.SpeedEventArgs> SpeedChanged;
        public event EventHandler<ToxTransferEventArgs.TimeEventArgs> ElapsedTimeChanged;

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

            _elapsedTimeCounter = new Timer(state => { ElapsedTime = ElapsedTime.AddSeconds(1); }, null,
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
