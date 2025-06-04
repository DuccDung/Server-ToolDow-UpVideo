using System;
using System.Collections.Generic;

namespace Server_ToolDow_UpVideo.Models;

public partial class ZoomRecording
{
    public int Id { get; set; }

    public string? MeetingUuid { get; set; }

    public string? FileId { get; set; }

    public string? FileName { get; set; }

    public string? DownloadUrl { get; set; }

    public DateTime? DownloadedAt { get; set; }
}
