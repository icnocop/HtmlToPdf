// <copyright file="BrowserDownloader.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.IO;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using PuppeteerSharp;

    /// <summary>
    /// Browser Downloader
    /// </summary>
    internal static class BrowserDownloader
    {
        /// <summary>
        /// Downloads the browser
        /// </summary>
        internal static void DownloadBrowser()
        {
            string mutexId = $@"Global\{Directory.GetCurrentDirectory().Replace('\\', '_')}";

            var allowEveryoneRule =
               new MutexAccessRule(
                   new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                   MutexRights.FullControl,
                   AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            using (var mutex = new Mutex(false, mutexId, out bool createdNew, securitySettings))
            {
                var hasHandle = mutex.WaitOne(Timeout.Infinite, false);
                if (hasHandle == false)
                {
                    throw new TimeoutException("Timeout waiting for exclusive access");
                }

                try
                {
                    BrowserFetcher browserFetcher = new BrowserFetcher();
                    Task task = browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
                    task.Wait();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
