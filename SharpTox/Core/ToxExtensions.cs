using System;

namespace SharpTox.Core
{
    public static class ToxExtensions
    {
        public static bool SendData(this ToxFileSender sender, byte[] data)
        {
            return sender.SendData(new ArraySegment<byte>(data, 0, data.Length));
        }
    }
}

