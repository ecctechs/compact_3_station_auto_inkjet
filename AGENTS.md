# Project Instructions

This is a C# WinForms project developed with Visual Studio 2022.

## Project facts

- Solution: `CompactInkjet.sln` (repo root).
- Main project: `InkjetOperator/InkjetOperator.csproj`.
- Target Framework: `net8.0-windows` (do not change).
- UI package: AntdUI `2.4.3` (stable). Do not use preview versions.
- Namespace convention: root namespace is `InkjetOperator`; the `Views/` folder maps to `InkjetOperator.Views`.
- Build the solution with: `dotnet build CompactInkjet.sln`.

## UI framework

- Use AntdUI for new UI controls.
- New application pages must be created as UserControl.
- Use `Views/ScanBarcodeUserControl` as the primary visual and structural reference.
- The reference screenshot is located at:
  `docs/design-reference/scan-barcode.png`

## Designer requirement

All UI must remain editable in the Visual Studio WinForms Designer.

- Create controls inside `InitializeComponent()`.
- Keep UI declarations in `.Designer.cs`.
- Do not dynamically create controls at runtime.
- Do not create methods such as `BuildUI`, `CreateLayout` or `ApplyStyles`.
- Do not use custom painting, GDI+, `OnPaint` or Paint events.
- Do not place UI-building code in constructors or Load events.
- Use TableLayoutPanel, FlowLayoutPanel, Dock, Anchor, Padding and Margin.
- Preserve Designer compatibility.

## When creating a new page

1. Inspect `ScanBarcodeUserControl` first.
2. Reuse the same design system and visual proportions.
3. Keep typography, colors, spacing, borders and button style consistent.
4. Create only the requested UserControl.
5. Do not add the page to the main menu unless explicitly requested.
6. Do not add business logic unless explicitly requested.
7. Run `dotnet build` after making changes.
