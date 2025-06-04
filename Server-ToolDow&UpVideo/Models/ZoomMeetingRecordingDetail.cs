using Newtonsoft.Json;

namespace Server_ToolDow_UpVideo.Models
{
    public class ZoomMeetingRecordingDetail
    {
        [JsonProperty("uuid")]
        public string? Uuid { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("account_id")]
        public string? AccountId { get; set; }

        [JsonProperty("host_id")]
        public string? HostId { get; set; }

        [JsonProperty("topic")]
        public string? Topic { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("recording_files")]
        public List<ZoomRecordingFile>? RecordingFiles { get; set; }
    }

    public class ZoomRecordingFile
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("file_type")]
        public string? FileType { get; set; }

        [JsonProperty("file_size")]
        public long FileSize { get; set; }

        [JsonProperty("download_url")]
        public string? DownloadUrl { get; set; }

        [JsonProperty("play_url")]
        public string? PlayUrl { get; set; }

        [JsonProperty("recording_start")]
        public DateTime? RecordingStart { get; set; }

        [JsonProperty("recording_end")]
        public DateTime? RecordingEnd { get; set; }
    }

}
