using System;
using System.Collections.Generic;

namespace Server_ToolDow_UpVideo.Models;

public partial class Meeting
{
    public string Uuid { get; set; } = null!;

    public string? Topic { get; set; }

    public DateTime? StartTime { get; set; }

    public virtual ICollection<RecordingFile> RecordingFiles { get; set; } = new List<RecordingFile>();
}
