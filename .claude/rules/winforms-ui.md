---
paths:
  - "**/*UserControl.cs"
  - "**/*UserControl.Designer.cs"
  - "**/*.resx"
  - "Views/**/*.cs"
---

# WinForms UI Design System

## Source of truth

The canonical reference implementation is:

- `Views/ScanBarcodeUserControl.cs`
- `Views/ScanBarcodeUserControl.Designer.cs`
- `Views/ScanBarcodeUserControl.resx`

Visual reference:

- `docs/design-reference/scan-barcode.png`

Before creating or redesigning another page, inspect the reference
UserControl and reproduce its design language rather than inventing
an unrelated style.

## Technology

- C# WinForms
- Visual Studio 2022
- AntdUI
- Designer-compatible controls only
- UserControl for application pages

Never convert the project to WPF, WinUI, MAUI, Avalonia or Web UI.

## Visual style

- Industrial control application
- Clean, simple and readable from a distance
- Blue primary background
- White or light-gray content panels
- Dark-blue borders
- Large headings
- Clear input labels
- Large action buttons
- Consistent spacing
- Limited decorative effects

## Design tokens

Primary background:

`#5B9BD5`

Dark border:

`#244765`

Card background:

`#FFFFFF`

Secondary surface:

`#DCE9F5`

Primary text:

`#111111`

Secondary text:

`#333333`

Success:

`#A8D58D`

Danger:

`#F5222D`

## Typography

Use the existing project font.

When no project font exists:

- Font family: Segoe UI
- Page title: 38–48 px, Bold
- Section title: 22–28 px, Semibold
- Form label: 17–21 px
- Input text: 16–20 px
- Button text: 20–24 px

## Spacing

Use consistent spacing values:

- Page outer padding: 32–48 px
- Card inner padding: 24–40 px
- Control vertical spacing: 12–20 px
- Related controls: 8–12 px
- Button gap: 48–80 px

Avoid random spacing values unless necessary.

## Layout

Prefer:

- TableLayoutPanel for structured pages and forms
- FlowLayoutPanel for groups of buttons
- Dock for primary containers
- Anchor for resizable controls
- Percent-based rows and columns
- AutoSize only when it does not break alignment

Avoid:

- Building the whole page using absolute Location values
- Overlapping controls
- Deeply nested panels without purpose
- Runtime-generated layout

## AntdUI

Prefer AntdUI controls for:

- Button
- Input
- Label
- Panel
- Select
- Checkbox
- Radio
- Tabs
- Badge and status indicators

Use standard WinForms layout containers when appropriate:

- TableLayoutPanel
- FlowLayoutPanel
- PictureBox

Only use properties that exist in the installed AntdUI version.
Do not guess property names.

## UserControl structure

Every new application page must include:

- `<PageName>UserControl.cs`
- `<PageName>UserControl.Designer.cs`
- `<PageName>UserControl.resx`

The `.cs` file should normally contain only:

- Constructor
- `InitializeComponent()`
- Explicitly requested event logic

The `.Designer.cs` file contains:

- Control fields
- Control creation
- Layout configuration
- Visual properties

## Required verification

After changing UI:

1. Run `dotnet restore` when dependencies changed.
2. Run `dotnet build`.
3. Fix errors caused by the change.
4. Check for duplicate control names.
5. Check event-handler references.
6. Confirm no UI controls are generated at runtime.
7. Report files changed.
8. Tell the user to open the UserControl in Visual Studio Designer
   for final visual inspection.
