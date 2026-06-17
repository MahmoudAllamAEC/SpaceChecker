# 🏢 Space Checker — Revit Add-in

<div align="center">

[![Revit API](https://img.shields.io/badge/Revit%20API-2022–2024-blue?style=for-the-badge&logo=autodesk)](https://www.autodesk.com/developer-network/platform-technologies/revit)
[![Language](https://img.shields.io/badge/C%23-.NET-purple?style=for-the-badge&logo=csharp)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/Version-1.0-green?style=for-the-badge)]()
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)]()

**A Revit API plugin that checks your building's actual spaces against a defined space program — automating one of the most error-prone compliance tasks in residential and commercial AEC projects.**

[▶️ Watch Demo Video](https://drive.google.com/file/d/18mYa4OjMhKjQa8n76I_xXBR8YzkYmWo8/view?usp=sharing) • [📥 Download Installer](https://github.com/MahmoudAllamAEC/SpaceChecker/releases/download/BimDev/Space.Checker.exe) 

</div>

---

## 🎬 Demo

> Click the thumbnail below to watch the full walkthrough

[![Space Checker Demo]https://drive.google.com/file/d/18mYa4OjMhKjQa8n76I_xXBR8YzkYmWo8/view?usp=sharing)

<!-- Replace the line below with an actual screenshot of your plugin UI -->
<!-- ![Space Checker UI](docs/screenshots/ui-preview.png) -->

---

## 🧩 What Problem Does It Solve?

In traditional AEC workflows, verifying that a building's rooms comply with a space program requires:
- Manually reading areas floor by floor in Revit
- Cross-referencing room names and sizes against an Excel spec by hand
- Tracking deviations, duplex units across levels, and repeated apartment types manually
- No clear pass/fail visibility without building custom schedules

**Space Checker eliminates all of that.** Define your space program once in Excel, import it, and get a fully color-coded compliance report — per apartment, per room, with deviation percentages — directly inside Revit.

---

## ✨ Features

| Feature | Description |
|---|---|
| 🏠 **Spatial Room Mapping** | Rooms are matched to apartments via point-in-polygon — no manual tagging required |
| 🔁 **Duplex Support** | Apartments spanning multiple levels are merged automatically by name |
| 📋 **Type-Based Program** | Define each apartment type once; every instance is checked against it |
| 📥 **Import Program** | Load your filled-in Excel space program in one click |
| 📤 **Create Template** | Generates a ready-to-fill Excel template from your model's layout |
| 🔄 **Export Data** | Dumps actual areas + rooms to Excel in the same import-compatible layout |
| 🟢 **Color-Coded Results** | Green = pass, Amber = pass but rooms deviate (⚠), Red = total area fails |
| 📐 **Adjustable Tolerance** | Set an acceptable deviation percentage before running the comparison |
| 🪟 **Modeless Window** | Keep working in Revit while Space Checker is open; refresh data on demand |

---

## 🖥️ Screenshots

<div align="center">

<!-- Add your actual screenshots below -->
<!-- Drag screenshots into the repo under docs/screenshots/ then update these paths -->

| Plugin UI | Compliance Results | Excel Sheet Detail |
|---|---|---|
| ![UI](docs/screenshots/Loaded UI.png) | ![Results](docs/screenshots/ComparIson Green UI.png) | ![Detail](docs/screenshots/agreed on sheet.png) |

</div>

---

## 🔧 Tech Stack

- **Language:** C# / .NET Framework 4.8.1
- **API:** Autodesk Revit API
- **UI:** WPF (MVVM pattern)
- **Export / Import:** EPPlus 5.8
- **Packaging:** Custom installer (multi-version support)

---

## 📦 Installation

### Option 1 — Installer (Recommended)

1. Download the latest release from the [Releases page](#)
2. Run the setup file — it automatically installs for all detected Revit versions
3. Launch Revit and go to the **AACD Architect** tab → **General** panel
4. Click **Space Checker** to launch

### Option 2 — Manual

1. Clone this repository
2. Build the solution in Visual Studio
3. Copy the `.addin` manifest and `.dll` to:
   ```
   %APPDATA%\Autodesk\Revit\Addins\{RevitVersion}\
   ```

---

## ✅ Compatibility

| Revit Version | Status |
|---|---|
| Revit 2022 | ✅ Supported |
| Revit 2023 | ✅ Supported |
| Revit 2024 | ✅ Supported |

---

## 🚀 How to Use

1. Open any Revit project with **Area boundaries** (one per apartment/space) and **rooms** placed inside them
2. Go to **AACD Architect tab → General → Space Checker**
3. The add-in extracts all areas and rooms automatically on open
4. Click **Create Template** to generate an Excel space program template, fill in the required areas per type, then click **Import Program** — or import an existing program directly
5. Set the **acceptable deviation (%)** in the header
6. Click **Run Comparison** to see each apartment's result, color-coded, with a per-room breakdown in the detail table
7. *(Optional)* Edit the model, click **Extract Areas** to refresh, then re-run the comparison
8. *(Optional)* Click **Export Data** to write the model's actual areas + rooms to Excel

---

## 🎨 Understanding the Results

| Color | Meaning |
|---|---|
| 🟢 Green | Area total is within the acceptable deviation — **Pass** |
| 🟡 Amber | Area total passes, but one or more rooms deviate — **Pass with Warning ⚠** |
| 🔴 Red | Area total is outside the acceptable deviation — **Fail** |

> **Note:** Pass/fail is decided by each area's **own total area** vs. the required total. Rooms that deviate inside a passing area are surfaced as a warning (⚠), not a failure, so you know to check them without failing the whole unit.

---

## 📁 Project Structure

```
SpaceChecker/
├── SpaceChecker/
│   ├── Excel/
│   │   ├── Export/           # ComparisonExporter, ProgramTemplateExporter, ProjectDataExporter
│   │   ├── Import/           # SpaceProgramImporter
│   │   └── ProgramSheetSchema.cs  # Single source of truth for the Excel layout
│   ├── Revit/
│   │   ├── Entry/            # ExtApp, ExtCmd, DependencyResolver
│   │   ├── Extraction/       # AreaExtractor, RoomExtractor, RoomUnitMapper, SpaceProgramComparer
│   │   ├── Models/           # AreaItem, RoomItem, UnitItem, ApartmentCheck, ProgramApartment…
│   │   └── Services/         # SpaceProgramService
│   └── UI/
│       ├── Commands/         # RelayCommand
│       ├── Converters/       # StatusToColorConverter
│       ├── ViewModels/       # MainViewModel, ApartmentRowVM, RoomCheckVM
│       └── Views/            # MainWindow.xaml
├── SpaceChecker.sln
└── README.md
```

---

## 🗺️ Roadmap

- [ ] PDF export of compliance report
- [ ] Revit 2025 / 2026 support
- [ ] Write pass/fail status back to Revit room parameters
- [ ] Filter by phase or workset
- [ ] Multi-language support (Arabic / English)
- [ ] BIM 360 / Revit Cloud integration

---

## 👤 Author

**Mahmoud Amr Allam**
Architect & BIM Software Developer

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-blue?style=flat-square&logo=linkedin)](https://www.linkedin.com/in/mahmoud-allam-4a25b4172/)
[![Email](https://img.shields.io/badge/Email-mahmoud.amr55@gmail.com-red?style=flat-square&logo=gmail)](mailto:mahmoud.amr55@gmail.com)
[![GitHub](https://img.shields.io/badge/GitHub-MahmoudAllamAEC-black?style=flat-square&logo=github)](https://github.com/MahmoudAllamAEC)

---

<div align="center">

⭐ **If this project helped you, please give it a star!** ⭐

</div>
