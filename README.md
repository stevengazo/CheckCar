<div align="center">

# 📱 CheckCar — App móvil

**Captura de reportes de flota en campo, offline-first**

![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-9-512BD4?logo=dotnet&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-local-003B57?logo=sqlite&logoColor=white)
![Platforms](https://img.shields.io/badge/Android%20%7C%20iOS%20%7C%20Windows-lightgrey)

</div>

---

## 📋 Descripción

CheckCar es la app móvil del ecosistema **CheckCars**, construida con **.NET MAUI**. Permite a los operadores capturar reportes de **salida** y **entrada** de vehículos directamente en campo, **funcionando sin conexión** y sincronizándose con la [CheckCars API](https://github.com/stevengazo/CheckCarsAPI) cuando hay red.

## ✨ Características

- 📴 **Offline-first**: cada reporte se guarda primero en **SQLite** local; nunca se pierde por falta de red.
- 🔄 **Sincronización** automática y manual: baja las definiciones de formularios y los vehículos, y sube los reportes pendientes.
- 🧩 **Formularios dinámicos**: los campos y las fotos requeridas se definen en el backend y la app los renderiza en tiempo de ejecución.
- 📷 **Captura de fotos** por categoría/slot (cámara o galería).
- 🔐 **Login JWT** con credenciales almacenadas de forma segura (`SecureStorage`).
- 📊 Dashboard con accesos rápidos y estado de sincronización.

## 🏗️ Arquitectura

```
CheckCar/
├── Models/
│   ├── Api/      # DTOs de la API (FormDefinition, Car…)
│   └── Local/    # Entidades SQLite (LocalReport, LocalPhoto, LocalCar…)
├── Services/
│   ├── ApiConfig.cs   # URL base de la API
│   ├── LocalDb.cs     # Persistencia SQLite (sqlite-net-pcl)
│   ├── ApiService.cs  # Cliente HTTP + JWT
│   └── SyncService.cs # Motor de sincronización offline-first
├── ViewModels/   # MVVM (CommunityToolkit.Mvvm)
└── Views/        # Login, Dashboard, Reports, DynamicForm…
```

**Flujo offline:** capturar → SQLite (`Pending`) + fotos locales → al haber red, `SyncService` sube a `/api/DynamicReports/form` y marca `Synced`.

## 🛠️ Tecnologías

**.NET 9 MAUI** · **CommunityToolkit.Mvvm** · **sqlite-net-pcl** · **MAUI Essentials** (Connectivity, MediaPicker, SecureStorage)

## 🚀 Puesta en marcha

### Requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) + workloads de MAUI (`dotnet workload install maui`)
- Visual Studio 2022 (recomendado) con el emulador/dispositivo objetivo
- La [CheckCars API](https://github.com/stevengazo/CheckCarsAPI) accesible

### Configuración de la API
Edita `Services/ApiConfig.cs`:
```csharp
public static string BaseUrl { get; set; } = "http://192.168.0.14:8085";
// Emulador Android: http://10.0.2.2:8085 · Windows local: http://localhost:8085
// Dispositivo físico: IP LAN de tu PC
```

### Compilar / ejecutar
```bash
dotnet build -f net9.0-android     # o net9.0-windows10.0.19041.0
```
O abre la solución en Visual Studio y ejecuta en el emulador/dispositivo.

## 🔗 Ecosistema CheckCars

- 📱 **App móvil** — este repositorio
- ⚙️ [CheckCars API](https://github.com/stevengazo/CheckCarsAPI) — backend .NET 9
- 🌐 [CheckCars Dashboard](https://github.com/stevengazo/checkcars-web) — frontend React

## 📄 Licencia

Proyecto de **Steven Gazo** · [stevengazo.co.cr](https://stevengazo.co.cr)
