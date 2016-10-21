﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WpfNative.Tryouts
{
    public static class MyService
    {
        // http://blogs.msdn.com/b/lucian/archive/2012/12/08/await-httpclient-getstringasync-and-cancellation.aspx
        public static async Task<int> DownloadAndCountBytesAsync(string url, CancellationToken token = default(CancellationToken))
        {
            await Task.Delay(TimeSpan.FromSeconds(3), token).ConfigureAwait(false);
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(url, token).ConfigureAwait(false))
            {
                var data = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return data.Length;
            }
        }
    }
}