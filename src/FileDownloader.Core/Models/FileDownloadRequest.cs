using System;

namespace FileDownloader.Core.Models
{
    public class FileDownloadRequest
    {
        public FileDownloadRequest(string resourceLink)
        {
            this.ResourceLink = new Uri(resourceLink);
        }

        public FileDownloadRequest(string resourceLink, NetworkCredentials networkCredentials)
        {
            this.ResourceLink = new Uri(resourceLink);
            this.NetworkCredentials = networkCredentials;
        }

        public FileDownloadRequest(Uri resourceLink, NetworkCredentials networkCredentials)
        {
            this.ResourceLink = resourceLink;
            this.NetworkCredentials = networkCredentials;
        }

        public FileDownloadRequest(Uri resourceLink)
        {
            this.ResourceLink = resourceLink;
        }

        public Uri ResourceLink { get; private set; }
        public NetworkCredentials NetworkCredentials { get; private set; }
    }
}