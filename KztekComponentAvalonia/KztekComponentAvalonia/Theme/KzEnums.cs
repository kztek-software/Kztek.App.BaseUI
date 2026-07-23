namespace KztekComponentAvalonia.Theme;

public enum KzButtonVariant { Primary, Accent, Secondary, Ghost, Danger }
public enum KzSize { Sm, Md, Lg }
public enum KzBadgeVariant { Success, Warning, Error, Info, Neutral, Accent }
public enum KzBadgeType { StatusPill, Tag }
public enum KzCardVariant { Default, Dark, Feature }
public enum KzInputState { Normal, Focus, Disabled, Error }
public enum KzNumericLayout { Horizontal, Vertical }
public enum KzNumericVariant { Default, Navy, Accent }

// ── New enums for ported controls ────────────────────────────────────────────

public enum KzLabelType
{
    H1,          // 36px Bold
    H2,          // 28px Bold
    H3,          // 20px Bold
    Subheading,  // 16px SemiBold, Navy700
    Body,        // 14px Regular — default
    Small,       // 12px Regular
    Caption,     // 11px Regular, Muted
    Muted        // 14px Regular, Muted color
}

public enum KzGroupBoxVariant
{
    Default,   // White bg, light border
    Filled,    // Navy-100 bg, light border
    Outlined   // Transparent bg, 2px border
}

public enum KzProgressBarVariant
{
    Primary,   // Navy fill
    Accent,    // Orange fill
    Success    // Green fill
}

// ── Device tree enums ────────────────────────────────────────────────────────

public enum KzDeviceStatus { Online, Offline, Warning, Unknown }

public enum KzDeviceType
{
    Group,
    Camera,
    Controller,
    EntryGate,
    ExitGate,
    Sensor,
    Server,
    Network,
    Generic
}
