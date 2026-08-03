using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DesignerCategory("Code")]
public sealed class DarkDataGridView : DataGridView, IThemeAwareControl
{
    private bool _useComfortableRows;

    public DarkDataGridView()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer,
            true);
        DoubleBuffered = true;
        AccessibleRole = AccessibleRole.Table;
        ApplyTheme();
    }

    [DefaultValue(false)]
    public bool UseComfortableRows
    {
        get => _useComfortableRows;
        set
        {
            if (_useComfortableRows == value)
            {
                return;
            }

            _useComfortableRows = value;
            RowTemplate.Height = value
                ? UiDimensions.ComfortableGridRowHeight
                : UiDimensions.GridRowHeight;

            foreach (DataGridViewRow row in Rows)
            {
                row.Height = RowTemplate.Height;
            }

            Invalidate();
        }
    }

    public void ApplyTheme()
    {
        ControlStyler.StyleDataGridView(this);
        RowTemplate.Height = UseComfortableRows
            ? UiDimensions.ComfortableGridRowHeight
            : UiDimensions.GridRowHeight;
        DefaultCellStyle.NullValue = "Not set";
    }

    protected override void OnCellPainting(
        DataGridViewCellPaintingEventArgs e)
    {
        base.OnCellPainting(e);

        if (!Focused
            || e.Graphics is null
            || CurrentCell is null
            || e.RowIndex < 0
            || e.ColumnIndex < 0
            || e.RowIndex != CurrentCell.RowIndex
            || e.ColumnIndex != CurrentCell.ColumnIndex)
        {
            return;
        }

        int focusWidth = DpiScaler.Scale(
            UiDimensions.FocusBorderWidth,
            DeviceDpi);
        Rectangle focusBounds = Rectangle.Inflate(
            e.CellBounds,
            -focusWidth,
            -focusWidth);
        using var focusPen = new Pen(
            ThemePalette.FocusBorder,
            focusWidth);
        e.Graphics.DrawRectangle(focusPen, focusBounds);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        InvalidateCurrentCell();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        InvalidateCurrentCell();
    }

    private void InvalidateCurrentCell()
    {
        if (CurrentCell is not null)
        {
            InvalidateCell(CurrentCell);
        }
    }
}
