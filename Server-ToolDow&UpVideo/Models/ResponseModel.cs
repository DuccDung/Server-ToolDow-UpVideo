namespace Server_ToolDow_UpVideo.Models
{
    public class ResponseModel<T>
    {
        public bool IsSussess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<T> DataList { get; set; } = new List<T>();
    }
}
