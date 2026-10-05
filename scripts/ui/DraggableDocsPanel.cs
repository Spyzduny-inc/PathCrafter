using Godot;
using System;

public partial class DraggableDocsPanel : PanelContainer
{
    private bool _isDragging = false;
    private Vector2 _dragOffset;
    private const float TopBarHeight = 54f;

    public override void _Ready()
    {
        VisibilityChanged += OnVisibilityChanged;
    }

    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            // Невелика затримка для оновлення точних розмірів у кадрі
            CallDeferred(MethodName.ApplySavedOrDefaultPosition);
        }
    }

    public void ApplySavedOrDefaultPosition()
    {
        var viewportSize = GetViewportRect().Size;

        if (Global.SavedDocsPosition.HasValue)
        {
            Vector2 pos = Global.SavedDocsPosition.Value;
            pos.Y = Mathf.Clamp(pos.Y, TopBarHeight, Math.Max(TopBarHeight, viewportSize.Y - Size.Y - 10f));
            pos.X = Mathf.Clamp(pos.X, 10f, Math.Max(10f, viewportSize.X - Size.X - 10f));
            GlobalPosition = pos;
        }
        else
        {
            // Початкове позиціонування за замовчуванням: під TopBar
            float defaultX = (viewportSize.X - Size.X) / 2f;
            if (defaultX < 10f) defaultX = 20f;
            float defaultY = TopBarHeight + 10f;
            GlobalPosition = new Vector2(defaultX, defaultY);
            Global.SavedDocsPosition = GlobalPosition;
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                _isDragging = true;
                _dragOffset = mb.GlobalPosition - GlobalPosition;
            }
            else
            {
                if (_isDragging)
                {
                    _isDragging = false;
                    Global.SavedDocsPosition = GlobalPosition;
                }
            }
        }
        else if (@event is InputEventMouseMotion mm && _isDragging)
        {
            var viewportSize = GetViewportRect().Size;
            Vector2 newPos = mm.GlobalPosition - _dragOffset;

            // Обмежуємо Y так, щоб панель ніколи не перекривала верхній TopBar (minY = 54px)
            newPos.Y = Mathf.Clamp(newPos.Y, TopBarHeight, Math.Max(TopBarHeight, viewportSize.Y - Size.Y - 10f));
            newPos.X = Mathf.Clamp(newPos.X, 10f, Math.Max(10f, viewportSize.X - Size.X - 10f));

            GlobalPosition = newPos;
            Global.SavedDocsPosition = newPos;
        }
    }
}
