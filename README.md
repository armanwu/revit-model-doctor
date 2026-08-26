# Revit Model Doctor

**Revit Model Doctor** is an automated Quality Control (QC) and Model Health Audit engine for **Autodesk Revit 2025, 2026, and 2027**. Built with C# .NET 8 (modern runtime), it features an interactive, modeless WPF dashboard designed to monitor database hygiene, documentation accuracy, and spatial coordinate safety in real-time.

---

## 💻 System Compatibility

- **Supported Revit Versions**: **Autodesk Revit 2025, 2026, and 2027**.
- **Zero-Dependency Plug & Play**: **No .NET SDK, Visual Studio, or developer tools required** for end users. The add-in utilizes pre-compiled binaries and Revit's native runtime.
- **Interactive Multi-Version Installer**: Choose target Revit version interactively (`Revit 2025`, `Revit 2026`, `Revit 2027`, or `ALL`).
- **Tested & Verified Environments**: Autodesk Revit 2025, 2026, and 2027 on Windows 10/11.

---

## 🌟 Key Features

- **Overall Health Score & Status Banner**: Dynamic overall health score calculation (0% - 100%) and status classification (*🟢 HEALTHY*, *🟡 NEEDS ATTENTION*, *🔴 CRITICAL*).
- **3-Category Classification Engine**: Categorizes audit rules into **Model Performance**, **Data & Deliverable Integrity**, and **Spatial & Model Safety**.
- **⚡ Safe 1-Click Quick Fix (Auto-Remediation)**: Automate rule remediation (e.g. pin unpinned Grids/Levels/Links, purge unused View Filters/Templates) directly from the modeless WPF dashboard with full Revit `Ctrl + Z` undo safety.
- **Interactive Health & Scoring Guide**: Click **❓ Help & Scoring Guide** in the dashboard header to view industry metric thresholds and category definitions.
- **Industry Standard Metric Thresholds**: Implements strict BIM quality control thresholds for warnings, CAD imports, in-place families, purge items, rooms, view extents, and coordinate placement.
- **Interactive Element Selection & View Navigation**: Double-click or click **Select Element** in the modeless dashboard to zoom, isolate, and highlight offending elements directly in active Revit views.
- **Persistent Element Ignore**: Suppress false positives permanently inside the `.rvt` project database using Revit `ExtensibleStorage`.
- **Dynamic Category & Rule Filtering**: Toggle entire categories or individual rule names to customize QC evaluations.
- **CSV Report Export**: Export detailed audit results, counts, and offending element IDs to CSV for team reports.

---

## 📊 Overall Health Scoring Logic

The dashboard computes an **Overall Model Health Score** percentage based on evaluated active rules:

| Overall Score | Status Classification | Status Message & Action Required | Banner Color |
| :---: | :--- | :--- | :--- |
| **85% – 100%** | 🟢 **PASS / HEALTHY** | Model is clean, performance optimal, ready for coordination and deliverables. | Green (`#10B981`) |
| **65% – 84%** | 🟡 **WARNING / NEEDS ATTENTION** | Model runs smoothly, but periodic cleanup is required before issues expand. | Orange (`#F59E0B`) |
| **< 65%** | 🔴 **FAIL / CRITICAL** | High risk of model sluggishness, file corruption, or inaccurate quantity schedules. Immediate fix required! | Red (`#EF4444`) |

---

## 📋 Health Check Categories & Industry Metric Thresholds

### 1. Model Performance
*Focuses on file stability, loading speed, and memory (RAM) optimization.*

| Audit Rule | 🟢 Pass Threshold | 🟡 Warning Threshold | 🔴 Fail Threshold | Issue & Risk Prevented |
| :--- | :---: | :---: | :---: | :--- |
| **Active Warnings** | $\le 50$ warnings | $51 - 200$ warnings | $> 200$ warnings | Model sluggishness, lag, and potential file corruption. |
| **Imported CAD Files** | $0$ CAD imports | $1 - 2$ (2D View only) | $> 2$ or 3D Model layer CAD | Polluted line styles/layers and severe 3D viewport lag. |
| **In-Place Families** | $\le 2$ elements | $3 - 10$ elements | $> 10$ elements | Degraded graphics rendering performance and inflated file size. |
| **Purgeable Items** | $\le 100$ items | $101 - 500$ items | $> 500$ items | Bloated database size and slow save/open operations. |
| **Unused View Filters & Templates** | $\le 5$ items | $6 - 20$ items | $> 20$ items | Unnecessary database clutter and inconsistent graphics standards. |

### 2. Data & Deliverable Integrity
*Focuses on quantity schedule accuracy and documentation deliverable quality.*

| Audit Rule | 🟢 Pass Threshold | 🟡 Warning Threshold | 🔴 Fail Threshold | Issue & Risk Prevented |
| :--- | :---: | :---: | :---: | :--- |
| **Unplaced & Unenclosed Rooms** | $0$ issues | $1 - 5$ rooms | $> 5$ rooms | Inaccurate room/space area calculations and schedule errors. |
| **Views Not on Sheets** | $< 20\%$ of views | $20\% - 40\%$ of views | $> 40\%$ of views | Cluttered Project Browser and working view sprawl. |
| **Unused Schedules & Legends** | $0$ unplaced | $1 - 5$ unplaced | $> 5$ unplaced | Unused schedule/legend definitions cluttering documentation output. |
| **Broken Links & IFC Status** | $0$ issues | $1$ Unloaded link | $\ge 1$ Not Found link | Missing reference models or circular attachment nesting. |
| **Duplicate Instances** | $0$ elements | $1 - 5$ elements | $> 5$ elements | Double counting in quantity takeoff schedules and overlapping geometry. |
| **Model Group Duplication** | $0$ unused groups | $1 - 3$ unused groups | $> 3$ unused groups | Unused group definitions bloating file size. |
| **Workset Allocation** | $0$ default workset | $1 - 5$ default workset | $> 5$ default workset | Important elements (Grids/Levels/Links) placed on user default worksets. |

### 3. Spatial & Model Safety
*Focuses on visual graphics stability and preventing accidental modeling errors.*

| Audit Rule | 🟢 Pass Threshold | 🟡 Warning Threshold | 🔴 Fail Threshold | Issue & Risk Prevented |
| :--- | :---: | :---: | :---: | :--- |
| **Large Model Extents** | $0$ elements > 16 km | $0$ elements (strict) | $\ge 1$ element > 16 km | Floating-point graphics corruption and disappearing geometry when zooming. |
| **Unpinned Grids & Levels** | $100\%$ Pinned | $90\% - 99\%$ Pinned | $< 90\%$ Pinned | Accidental displacement of building reference axes or level datums. |
| **View Clipping & Extents** | $0$ unclipped | $1 - 5$ unclipped | $> 5$ unclipped | Infinite view rendering depth causing viewport lag. |

---

## 🚀 Quick Start & Installation

### Zero-Dependency Installation (For Non-Developers & End-Users)
1. Close Autodesk Revit.
2. Double-click **`Install.bat`** in the root folder.
3. Select your target Revit version from the interactive menu:
   - `[1]` Revit 2025
   - `[2]` Revit 2026
   - `[3]` Revit 2027
   - `[4]` Install for ALL versions (2025, 2026, 2027)
4. *Done!* No .NET SDK, Visual Studio, or extra installation is required.

### Running in Autodesk Revit
1. Launch Autodesk Revit (2025, 2026, or 2027) and open any project file (`.rvt`).
2. Navigate to the **Add-Ins** ribbon tab.
3. Click **Run Health Check** in the Model Doctor panel to open the Modeless Dashboard.

### Clean 1-Click Uninstallation
1. Close Autodesk Revit.
2. Double-click **`Uninstall.bat`**.
3. Select target version (`[1] 2025`, `[2] 2026`, `[3] 2027`, or `[4] ALL`).

---

## 🛠 Project Architecture

```text
src/
├── Commands/
│   └── CmdRunHealthCheck.cs      # External Command entry point & rule registry
├── Core/
│   ├── CategoryFilterItem.cs     # Category ignore filter model
│   ├── HealthRuleResult.cs       # Rule result & offending element collection
│   ├── IHealthCheckRule.cs       # Modular health rule interface contract
│   ├── IgnoreElementHandler.cs   # ExternalEvent handler for persistent ignore
│   ├── IgnoreStorageService.cs   # Revit ExtensibleStorage service
│   ├── OffendingElementInfo.cs   # Offending element record & selection details
│   └── SelectElementHandler.cs   # ExternalEvent handler for Revit element selection
├── Rules/
│   ├── CadImportRule.cs
│   ├── DuplicateElementsRule.cs
│   ├── InPlaceFamilyRule.cs
│   ├── ModelGroupDuplicationRule.cs
│   ├── PurgeableElementsRule.cs
│   ├── RevitLinksAndIfcStatusRule.cs
│   ├── SurveyAndBasePointDistanceRule.cs
│   ├── UnboundedRoomsAndSpacesRule.cs
│   ├── UnpinnedGridsAndLevelsRule.cs
│   ├── UnplacedViewsRule.cs
│   ├── UnusedSchedulesAndLegendsRule.cs
│   ├── UnusedViewFiltersAndTemplatesRule.cs
│   ├── ViewClippingAndExtentsRule.cs
│   ├── WarningCountRule.cs
│   └── WorksetAllocationRule.cs
├── ViewModels/
│   └── HealthCheckDashboardViewModel.cs # Modeless WPF ViewModel & QC calculation engine
└── Views/
    ├── HealthCheckDashboardView.xaml    # WPF Dashboard UI
    └── HelpView.xaml                    # Interactive Help & Scoring Guide window
```

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

Copyright (c) 2026 Arman Arisman.
