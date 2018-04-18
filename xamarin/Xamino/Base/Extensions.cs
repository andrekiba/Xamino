using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using System;
using System.Threading;

namespace Xamino.Base
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        public static bool IsIpAddress(this string value) => IPAddress.TryParse(value, out var ipAddress);

        public static bool IsPort(this string value)
        {
            if (int.TryParse(value, out var port))
                return 1 <= port && port <= 65535;
            return false;
        }
    }

    public static class StreamExtensions
    {
        public static Task<byte[]> ReadFullyAsync(this NetworkStream stream, CancellationToken token = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<byte[]>();
            var bytes = new List<byte>();
            token.Register(() => tcs.TrySetCanceled());

            try
            {
                while(stream.DataAvailable)
                {
                    bytes.Add((byte)stream.ReadByte());
                }
                tcs.SetResult(bytes.ToArray());
            }
            catch(Exception ex)
            {
                tcs.SetException(ex);  
            }

            return tcs.Task;
        }
    }
}
