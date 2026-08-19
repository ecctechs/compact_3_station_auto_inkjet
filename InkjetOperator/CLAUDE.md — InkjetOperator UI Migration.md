# Project Context

There are 3 important directories in this workspace:

```text
NastoKeyence/
InkjetOperator/
AntdUI-main/
```

## Roles

`NastoKeyence` is the **UI Reference Project**.

`InkjetOperator` is the **Target Project** that must be modified.

`AntdUI-main` is the **AntdUI framework source reference** and should be used to verify AntdUI APIs when necessary.

---

# Main Goal

Improve the UI quality of `InkjetOperator` so its:

- text rendering
- DPI behavior
- font rendering
- AntdUI appearance
- buttons
- notification
- message
- modal
- popup
- dialog
- animation
- radius
- visual consistency

match the quality and behavior of `NastoKeyence` as closely as practical.

---

# CRITICAL RULE

NastoKeyence is ONLY a UI / presentation reference.

DO NOT copy NastoKeyence business logic into InkjetOperator.

Do not copy:

- printer logic
- Keyence logic
- database logic
- TCP logic
- PLC logic
- service logic
- production workflow
- Nasto-specific features

Preserve InkjetOperator business logic.

---

# Reference Priority

When implementing UI behavior, inspect NastoKeyence first.

Especially inspect:

```text
NastoKeyence/src/Nasto.KeyenceLink.WinForms/Program.cs
NastoKeyence/src/Nasto.KeyenceLink.WinForms/Nasto.KeyenceLink.WinForms.csproj

NastoKeyence/src/Nasto.KeyenceLink.WinForms/Theme/
NastoKeyence/src/Nasto.KeyenceLink.WinForms/Forms/
NastoKeyence/src/Nasto.KeyenceLink.WinForms/Controls/
```

Search NastoKeyence for:

```text
AntdUI.Config
AntdUI.Message
AntdUI.Notification
AntdUI.Modal
AntdUI.Window
AntdUI.BorderlessForm
DesignTokens
ButtonStyles
PerMonitorV2
TextRenderingHighQuality
ShowInWindow
```

---

# AntdUI API Rule

Do not invent AntdUI APIs.

If an AntdUI API is unclear, inspect:

```text
AntdUI-main/src/AntdUI/
AntdUI-main/example/
AntdUI-main/doc/
```

before implementing it.

---

# Designer Rule

InkjetOperator is a WinForms application.

Preserve Visual Studio WinForms Designer compatibility.

Static UI should remain in:

```text
*.Designer.cs
```

Behavior should remain in:

```text
*.cs
```

Do not recreate entire forms dynamically in constructors unless runtime-generated UI is genuinely required.

Do not unnecessarily rewrite existing Designer files.

---

# Modification Rule

Before modifying a screen:

1. Read its `.cs`
2. Read its `.Designer.cs`
3. Understand existing event handlers
4. Understand business behavior
5. Modify only presentation code required for the task

Never remove existing functionality unless explicitly requested.

---

# Build Rule

After every meaningful phase run:

```bash
dotnet restore
dotnet build
```

Fix compile errors before proceeding.

Do not claim a task is completed if the project does not build.

---

# IMPORTANT

Do NOT migrate InkjetOperator to another .NET version as part of UI parity work unless explicitly instructed.

UI parity and .NET migration are separate tasks.