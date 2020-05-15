// <copyright file="BrowserDownloader.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.IO;
    using System.Linq;
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
        /// <param name="logger">The logger.</param>
        /// <exception cref="TimeoutException">Timeout waiting for exclusive access</exception>
        internal static void DownloadBrowser(Logger logger)
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
                    if (browserFetcher.LocalRevisions().Any())
                    {
                        // local revision already exists
                        return;
                    }

                    logger.LogDebug("Downloading browser...");
                    Task<RevisionInfo> task = browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
                    task.Wait();

                    RevisionInfo revisionInfo = task.Result;
                    if (!revisionInfo.Downloaded)
                    {
                        throw new Exception($"Browser could not be downlaoded from URL \"{revisionInfo.Url}\".");
                    }

                    logger.LogDebug($"Browser downloaded to \"{revisionInfo.FolderPath}\".");
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
