using System;
using System.IO;
using System.Text;

namespace SharpTox.Core
{
    public class ToxDataInfo
    {
        /// <summary>
        /// The Tox ID of this data file.
        /// </summary>
        public ToxId Id { get; private set; }

        /// <summary>
        /// The name used in this data file.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The status message in this data file.
        /// </summary>
        public string StatusMessage { get; private set; }

        /// <summary>
        /// The status used in this data file.
        /// </summary>
        public ToxUserStatus Status { get; private set; }

        /// <summary>
        /// The secret key of this data file.
        /// </summary>
        public ToxKey SecretKey { get; private set; }

        private ToxDataInfo() { }

        internal static ToxDataInfo FromToxData(ToxData data)
        {
            try
            {
                ToxId id = null;
                string name = null;
                string statusMessage = null;
                ToxUserStatus status = ToxUserStatus.None;
                byte[] secretKey = null;

                using (var stream = new MemoryStream(data.Bytes))
                using (var reader = new BinaryReader(stream))
                {
                    stream.Position += sizeof(uint);

                    uint cookie = reader.ReadUInt32();
                    if (cookie != ToxConstants.Cookie)
                        throw new Exception("Invalid cookie, this doesn't look like a tox profile");

                    uint length = reader.ReadUInt32();
                    long left = reader.BaseStream.Length - reader.BaseStream.Position;

                    while (left >= length)
                    {
                        var type = ReadStateType(reader);

                        if (type == StateType.EOF)
                            break;

                        switch (type)
                        {
                            case StateType.NospamKeys:
                                int nospam = reader.ReadInt32();
                                byte[] publicKey = reader.ReadBytes(ToxConstants.PublicKeySize);

                                secretKey = reader.ReadBytes(ToxConstants.SecretKeySize);
                                id = new ToxId(publicKey, nospam);
                                break;
                            case StateType.Name:
                                name = Encoding.UTF8.GetString(reader.ReadBytes((int)length), 0, (int)length);
                                break;
                            case StateType.StatusMessage:
                                statusMessage = Encoding.UTF8.GetString(reader.ReadBytes((int)length), 0, (int)length);
                                break;
                            case StateType.Status:
                                status = (ToxUserStatus)reader.ReadByte();
                                break;
                            default:
                            case StateType.Dht:
                            case StateType.Friends:
                            case StateType.TcpRelay:
                            case StateType.PathNode:
                                stream.Position += length; //skip this
                                break;
                            case StateType.Corrupt:
                                throw new Exception("This Tox save file is corrupt");
                        }

                        left = reader.BaseStream.Length - reader.BaseStream.Position;
                        if (left < sizeof(uint))
                            break;
                        else
                            length = reader.ReadUInt32();
                    }
                }

                return new ToxDataInfo()
                {
                    Id = id,
                    Name = name,
                    StatusMessage = statusMessage,
                    Status = status,
                    SecretKey = new ToxKey(ToxKeyType.Secret, secretKey)
                };
            }
            catch { return null; }
        }

        private static StateType ReadStateType(BinaryReader reader)
        {
            uint type = reader.ReadUInt32();
            if (type >> 16 != ToxConstants.CookieInner)
                return StateType.Corrupt;

            return (StateType)type;
        }

        private enum StateType : ushort
        {
            NospamKeys = 1,
            Dht = 2,
            Friends = 3,
            Name = 4,
            StatusMessage = 5,
            Status = 6,
            TcpRelay = 10,
            PathNode = 11,
            Corrupt = 50,
            EOF = 255,
        }
    }
}
