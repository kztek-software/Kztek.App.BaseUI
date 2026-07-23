using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using KztekComponentAvalonia.Theme;

namespace KztekComponentAvalonia.Controls;

/// <summary>
/// KZTEK Design System virtual keyboard — Alpha / Numeric / Special modes,
/// Portrait and Landscape orientations, with Vietnamese Telex input.
/// </summary>
public class KzKeyboard : Control
{
    // ── Public enums ─────────────────────────────────────────────────────────
    public enum KzKeyboardOrientation { Portrait, Landscape }
    public enum KzKeyboardMode       { Alpha, Numeric, Special }

    // ── Events ────────────────────────────────────────────────────────────────
    public event EventHandler<string>?               KeyPressed;
    public event EventHandler<KzKeyboardOrientation>? OrientationChanged;

    // ── Key definition ────────────────────────────────────────────────────────
    private record struct KD(string Lbl, string LblUp, string Val, string ValUp, double W, bool Special);

    private static KD A(string lc) => new(lc, lc.ToUpperInvariant(), lc, lc.ToUpperInvariant(), 1.0, false);
    private static KD C(string ch) => new(ch, ch, ch, ch, 1.0, false);
    private static KD S(string lbl, string cmd, double w = 1.5) => new(lbl, lbl, cmd, cmd, w, true);
    private static KD SP(double w = 3.5) => new("Space", "Space", " ", " ", w, true);

    // ── Key layout tables ─────────────────────────────────────────────────────
    private static readonly KD[][] AlphaRows =
    {
        new[] { A("q"),A("w"),A("e"),A("r"),A("t"),A("y"),A("u"),A("i"),A("o"),A("p") },
        new[] { A("a"),A("s"),A("d"),A("f"),A("g"),A("h"),A("j"),A("k"),A("l") },
        new[] { S("⇧","SHIFT",1.5), A("z"),A("x"),A("c"),A("v"),A("b"),A("n"),A("m"), S("⌫","BACK",1.5) },
        new[] { S("?123","NUM",1.5), C(","), SP(3.5), C("."), S("VIE","TELEX",1.2), S("↵","ENTER",1.5) }
    };

    private static readonly KD[][] NumRows =
    {
        new[] { C("1"),C("2"),C("3"),C("4"),C("5"),C("6"),C("7"),C("8"),C("9"),C("0") },
        new[] { C("@"),C("#"),C("$"),C("%"),C("^"),C("-"),C("+"),C("("),C(")"),C("/") },
        new[] { S("#+=","SPEC",1.5), C("*"),C("\""),C("'"),C(":"),C(";"),C("!"),C("?"), S("⌫","BACK",1.5) },
        new[] { S("ABC","ALPHA",1.5), C(","), SP(4.0), C("."), S("↵","ENTER",1.5) }
    };

    private static readonly KD[][] SpecRows =
    {
        new[] { C("["),C("]"),C("{"),C("}"),C("<"),C(">"),C("="),C("_"),C("\\"),C("|") },
        new[] { C("~"),C("^"),C("`"),C("+"),C("-"),C("*"),C("/"),C(":"),C(";"),C("@") },
        new[] { S("?123","NUM",1.5), C("."),C(","),C(":"),C(";"),C("!"),C("?"), S("⌫","BACK",1.5) },
        new[] { S("ABC","ALPHA",1.5), C("-"), SP(4.0), C("/"), S("↵","ENTER",1.5) }
    };

    // ── Avalonia Styled Properties ────────────────────────────────────────────
    public static readonly StyledProperty<KzKeyboardOrientation> OrientationProperty =
        AvaloniaProperty.Register<KzKeyboard, KzKeyboardOrientation>(
            nameof(Orientation), KzKeyboardOrientation.Portrait);

    public static readonly StyledProperty<TextBox?> TargetTextBoxProperty =
        AvaloniaProperty.Register<KzKeyboard, TextBox?>(nameof(TargetTextBox));

    public static readonly StyledProperty<bool> TelexEnabledProperty =
        AvaloniaProperty.Register<KzKeyboard, bool>(nameof(TelexEnabled), false);

    public KzKeyboardOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set
        {
            if (GetValue(OrientationProperty) == value) return;
            SetValue(OrientationProperty, value);
            ApplyOrientation(value);
            InvalidateVisual();
        }
    }

    public TextBox? TargetTextBox
    {
        get => GetValue(TargetTextBoxProperty);
        set => SetValue(TargetTextBoxProperty, value);
    }

    public bool TelexEnabled
    {
        get => GetValue(TelexEnabledProperty);
        set
        {
            SetValue(TelexEnabledProperty, value);
            TelexResetSyllable();
            InvalidateVisual();
        }
    }

    // ── State ─────────────────────────────────────────────────────────────────
    private KzKeyboardMode _mode           = KzKeyboardMode.Alpha;
    private bool           _shifted        = false;
    private bool           _capsLock       = false;
    private bool           _telexEnabled   = false;
    private string         _telexRaw       = "";
    private int            _telexDisplayLen = 0;
    private string?        _pressedVal;

    private readonly List<(Rect R, KD Key)> _layout = new();

    // ── Colours ───────────────────────────────────────────────────────────────
    private static readonly IBrush KeyBg     = Brushes.White;
    private static readonly IBrush KeyBgSpec = new SolidColorBrush(Color.Parse("#CBC7BF"));
    private static readonly IBrush KeyFg     = new SolidColorBrush(Color.Parse("#4A4A4A"));
    private static readonly IBrush KbBg      = new SolidColorBrush(Color.Parse("#F0EBE1"));
    private static readonly IBrush PressedBg = KzTokens.Navy900Brush;
    private static readonly IBrush PressedFg = Brushes.White;
    private static readonly IBrush Shadow    = new SolidColorBrush(Color.FromArgb(22, 0, 0, 0));
    private static readonly IPen   Border    = new Pen(new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)), 0.5);
    private const double           Pad       = 8;
    private const double           Gap       = 9;
    private const double           KeyRadius = 8;

    // ── Portrait / Landscape sizes ────────────────────────────────────────────
    public static readonly Size PortraitSize  = new(390, 300);
    public static readonly Size LandscapeSize = new(640, 220);

    // ── Constructor ───────────────────────────────────────────────────────────
    public KzKeyboard()
    {
        Width  = PortraitSize.Width;
        Height = PortraitSize.Height;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    // ── Public methods ────────────────────────────────────────────────────────
    public void ToggleOrientation() =>
        Orientation = Orientation == KzKeyboardOrientation.Portrait
            ? KzKeyboardOrientation.Landscape
            : KzKeyboardOrientation.Portrait;

    private void ApplyOrientation(KzKeyboardOrientation o)
    {
        if (o == KzKeyboardOrientation.Portrait)
        { Width = PortraitSize.Width;  Height = PortraitSize.Height; }
        else
        { Width = LandscapeSize.Width; Height = LandscapeSize.Height; }
        OrientationChanged?.Invoke(this, o);
    }

    // ── Rendering ─────────────────────────────────────────────────────────────
    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);
        _layout.Clear();

        var rows = _mode switch
        {
            KzKeyboardMode.Numeric => NumRows,
            KzKeyboardMode.Special => SpecRows,
            _                      => AlphaRows
        };

        double W     = Bounds.Width  - Pad * 2;
        double H     = Bounds.Height - Pad * 2;
        int    nRows = rows.Length;
        double rowH  = (H - Gap * (nRows - 1)) / nRows;
        double y     = Pad;

        foreach (var row in rows)
        {
            double totalW = 0;
            foreach (var k in row) totalW += k.W;
            double unitW = (W - Gap * (row.Length - 1)) / totalW;
            double x = Pad;

            foreach (var key in row)
            {
                var rect = new Rect(x, y, unitW * key.W, rowH);
                _layout.Add((rect, key));
                DrawKey(ctx, rect, key);
                x += unitW * key.W + Gap;
            }
            y += rowH + Gap;
        }
    }

    private void DrawKey(DrawingContext ctx, Rect r, KD key)
    {
        string val         = ResolveVal(key);
        bool   pressed     = _pressedVal == val;
        bool   shiftActive = val == "SHIFT" && (_shifted || _capsLock);
        bool   telexActive = val == "TELEX" && _telexEnabled;

        IBrush bg = pressed || shiftActive || telexActive ? PressedBg
                  : key.Special                           ? KeyBgSpec
                  : KeyBg;
        IBrush fg = (pressed || shiftActive || telexActive) ? PressedFg : KeyFg;

        // Telex key shows toggled label
        string lbl = val == "TELEX"
            ? (_telexEnabled ? "ENG" : "VIE")
            : ResolveLabel(key);

        // Drop shadow
        if (!pressed)
        {
            var shadowRect = new Rect(r.X + 1, r.Y + 2, r.Width, r.Height);
            ctx.DrawRectangle(Shadow, null, shadowRect, KeyRadius, KeyRadius);
        }

        // Key body
        ctx.DrawRectangle(bg, Border, r, KeyRadius, KeyRadius);

        if (lbl.Length == 0) return;

        double fs = r.Height < 30 ? 7.5
                  : r.Height < 42 ? 9.5
                  : r.Height < 58 ? 11.0
                  : r.Height < 80 ? 15.0
                  : 18.0;

        if      (lbl.Length >= 9) fs = Math.Max(fs * 0.52, 6.0);
        else if (lbl.Length >= 5) fs = Math.Max(fs * 0.85, 7.0);
        else if (lbl.Length >= 3) fs = Math.Max(fs * 0.90, 7.0);

        var ft = new FormattedText(
            lbl,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            fs,
            fg);

        var textOrigin = new Point(
            r.X + (r.Width  - ft.Width)  / 2,
            r.Y + (r.Height - ft.Height) / 2);

        ctx.DrawText(ft, textOrigin);
    }

    private string ResolveLabel(KD k) =>
        k.Special ? k.Lbl : (_shifted || _capsLock ? k.LblUp : k.Lbl);

    private string ResolveVal(KD k) =>
        k.Special ? k.Val : (_shifted || _capsLock ? k.ValUp : k.Val);

    // ── Pointer handling ──────────────────────────────────────────────────────
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var pos = e.GetPosition(this);

        foreach (var (rect, key) in _layout)
        {
            if (!rect.Contains(pos)) continue;
            string val = ResolveVal(key);
            _pressedVal = val;
            InvalidateVisual();
            ProcessKey(val);

            _ = System.Threading.Tasks.Task.Delay(110).ContinueWith(_ =>
            {
                _pressedVal = null;
                DispatcherTimer.RunOnce(InvalidateVisual, TimeSpan.Zero);
            });
            break;
        }
    }

    private void ProcessKey(string val)
    {
        switch (val)
        {
            case "SHIFT":
                if (_capsLock)     { _capsLock = false; _shifted = false; }
                else if (_shifted) { _shifted  = false; _capsLock = true; }
                else               { _shifted  = true; }
                InvalidateVisual();
                return;

            case "TELEX":
                _telexEnabled = !_telexEnabled;
                TelexResetSyllable();
                InvalidateVisual();
                return;

            case "BACK":
                DoBackspace();
                KeyPressed?.Invoke(this, "BACKSPACE");
                return;

            case "ENTER":
                TelexResetSyllable();
                TypeTextRaw("\n");
                KeyPressed?.Invoke(this, "ENTER");
                return;

            case "FLIP":
                ToggleOrientation();
                return;

            case "NUM":   _mode = KzKeyboardMode.Numeric; TelexResetSyllable(); InvalidateVisual(); return;
            case "ALPHA": _mode = KzKeyboardMode.Alpha;   TelexResetSyllable(); InvalidateVisual(); return;
            case "SPEC":  _mode = KzKeyboardMode.Special; TelexResetSyllable(); InvalidateVisual(); return;

            case " ":
                TelexResetSyllable();
                TypeTextRaw(" ");
                if (_shifted && !_capsLock) { _shifted = false; InvalidateVisual(); }
                KeyPressed?.Invoke(this, val);
                return;

            default:
                if (_telexEnabled && val.Length == 1 && char.IsLetter(val[0]))
                    TypeTelex(val);
                else
                {
                    TelexResetSyllable();
                    TypeTextRaw(val);
                    if (_shifted && !_capsLock) { _shifted = false; InvalidateVisual(); }
                }
                KeyPressed?.Invoke(this, val);
                return;
        }
    }

    // ── Telex input logic ─────────────────────────────────────────────────────

    private void TypeTelex(string val)
    {
        bool upper = _shifted || _capsLock;
        _telexRaw += char.ToLowerInvariant(val[0]);

        string newDisplay = TransformTelex(_telexRaw);
        if (upper && newDisplay.Length > 0)
            newDisplay = char.ToUpperInvariant(newDisplay[0]) + newDisplay.Substring(1);

        DeleteCharsBeforeCursor(_telexDisplayLen);
        InsertAtCursor(newDisplay);
        _telexDisplayLen = newDisplay.Length;

        if (_shifted && !_capsLock) { _shifted = false; InvalidateVisual(); }
    }

    private void TelexResetSyllable()
    {
        _telexRaw        = "";
        _telexDisplayLen = 0;
    }

    // ── Telex transformation engine ───────────────────────────────────────────

    private static string TransformTelex(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;

        string s = raw.ToLowerInvariant();

        // dd → đ
        s = s.Replace("dd", "đ");

        // Extract tone from end
        int tone = 0;
        if (s.Length >= 2)
        {
            char last   = s[s.Length - 1];
            string prefix = s.Substring(0, s.Length - 1);
            if (HasVowel(prefix) && TryParseTone(last, out int t))
            {
                if (last == 'r')
                {
                    char prev = prefix[prefix.Length - 1];
                    if (!IsBaseVowel(prev)) goto skipTone;
                }
                tone = t;
                s = prefix;
            }
        }
        skipTone:

        // Vowel base transforms
        s = s.Replace("uw", "ư").Replace("ow", "ơ")
             .Replace("aa", "â").Replace("aw", "ă")
             .Replace("ee", "ê").Replace("oo", "ô");

        if (tone > 0)
            s = ApplyToneToString(s, tone);

        return s;
    }

    private static bool TryParseTone(char c, out int tone)
    {
        switch (c)
        {
            case 's': tone = 1; return true;
            case 'f': tone = 2; return true;
            case 'r': tone = 3; return true;
            case 'x': tone = 4; return true;
            case 'j': tone = 5; return true;
            default:  tone = 0; return false;
        }
    }

    private static bool HasVowel(string s)
    {
        foreach (char c in s)
            if (IsBaseVowel(c) || IsVowelWithMark(c)) return true;
        return false;
    }

    private static bool IsBaseVowel(char c) =>
        "aeiouăâêôơư".IndexOf(c) >= 0;

    private static bool IsVowelWithMark(char c) =>
        "áàảãạắằẳẵặấầẩẫậéèẻẽẹếềểễệíìỉĩịóòỏõọốồổỗộớờởỡợúùủũụứừửữựýỳỷỹỵ".IndexOf(c) >= 0;

    private static string ApplyToneToString(string s, int tone)
    {
        int pos = FindTonicPosition(s);
        if (pos < 0) return s;

        char v = s[pos];
        if (!ToneTable.TryGetValue((v, tone), out string? toned)) return s;

        return s.Substring(0, pos) + toned + s.Substring(pos + 1);
    }

    private static int FindTonicPosition(string s)
    {
        const string vowels = "aeiouăâêôơư";
        var vp = new List<int>();
        for (int i = 0; i < s.Length; i++)
            if (vowels.IndexOf(s[i]) >= 0) vp.Add(i);

        if (vp.Count == 0) return -1;
        if (vp.Count == 1) return vp[0];

        int last = vp[vp.Count - 1];
        int prev = vp[vp.Count - 2];

        bool lastBeforeConsonant = last < s.Length - 1 && vowels.IndexOf(s[last + 1]) < 0;
        if (lastBeforeConsonant) return last;

        if (last == prev + 1)
        {
            char v1 = s[prev], v2 = s[last];
            if (v1 == 'o' && (v2 == 'a' || v2 == 'e' || v2 == 'ă')) return last;
            if (v1 == 'u' && v2 == 'y') return last;
            if (v2 == 'a') return last;
            return prev;
        }
        return last;
    }

    private static readonly Dictionary<(char, int), string> ToneTable = new()
    {
        {('a',1),"á"},{('a',2),"à"},{('a',3),"ả"},{('a',4),"ã"},{('a',5),"ạ"},
        {('ă',1),"ắ"},{('ă',2),"ằ"},{('ă',3),"ẳ"},{('ă',4),"ẵ"},{('ă',5),"ặ"},
        {('â',1),"ấ"},{('â',2),"ầ"},{('â',3),"ẩ"},{('â',4),"ẫ"},{('â',5),"ậ"},
        {('e',1),"é"},{('e',2),"è"},{('e',3),"ẻ"},{('e',4),"ẽ"},{('e',5),"ẹ"},
        {('ê',1),"ế"},{('ê',2),"ề"},{('ê',3),"ể"},{('ê',4),"ễ"},{('ê',5),"ệ"},
        {('i',1),"í"},{('i',2),"ì"},{('i',3),"ỉ"},{('i',4),"ĩ"},{('i',5),"ị"},
        {('o',1),"ó"},{('o',2),"ò"},{('o',3),"ỏ"},{('o',4),"õ"},{('o',5),"ọ"},
        {('ô',1),"ố"},{('ô',2),"ồ"},{('ô',3),"ổ"},{('ô',4),"ỗ"},{('ô',5),"ộ"},
        {('ơ',1),"ớ"},{('ơ',2),"ờ"},{('ơ',3),"ở"},{('ơ',4),"ỡ"},{('ơ',5),"ợ"},
        {('u',1),"ú"},{('u',2),"ù"},{('u',3),"ủ"},{('u',4),"ũ"},{('u',5),"ụ"},
        {('ư',1),"ứ"},{('ư',2),"ừ"},{('ư',3),"ử"},{('ư',4),"ữ"},{('ư',5),"ự"},
        {('y',1),"ý"},{('y',2),"ỳ"},{('y',3),"ỷ"},{('y',4),"ỹ"},{('y',5),"ỵ"},
    };

    // ── Text target helpers ───────────────────────────────────────────────────

    private void TypeTextRaw(string text)
    {
        var tb = TargetTextBox;
        if (tb is null) return;
        int pos = tb.CaretIndex;
        tb.Text = (tb.Text ?? "").Insert(pos, text);
        tb.CaretIndex = pos + text.Length;
    }

    private void InsertAtCursor(string text) => TypeTextRaw(text);

    private void DeleteCharsBeforeCursor(int count)
    {
        var tb = TargetTextBox;
        if (tb is null || count <= 0) return;
        int pos = tb.CaretIndex;
        int del = Math.Min(count, pos);
        if (del <= 0) return;
        string txt = tb.Text ?? "";
        tb.Text       = txt.Remove(pos - del, del);
        tb.CaretIndex = pos - del;
    }

    private void DoBackspace()
    {
        if (_telexEnabled && _telexRaw.Length > 0)
        {
            _telexRaw = _telexRaw.Substring(0, _telexRaw.Length - 1);
            string newDisplay = TransformTelex(_telexRaw);
            DeleteCharsBeforeCursor(_telexDisplayLen);
            if (newDisplay.Length > 0) InsertAtCursor(newDisplay);
            _telexDisplayLen = newDisplay.Length;
            return;
        }

        TelexResetSyllable();
        var tb = TargetTextBox;
        if (tb is null) return;
        int pos = tb.CaretIndex;
        if (pos <= 0 || string.IsNullOrEmpty(tb.Text)) return;
        tb.Text       = tb.Text.Remove(pos - 1, 1);
        tb.CaretIndex = pos - 1;
    }
}
