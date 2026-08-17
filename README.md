# Revit Model Doctor

Model Doctor is a C# .NET 10 Revit Add-In that performs automated health check audits and Quality Control (QC) assessments on Revit models via an interactive modeless WPF dashboard.

## Key Features

- **Overall Model QC Rating**: Real-time assessment banner (*PASSED QC*, *PASSED WITH WARNINGS*, *FAILED QC*).
- **Category & Rule Ignore**: Toggle and exclude specific categories or rule names from QC evaluation.
- **Modeless Revit Navigation**: Locate, select, and activate offending elements or views in Revit directly from the dashboard.
- **Persistent Element Ignore**: Suppress false positives permanently inside the `.rvt` file via `ExtensibleStorage`.
- **Modular Health Rules**: Audits CAD imports, in-place families, unpinned datums/links, unplaced views, view templates, and native warnings.
- **CSV Audit Export**: Export audit results and QC status reports to CSV.

## Quick Start

1. **Install**: Run `Install.bat` to build and deploy to Revit 2027 (`%APPDATA%\Autodesk\Revit\Addins\2027\`).
2. **Run**: In Revit 2027, open the **Add-Ins** ribbon tab and click **Run Health Check**.
3. **Uninstall**: Run `Uninstall.bat`.

## Copyright

Created by Arman Arisman.  
Developed with AI assistance.  
Copyright (c) 2026 Arman Arisman. All rights reserved.
