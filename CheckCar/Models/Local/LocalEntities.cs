using SQLite;

namespace CheckCar.Models.Local
{
    /// <summary>Definición de formulario cacheada localmente (JSON crudo del DTO).</summary>
    public class LocalFormDefinition
    {
        [PrimaryKey] public string Type { get; set; } = "";   // "Exit" | "Entry"
        public string Json { get; set; } = "{}";
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>Vehículo cacheado para el selector offline.</summary>
    public class LocalCar
    {
        [PrimaryKey] public string CarId { get; set; } = "";
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Plate { get; set; }
        public string Display => $"{Brand} {Model} · {Plate}".Trim();
    }

    /// <summary>Reporte capturado localmente (pendiente de sincronizar).</summary>
    public class LocalReport
    {
        [PrimaryKey, AutoIncrement] public int LocalId { get; set; }
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = "";
        public string? CarId { get; set; }
        public string? CarPlate { get; set; }
        public string DataJson { get; set; } = "{}";
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";   // Pending | Synced | Error
        public string? Error { get; set; }
    }

    /// <summary>Foto local asociada a un reporte, etiquetada por slot.</summary>
    public class LocalPhoto
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        [Indexed] public int LocalReportId { get; set; }
        public string SlotKey { get; set; } = "";
        public string FilePath { get; set; } = "";
        public bool Synced { get; set; }
    }
}
