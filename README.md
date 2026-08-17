# Revit Model Doctor

Model Doctor is a C# .NET 10 Revit Add-In that performs automated health check audits on Revit models and presents real-time results via a modeless, interactive WPF Dashboard.

---

## Key Features

- **Ribbon Integration**: Registers a "Model Doctor" panel under the default Revit Add-Ins tab.
- **Modeless & Interactive Dashboard**: Operates non-modally via Revit `ExternalEvent` architecture—allowing modelers and BIM managers to freely navigate, edit, and click within Revit while keeping the audit dashboard open.
- **Select & Show in Revit**: One-click element locator button and double-click interaction to highlight and zoom to any offending element directly in active Revit views.
- **Categorized Audit Rules**:
  - **Imports & Links**: Detects unlinked, directly imported CAD drawings (`.dwg`).
  - **Model Hygiene & Performance**:
    - **In-Place Families Check**: Identifies in-place family instances affecting file size and regeneration speed.
    - **Unpinned Datum & Links Check**: Flags unpinned Grids, Levels, and Revit Links at risk of accidental displacement.
  - **Views & Sheets Health**:
    - **Unplaced Model Views Check**: Detects printable model views not assigned to any drawing sheet.
    - **Views Without View Template Check**: Flags views missing View Templates to maintain graphic standards.
  - **Granular Warning Classifier**: Automatically categorizes native Revit warnings into specific sub-categories:
    - *Warnings - Hosting* (Unhosted / Lost Host Elements)
    - *Warnings - MEP Systems* (Unconnected Pipes, Ducts, Electrical Circuits)
    - *Warnings - Geometry* (Overlapping & Duplicate Elements, Curtain Wall Issues)
    - *Warnings - Rooms & Spaces* (Unenclosed / Redundant Rooms)
    - *Warnings - Structural* (Structural & Analytical Misalignments)
    - *Warnings - General* (Other Revit Model Warnings)
- **Reporting & Export**: One-click Copy Element ID and full CSV Audit Report Export.

---

## System Requirements

- **Target Application**: Autodesk Revit 2027 (Tested)
- **Target Framework**: .NET 10 (`net10.0-windows`)

---

## Installation & Usage

1. **Installation**: Run `Install.bat` to build the project and deploy the add-in files to `%APPDATA%\Autodesk\Revit\Addins\2027\`.
2. **Usage**: Open Revit 2027, go to the **Add-Ins** ribbon tab, and click **Run Health Check** in the Model Doctor panel.
3. **Uninstallation**: Run `Uninstall.bat`.

---

## Copyright

Created by Arman Arisman  
Developed with AI assistance.  
Copyright (c) 2026 Arman Arisman. All rights reserved.
