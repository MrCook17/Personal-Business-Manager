using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DefaultEvent(nameof(TextChanged))]
[DefaultProperty(nameof(Text))]
[DesignerCategory("Code")]
public sealed class DarkTextBox : DarkInputBase
{
    private readonly TextBox _textBox;

    public DarkTextBox()
        : base(new TextBox())
    {
        _textBox = (TextBox)Editor;
        _textBox.BorderStyle = BorderStyle.None;
        _textBox.TextChanged += (_, _) =>
            OnTextChanged(EventArgs.Empty);

        ApplyTheme();
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    [AllowNull]
    public override string Text
    {
        get => _textBox.Text;
        set
        {
            value ??= string.Empty;

            if (!string.Equals(
                    _textBox.Text,
                    value,
                    StringComparison.Ordinal))
            {
                _textBox.Text = value;
            }
        }
    }

    [DefaultValue("")]
    public string PlaceholderText
    {
        get => _textBox.PlaceholderText;
        set => _textBox.PlaceholderText = value ?? string.Empty;
    }

    [DefaultValue(false)]
    public bool ReadOnly
    {
        get => _textBox.ReadOnly;
        set
        {
            if (_textBox.ReadOnly == value)
            {
                return;
            }

            _textBox.ReadOnly = value;
            ApplyTheme();
        }
    }

    [DefaultValue(false)]
    public bool Multiline
    {
        get => _textBox.Multiline;
        set
        {
            if (_textBox.Multiline == value)
            {
                return;
            }

            _textBox.Multiline = value;
            MinimumSize = new Size(
                0,
                value
                    ? UiDimensions.MultilineInputMinimumHeight
                    : UiDimensions.StandardControlHeight);
            Height = Math.Max(Height, MinimumSize.Height);
            PerformLayout();
        }
    }

    [DefaultValue(false)]
    public bool UseSystemPasswordChar
    {
        get => _textBox.UseSystemPasswordChar;
        set => _textBox.UseSystemPasswordChar = value;
    }

    [DefaultValue(32767)]
    public int MaxLength
    {
        get => _textBox.MaxLength;
        set => _textBox.MaxLength = value;
    }

    [DefaultValue(HorizontalAlignment.Left)]
    public HorizontalAlignment TextAlign
    {
        get => _textBox.TextAlign;
        set => _textBox.TextAlign = value;
    }

    [DefaultValue(ScrollBars.None)]
    public ScrollBars ScrollBars
    {
        get => _textBox.ScrollBars;
        set => _textBox.ScrollBars = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public TextBox EditorTextBox => _textBox;

    protected override bool IsReadOnly => ReadOnly;

    public void SelectAll()
    {
        _textBox.SelectAll();
    }

    protected override void LayoutEditor(Rectangle contentBounds)
    {
        var textBox = (TextBox)Editor;

        if (textBox.Multiline)
        {
            textBox.Bounds = contentBounds;
            return;
        }

        int top = Math.Max(
            contentBounds.Top,
            contentBounds.Top
                + ((contentBounds.Height - textBox.PreferredHeight) / 2));
        textBox.SetBounds(
            contentBounds.Left,
            top,
            contentBounds.Width,
            textBox.PreferredHeight);
    }
}
