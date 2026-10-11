using System.Text.Json.Serialization;

namespace CheckCar.Models.Api
{
    /// <summary>Respuesta del login de la API.</summary>
    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }

    /// <summary>Definición de formulario dinámico (desde /api/FormDefinitions).</summary>
    public class FormDefinitionDto
    {
        [JsonPropertyName("formDefinitionId")] public string? FormDefinitionId { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; } = "";
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("fields")] public List<FormFieldDto> Fields { get; set; } = new();
        [JsonPropertyName("photoSlots")] public List<PhotoSlotDto> PhotoSlots { get; set; } = new();
    }

    public class FormFieldDto
    {
        [JsonPropertyName("key")] public string Key { get; set; } = "";
        [JsonPropertyName("label")] public string Label { get; set; } = "";
        [JsonPropertyName("type")] public string Type { get; set; } = "text";
        [JsonPropertyName("required")] public bool Required { get; set; }
        [JsonPropertyName("order")] public int Order { get; set; }
        [JsonPropertyName("options")] public List<string>? Options { get; set; }
        [JsonPropertyName("min")] public double? Min { get; set; }
        [JsonPropertyName("max")] public double? Max { get; set; }
    }

    public class PhotoSlotDto
    {
        [JsonPropertyName("key")] public string Key { get; set; } = "";
        [JsonPropertyName("label")] public string Label { get; set; } = "";
        [JsonPropertyName("required")] public bool Required { get; set; }
        [JsonPropertyName("multiple")] public bool Multiple { get; set; }
        [JsonPropertyName("order")] public int Order { get; set; }
    }

    /// <summary>Vehículo (subconjunto usado por la app).</summary>
    public class CarDto
    {
        [JsonPropertyName("carId")] public string CarId { get; set; } = "";
        [JsonPropertyName("brand")] public string? Brand { get; set; }
        [JsonPropertyName("model")] public string? Model { get; set; }
        [JsonPropertyName("plate")] public string? Plate { get; set; }
    }
}
