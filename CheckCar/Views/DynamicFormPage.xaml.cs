using System.Text.Json;
using CheckCar.Models.Api;
using CheckCar.Models.Local;
using CheckCar.Services;

namespace CheckCar.Views
{
    [QueryProperty(nameof(ReportType), "type")]
    public partial class DynamicFormPage : ContentPage
    {
        private readonly LocalDb _db;
        private readonly SyncService _sync;

        private FormDefinitionDto? _def;
        private List<LocalCar> _cars = new();
        private Picker? _carPicker;

        // Lectores de valor por clave de campo.
        private readonly Dictionary<string, Func<object?>> _getters = new();
        // Fotos capturadas por slot (rutas locales).
        private readonly Dictionary<string, List<string>> _photos = new();
        private readonly Dictionary<string, Label> _photoLabels = new();

        public string? ReportType { get; set; }

        public DynamicFormPage(LocalDb db, SyncService sync)
        {
            InitializeComponent();
            _db = db;
            _sync = sync;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_def != null) return; // ya construido
            await BuildAsync();
        }

        private async Task BuildAsync()
        {
            var type = ReportType ?? "Exit";
            Title = type == "Exit" ? "Nueva salida" : "Nueva entrada";
            HeaderLabel.Text = Title;

            var local = await _db.GetFormDefinitionAsync(type);
            if (local == null)
            {
                Busy.IsRunning = false;
                Root.Children.Add(new Label { Text = "No hay un formulario configurado/cacheado para este tipo. Sincroniza con conexión primero.", TextColor = Colors.OrangeRed });
                return;
            }

            _def = JsonSerializer.Deserialize<FormDefinitionDto>(local.Json);
            _cars = await _db.GetCarsAsync();
            Busy.IsRunning = false;
            Busy.IsVisible = false;

            // Selector de vehículo.
            Root.Children.Add(new Label { Text = "Vehículo", FontAttributes = FontAttributes.Bold });
            _carPicker = new Picker { Title = "Selecciona un vehículo" };
            foreach (var c in _cars) _carPicker.Items.Add(c.Display);
            Root.Children.Add(_carPicker);

            // Campos dinámicos.
            foreach (var f in (_def?.Fields ?? new()).OrderBy(x => x.Order))
                Root.Children.Add(BuildField(f));

            // Slots de foto.
            foreach (var s in (_def?.PhotoSlots ?? new()).OrderBy(x => x.Order))
                Root.Children.Add(BuildPhotoSlot(s));

            SaveButton.IsVisible = true;
        }

        private View BuildField(FormFieldDto f)
        {
            var stack = new VerticalStackLayout { Spacing = 4 };
            stack.Children.Add(new Label { Text = f.Label + (f.Required ? " *" : ""), FontAttributes = FontAttributes.Bold });

            switch (f.Type)
            {
                case "number":
                    var num = new Entry { Keyboard = Keyboard.Numeric };
                    _getters[f.Key] = () => double.TryParse(num.Text, out var d) ? d : (object?)num.Text;
                    stack.Children.Add(num);
                    break;
                case "textarea":
                    var ed = new Editor { HeightRequest = 90 };
                    _getters[f.Key] = () => ed.Text;
                    stack.Children.Add(ed);
                    break;
                case "date":
                    var dp = new DatePicker();
                    _getters[f.Key] = () => dp.Date.ToString("yyyy-MM-dd");
                    stack.Children.Add(dp);
                    break;
                case "select":
                    var pk = new Picker { Title = "Selecciona…" };
                    foreach (var o in f.Options ?? new()) pk.Items.Add(o);
                    _getters[f.Key] = () => pk.SelectedIndex >= 0 ? pk.Items[pk.SelectedIndex] : null;
                    stack.Children.Add(pk);
                    break;
                case "checkbox":
                    var sw = new Switch();
                    _getters[f.Key] = () => sw.IsToggled;
                    stack.Children.Add(sw);
                    break;
                case "range":
                    var val = new Label { Text = ((int)(f.Min ?? 0)).ToString() };
                    var sl = new Slider { Minimum = f.Min ?? 0, Maximum = f.Max ?? 100, Value = f.Min ?? 0 };
                    sl.ValueChanged += (_, e) => val.Text = ((int)e.NewValue).ToString();
                    _getters[f.Key] = () => (int)sl.Value;
                    stack.Children.Add(sl);
                    stack.Children.Add(val);
                    break;
                default: // text
                    var tx = new Entry();
                    _getters[f.Key] = () => tx.Text;
                    stack.Children.Add(tx);
                    break;
            }
            return stack;
        }

        private View BuildPhotoSlot(PhotoSlotDto s)
        {
            _photos[s.Key] = new List<string>();
            var stack = new VerticalStackLayout { Spacing = 4 };
            stack.Children.Add(new Label { Text = "📷 " + s.Label + (s.Required ? " *" : ""), FontAttributes = FontAttributes.Bold });

            var count = new Label { Text = "Sin fotos", FontSize = 12, TextColor = Colors.Gray };
            _photoLabels[s.Key] = count;

            var btn = new Button { Text = "Tomar / elegir foto" };
            btn.Clicked += async (_, _) => await CapturePhotoAsync(s);

            stack.Children.Add(btn);
            stack.Children.Add(count);
            return stack;
        }

        private async Task CapturePhotoAsync(PhotoSlotDto s)
        {
            try
            {
                FileResult? photo = MediaPicker.Default.IsCaptureSupported
                    ? await MediaPicker.Default.CapturePhotoAsync()
                    : await MediaPicker.Default.PickPhotoAsync();
                if (photo == null) return;

                var dest = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}");
                using (var src = await photo.OpenReadAsync())
                using (var dst = File.Create(dest))
                    await src.CopyToAsync(dst);

                if (!s.Multiple) _photos[s.Key].Clear();
                _photos[s.Key].Add(dest);
                _photoLabels[s.Key].Text = $"{_photos[s.Key].Count} foto(s)";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Foto", $"No se pudo capturar: {ex.Message}", "OK");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_def == null) return;

            // Construye el diccionario de datos.
            var data = new Dictionary<string, object?>();
            foreach (var kv in _getters) data[kv.Key] = kv.Value();

            // Validación de requeridos.
            var missing = _def.Fields.Where(f => f.Required && IsEmpty(data.GetValueOrDefault(f.Key))).Select(f => f.Label).ToList();
            var missingPhotos = _def.PhotoSlots.Where(s => s.Required && (_photos[s.Key].Count == 0)).Select(s => s.Label).ToList();
            if (missing.Count > 0 || missingPhotos.Count > 0)
            {
                await DisplayAlert("Faltan datos", "Obligatorios: " + string.Join(", ", missing.Concat(missingPhotos)), "OK");
                return;
            }

            var car = _carPicker?.SelectedIndex >= 0 ? _cars[_carPicker.SelectedIndex] : null;

            var report = new LocalReport
            {
                Type = ReportType ?? "Exit",
                CarId = car?.CarId,
                CarPlate = car?.Plate,
                DataJson = JsonSerializer.Serialize(data),
                CreatedUtc = DateTime.UtcNow,
                Status = "Pending"
            };
            var localId = await _db.InsertReportAsync(report);

            foreach (var (slotKey, files) in _photos)
                foreach (var f in files)
                    await _db.InsertPhotoAsync(new LocalPhoto { LocalReportId = localId, SlotKey = slotKey, FilePath = f });

            // Sincroniza en segundo plano (si hay red).
            _ = Task.Run(() => _sync.SyncAsync());

            await DisplayAlert("Guardado", _sync.IsOnline ? "Reporte guardado y sincronizando." : "Reporte guardado localmente (pendiente de sincronizar).", "OK");
            await Shell.Current.GoToAsync("..");
        }

        private static bool IsEmpty(object? v) =>
            v == null || (v is string s && string.IsNullOrWhiteSpace(s));
    }
}
