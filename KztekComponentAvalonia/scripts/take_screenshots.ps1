Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type @'
using System;
using System.Runtime.InteropServices;
public class Win32a {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
    [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr h, int x, int y, int w, int ht, bool repaint);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, ref RECT r);
    [DllImport("user32.dll")] public static extern void mouse_event(int f, int x, int y, int d, int e2);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, int flags, int extra);
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
    public const int MOUSEEVENTF_LEFTDOWN = 0x02;
    public const int MOUSEEVENTF_LEFTUP   = 0x04;
    public const byte VK_MENU = 0x12;
    public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
    public const int KEYEVENTF_KEYUP       = 0x0002;
}
'@

$outDir = "H:\Vitecode\Claude\KztekComponentAvalonia\screenshots"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function FocusWin { param($hwnd)
    [Win32a]::keybd_event([Win32a]::VK_MENU, 0, [Win32a]::KEYEVENTF_EXTENDEDKEY, 0)
    [Win32a]::SetForegroundWindow($hwnd) | Out-Null
    [Win32a]::keybd_event([Win32a]::VK_MENU, 0, ([Win32a]::KEYEVENTF_EXTENDEDKEY -bor [Win32a]::KEYEVENTF_KEYUP), 0)
    Start-Sleep -Milliseconds 300
}

function Shot { param($hwnd, $path)
    FocusWin $hwnd
    Start-Sleep -Milliseconds 900
    $r = New-Object Win32a+RECT
    [Win32a]::GetWindowRect($hwnd, [ref]$r) | Out-Null
    $w = $r.R - $r.L; $h = $r.B - $r.T
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g   = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($w, $h)))
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    Write-Host "Saved: $path"
}

function Click { param($x, $y)
    [Win32a]::SetCursorPos($x, $y) | Out-Null
    Start-Sleep -Milliseconds 60
    [Win32a]::mouse_event([Win32a]::MOUSEEVENTF_LEFTDOWN, $x, $y, 0, 0)
    Start-Sleep -Milliseconds 60
    [Win32a]::mouse_event([Win32a]::MOUSEEVENTF_LEFTUP, $x, $y, 0, 0)
    Start-Sleep -Milliseconds 500
}

function Scroll { param($x, $y, $ticks = 8)
    [Win32a]::SetCursorPos($x, $y) | Out-Null
    Start-Sleep -Milliseconds 100
    for ($i = 0; $i -lt $ticks; $i++) {
        [Win32a]::mouse_event(0x0800, $x, $y, -120, 0)
        Start-Sleep -Milliseconds 40
    }
    Start-Sleep -Milliseconds 700
}

$exe  = "H:\Vitecode\Claude\KztekComponentAvalonia\KztekPreviewAvalonia\bin\Release\net8.0\KztekPreviewAvalonia.exe"
$proc = Start-Process $exe -PassThru
Write-Host "Launched PID $($proc.Id)"
Start-Sleep -Seconds 5

$hwnd = $null
for ($i = 0; $i -lt 15; $i++) {
    $p2 = Get-Process -Id $proc.Id -ErrorAction SilentlyContinue
    if ($p2 -and $p2.MainWindowHandle -ne [IntPtr]::Zero) { $hwnd = $p2.MainWindowHandle; break }
    Start-Sleep -Milliseconds 800
}
if (-not $hwnd) { Write-Host "Not found"; $proc.Kill(); exit 1 }

[Win32a]::ShowWindow($hwnd, 9) | Out-Null
[Win32a]::MoveWindow($hwnd, 0, 0, 1200, 780, $true) | Out-Null
FocusWin $hwnd
Start-Sleep -Milliseconds 1000

$r = New-Object Win32a+RECT
[Win32a]::GetWindowRect($hwnd, [ref]$r) | Out-Null
$winX = $r.L; $winY = $r.T
Write-Host "Window at ($winX, $winY)"

# Avalonia sidebar: width ~220px, item height ~48px
$cx = $winX + 600; $cy = $winY + 400

function NavKey { param($vk)
    # Use keybd_event directly — not intercepted by child controls
    FocusWin $hwnd
    Start-Sleep -Milliseconds 200
    [Win32a]::keybd_event($vk, 0, 0, 0)          # key down
    Start-Sleep -Milliseconds 60
    [Win32a]::keybd_event($vk, 0, 2, 0)           # key up
    Start-Sleep -Milliseconds 700
}

# VK codes for F1-F7
$VK_F1 = 0x70; $VK_F2 = 0x71; $VK_F3 = 0x72; $VK_F4 = 0x73
$VK_F5 = 0x74; $VK_F6 = 0x75; $VK_F7 = 0x76

# 1. Buttons (F1 = page 0)
NavKey $VK_F1
Shot $hwnd "$outDir\01_buttons.png"

# 2. Inputs (F2 = page 1)
NavKey $VK_F2
Shot $hwnd "$outDir\02_inputs.png"
Click $cx $cy
Scroll $cx $cy 8
Shot $hwnd "$outDir\02_inputs_scroll.png"

# 3. Badges (F3 = page 2)
NavKey $VK_F3
Shot $hwnd "$outDir\03_badges.png"

# 4. Cards (F4 = page 3)
NavKey $VK_F4
Shot $hwnd "$outDir\04_cards.png"

# 7. Form Controls (F7 = page 6)
NavKey $VK_F7
Shot $hwnd "$outDir\07_form_top.png"
Click $cx $cy
Scroll $cx $cy 8
Shot $hwnd "$outDir\07_form_mid.png"
Scroll $cx $cy 10
Shot $hwnd "$outDir\07_form_bot.png"

$proc.Kill()
Write-Host "Done"
