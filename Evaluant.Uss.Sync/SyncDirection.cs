using System;
using System.Text;

namespace Evaluant.Uss.Sync
{
    public enum SyncDirection
    {
        FullDownload,
		FullDownloadNoBulk,
        SmartDownload,
        SmartUpload,
        SmartUploadDownload,
        FullUploadDownload
    }
}
