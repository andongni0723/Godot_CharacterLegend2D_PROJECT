using Godot;
using System;

public partial class WorldController : Node2D
{
    TileMap _tileMap => GetNode<TileMap>("TileMap");
    Camera2D _camera => GetNode<Camera2D>("Player/Camera2D");

    public override void _Ready()
    {
        Rect2I _used = _tileMap.GetUsedRect();
        Vector2 _tileSize = _tileMap.TileSet.TileSize;

        // Set camera limits
        _camera.LimitTop = (int)(_used.Position.Y * _tileSize.Y);
        _camera.LimitBottom = (int)(_used.End.Y * _tileSize.Y) * 10;
        _camera.LimitLeft = (int)(_used.Position.X * _tileSize.X) + 30;
        _camera.LimitRight = (int)(_used.End.X * _tileSize.X);
        _camera.ResetSmoothing();
        
        
    }
}
