namespace Matrix.DTOs
{
    public class ReportListItemDto
    {
        public Guid ReportId { get; set; }
        public string Reason { get; set; } = "";
        public string Reporter { get; set; } = "";
        public string Type { get; set; } = "";      // "User" / "Article"
        public string Target { get; set; } = "";    // 顯示對象名稱/標題
        public DateTime? CreateTime { get; set; }   // 這裡回 null -> 前端顯示 "-"
        public DateTime? ModifyTime { get; set; }   // 用 Report.ProcessTime 回填
        public string Status { get; set; } = "";    // "Pending" / "Processed" / "Rejected"
    }
}
