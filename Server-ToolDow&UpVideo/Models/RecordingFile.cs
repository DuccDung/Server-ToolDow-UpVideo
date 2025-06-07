using System;
using System.Collections.Generic;

namespace Server_ToolDow_UpVideo.Models;

public partial class RecordingFile
{
    public string FileId { get; set; } = null!;

    public string? MeetingUuid { get; set; }

    public string? FileName { get; set; }

    public string? FileType { get; set; }

    public string? DownloadUrl { get; set; }

    public DateTime? DownloadedAt { get; set; }
    public bool Condition { get; set; }

    public virtual Meeting? MeetingUu { get; set; }
}
