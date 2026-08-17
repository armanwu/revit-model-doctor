# Revit Model Doctor

Model Doctor is a C# .NET 10 Revit Add-In that performs automated health check audits on Revit models and presents results via an interactive WPF Dashboard.

---

## Key Features

- Ribbon Integration: Registers a "Model Doctor" panel under the default Revit Add-Ins tab.
- Modular Rule Engine:
  - Direct CAD Imports Check: Detects unlinked imported CAD drawings affecting model performance.
  - Model Warnings Check: Evaluates document warnings and affected elements.
- WPF Dashboard: Interactive UI displaying rule status, offending Element IDs, element-specific error/warning explanations, and one-click Element ID copying.

---

## System Requirements

- Target Application: Autodesk Revit 2027 (Tested)
- Target Framework: .NET 10 (net10.0-windows)

---

## Installation & Usage

1. Installation: Run `Install.bat` to build the project and deploy the add-in files to `%APPDATA%\Autodesk\Revit\Addins\2027\`.
2. Usage: Open Revit 2027, go to the Add-Ins ribbon tab, and click "Run Health Check" in the Model Doctor panel.
3. Uninstallation: Run `Uninstall.bat`.

---

## Copyright

Created by Arman Arisman  
Developed with AI assistance.  
Copyright (c) 2026 Arman Arisman. All rights reserved.
