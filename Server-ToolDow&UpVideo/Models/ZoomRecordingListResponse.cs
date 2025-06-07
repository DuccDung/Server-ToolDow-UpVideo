using Newtonsoft.Json;

namespace ToolDowloadVideoAndUpToYoutube.Models
{
    public class ZoomRecordingListResponse
    {
        [JsonProperty("from")]
        public string? From { get; set; }

        [JsonProperty("to")]
        public string? To { get; set; }

        [JsonProperty("total_records")]
        public int TotalRecords { get; set; }

        [JsonProperty("meetings")]
        public List<ZoomMeetingRecording>? Meetings { get; set; }
    }

    public class ZoomMeetingRecording
    {
        [JsonProperty("uuid")]
        public string? Uuid { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("topic")]
        public string? Topic { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }
    }

}
