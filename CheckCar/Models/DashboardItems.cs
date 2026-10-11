namespace CheckCar.Models
{
    /// <summary>A summary metric shown on the dashboard (e.g. "12 vehículos").</summary>
    public class StatCard
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string Glyph { get; set; } = string.Empty;   // emoji / short label used as icon
        public Color Accent { get; set; } = Colors.SlateGray;
    }

    /// <summary>A large tappable shortcut on the dashboard (e.g. "Nuevo reporte").</summary>
    public class QuickAction
    {
        public string Title { get; set; } = string.Empty;
        public string Glyph { get; set; } = string.Empty;
        public Color Accent { get; set; } = Colors.SlateGray;
        public string Route { get; set; } = string.Empty;   // Shell route to navigate to
    }

    /// <summary>A recent-activity entry on the dashboard timeline.</summary>
    public class ActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string When { get; set; } = string.Empty;
        public string Glyph { get; set; } = string.Empty;
        public Color Accent { get; set; } = Colors.SlateGray;
    }
}
