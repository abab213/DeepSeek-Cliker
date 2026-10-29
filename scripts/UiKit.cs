using Godot;
using System;

public static class UiKit
{
    public static readonly Color BgTop = new("040912");
    public static readonly Color BgBottom = new("08111f");
    public static readonly Color Panel = new("0a1524");
    public static readonly Color PanelRaised = new("0d1a2c");
    public static readonly Color PanelDeep = new("07101c");
    public static readonly Color Line = new("1a3149");
    public static readonly Color LineSoft = new("14283d");
    public static readonly Color White = new("ffffff");
    public static readonly Color Text = new("eaf6ff");
    public static readonly Color Muted = new("7d94ad");
    public static readonly Color Faint = new("4d647c");
    public static readonly Color Cyan = new("46e3ff");
    public static readonly Color CyanDeep = new("159ec4");
    public static readonly Color Blue = new("4d6bfe");
    public static readonly Color Gold = new("ffd166");
    public static readonly Color GoldDeep = new("d79b37");
    public static readonly Color Green = new("58e6a6");
    public static readonly Color Red = new("ff5b6e");
    public static readonly Color Warm = new("ffb35c");

    public static readonly Font BodyFont;
    public static readonly Font BoldFont;
    public static readonly Font DisplayFont;

    static UiKit()
    {
        BodyFont = MakeFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Noto Sans CJK SC", "Segoe UI" }, 400);
        BoldFont = MakeFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Noto Sans CJK SC", "Segoe UI" }, 700);
        DisplayFont = MakeFont(new[] { "Bahnschrift", "Bahnschrift SemiBold", "Segoe UI" }, 600);
    }

    public static SystemFont MakeFont(string[] names, int weight)
    {
        return new SystemFont
        {
            FontNames = names,
            FontWeight = weight,
            MultichannelSignedDistanceField = true,
            AllowSystemFallback = true,
        };
    }

    public static Color Alpha(Color color, float alpha) => new(color.R, color.G, color.B, alpha);

    public static Label Label(string text, int size, Color color, Font? font = null)
    {
        var label = new Label
        {
            Text = text,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            VerticalAlignment = VerticalAlignment.Center,
        };
        label.AddThemeFontOverride("font", font ?? BodyFont);
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    public static Label Display(string text, int size, Color color)
    {
        return Label(text, size, color, DisplayFont);
    }

    public static Label Spaced(string text, int size, Color color, Font? font = null)
    {
        var spaced = string.Join(" ", text.ToCharArray());
        return Label(spaced, size, color, font ?? DisplayFont);
    }

    public static TextureRect Icon(string path, int size, Color? modulate = null)
    {
        var texture = GD.Load<Texture2D>(path);
        return new TextureRect
        {
            Texture = texture,
            CustomMinimumSize = new Vector2(size, size),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Modulate = modulate ?? Colors.White,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
    }

    public static StyleBoxFlat Box(
        Color background,
        Color border,
        int radius = 14,
        int borderWidth = 1,
        int shadowSize = 0,
        Color? shadowColor = null)
    {
        var box = new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthLeft = borderWidth,
            BorderWidthTop = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthBottom = borderWidth,
            CornerRadiusTopLeft = radius,
            CornerRadiusTopRight = radius,
            CornerRadiusBottomLeft = radius,
            CornerRadiusBottomRight = radius,
            AntiAliasing = true,
        };

        if (shadowSize > 0)
        {
            box.ShadowSize = shadowSize;
            box.ShadowColor = shadowColor ?? new Color(0, 0, 0, 0.38f);
            box.ShadowOffset = new Vector2(0, 6);
        }

        return box;
    }

    public static StyleBoxFlat PanelStyle(bool raised = false)
    {
        return Box(raised ? PanelRaised : Panel, Line, 16, 1, 12, new Color(0, 0, 0, 0.32f));
    }

    public static PanelContainer PanelBox(Control? child, StyleBoxFlat? style = null)
    {
        var panel = new PanelContainer { SizeFlagsVertical = Control.SizeFlags.Fill };
        panel.AddThemeStyleboxOverride("panel", style ?? PanelStyle());
        if (child != null)
            panel.AddChild(child);
        return panel;
    }

    public static HBoxContainer HBox(int separation = 0)
    {
        var box = new HBoxContainer();
        box.AddThemeConstantOverride("separation", separation);
        return box;
    }

    public static VBoxContainer VBox(int separation = 0)
    {
        var box = new VBoxContainer();
        box.AddThemeConstantOverride("separation", separation);
        return box;
    }

    public static MarginContainer Pad(Control child, int all)
    {
        return Pad(child, all, all, all, all);
    }

    public static MarginContainer Pad(Control child, int left, int top, int right, int bottom)
    {
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", left);
        margin.AddThemeConstantOverride("margin_top", top);
        margin.AddThemeConstantOverride("margin_right", right);
        margin.AddThemeConstantOverride("margin_bottom", bottom);
        margin.AddChild(child);
        return margin;
    }

    public static Control VSpace(int height)
    {
        return new Control { CustomMinimumSize = new Vector2(0, height), MouseFilter = Control.MouseFilterEnum.Ignore };
    }

    public static Control HSpace(int width, bool expand = false)
    {
        var space = new Control { CustomMinimumSize = new Vector2(width, 0), MouseFilter = Control.MouseFilterEnum.Ignore };
        if (expand)
        {
            space.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            space.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        }
        return space;
    }

    public static ColorRect HLine(Color color, int height = 1)
    {
        return new ColorRect
        {
            Color = color,
            CustomMinimumSize = new Vector2(0, height),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
    }

    public static ColorRect VLine(Color color, int width = 1)
    {
        return new ColorRect
        {
            Color = color,
            CustomMinimumSize = new Vector2(width, 0),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
    }

    public static TextureRect VerticalGradient(Color top, Color bottom, float height = 0)
    {
        var gradient = new Gradient();
        gradient.SetColors(new[] { top, bottom });
        gradient.SetOffsets(new[] { 0f, 1f });

        var texture = new GradientTexture2D
        {
            Gradient = gradient,
            Width = 8,
            Height = 256,
            FillFrom = new Vector2(0.5f, 0f),
            FillTo = new Vector2(0.5f, 1f),
        };

        var rect = new TextureRect
        {
            Texture = texture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        if (height > 0)
            rect.CustomMinimumSize = new Vector2(0, height);
        return rect;
    }

    public static TextureRect HorizontalGradient(Color left, Color right, float width = 0)
    {
        var gradient = new Gradient();
        gradient.SetColors(new[] { left, right });
        gradient.SetOffsets(new[] { 0f, 1f });

        var texture = new GradientTexture2D
        {
            Gradient = gradient,
            Width = 256,
            Height = 8,
            FillFrom = new Vector2(0f, 0.5f),
            FillTo = new Vector2(1f, 0.5f),
        };

        var rect = new TextureRect
        {
            Texture = texture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        if (width > 0)
            rect.CustomMinimumSize = new Vector2(width, 0);
        return rect;
    }

    public static StyleBoxFlat ButtonStyle(Color background, Color border, int radius = 8)
    {
        var style = Box(background, border, radius, 1, 0);
        style.ContentMarginLeft = 12;
        style.ContentMarginRight = 12;
        style.ContentMarginTop = 6;
        style.ContentMarginBottom = 6;
        return style;
    }

    public static Button Button(string text, Color background, Color border, Color textColor, int fontSize = 12)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 30),
            FocusMode = Control.FocusModeEnum.None,
            MouseDefaultCursorShape = Control.CursorShape.PointingHand,
        };
        var normal = ButtonStyle(background, border);
        var hover = ButtonStyle(background.Lerp(Colors.White, 0.08f), border.Lerp(Colors.White, 0.2f));
        var pressed = ButtonStyle(background.Lerp(Colors.Black, 0.2f), border);
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeStyleboxOverride("disabled", ButtonStyle(background.Lerp(Colors.Black, 0.25f), LineSoft));
        button.AddThemeFontOverride("font", BoldFont);
        button.AddThemeFontSizeOverride("font_size", fontSize);
        button.AddThemeColorOverride("font_color", textColor);
        button.AddThemeColorOverride("font_hover_color", textColor);
        button.AddThemeColorOverride("font_pressed_color", textColor);
        button.AddThemeColorOverride("font_disabled_color", Faint);
        return button;
    }

    public static PanelContainer Tag(string text, Color color, int fontSize = 9)
    {
        var label = Label(text, fontSize, color, BoldFont);
        var panel = new PanelContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
        };
        var style = Box(Alpha(color, 0.12f), Alpha(color, 0.4f), 6, 1);
        style.ContentMarginLeft = 7;
        style.ContentMarginRight = 7;
        style.ContentMarginTop = 2;
        style.ContentMarginBottom = 2;
        panel.AddThemeStyleboxOverride("panel", style);
        panel.AddChild(label);
        return panel;
    }

    public static void Fill(Control control)
    {
        control.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
    }
}
