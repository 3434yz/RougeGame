using System.Collections.Generic;
using UnityEngine;

public class Stage
{
    private Scene _scene;
    private readonly Player _player;
    private Vector2 _playerBornPosition;
    public const float WALL_WIDTH = .5f;


    public string mapStr = "BBBBBBBBBB\n" +
                           "B        B\n" +
                           "B        B\n" +
                           "B        B\n" +
                           "B        B\n" +
                           "B        B\n" +
                           "B    P   B\n" +
                           "B        B\n" +
                           "B    B   B\n" +
                           "BBBBBBBBBB";

    public SpaceIndexFloat<Block> blocks;

    public Stage(Scene scene)
    {
        _scene = scene;
        _player = scene._player;
        blocks = new SpaceIndexFloat<Block>(false);
        blocks.Init(null, Scene.numRows, Scene.numCols, Scene.cellSize);
        GenerateMap();
    }

    public void Update()
    {
        _player.Update();
    }

    public void Draw()
    {
        _player.Draw();
    }

    public void GenerateMap()
    {
        // 计算地图尺寸（行数和列数）
        var rows = mapStr.Split('\n');
        var numRows = rows.Length;
        var numCols = rows[0].Trim().Length; // 假设所有行长度相同

        var startPosition = new Vector2(
            0f, // X坐标保持0（最左侧）
            Scene.cellSize + (numRows - 1) * Scene.cellSize // Y坐标从0.64开始，向上扩展整个地图高度
        );

        // 从最大行索引递减到0（从上到下）
        for (var y = 0; y < numRows; y++)
        {
            var row = rows[y].Trim();

            // 从左到右遍历每一列
            for (var x = 0; x < row.Length; x++)
            {
                var tileType = row[x];

                // 计算世界坐标（考虑预制体轴心在左上角）
                var position = new Vector2(
                    startPosition.x + x * Scene.cellSize,
                    startPosition.y - y * Scene.cellSize // Y坐标随y递减而向下移动
                );

                // 根据字符类型生成对应的游戏对象
                switch (tileType)
                {
                    case 'B': // 砖块
                        var go = Object.Instantiate(_scene.blookPrefab, position, Quaternion.identity);
                        var block = new Block(go);
                        blocks.Add(block, new XYi(x, y));
                        break;

                    case 'P': // 玩家
                        _playerBornPosition = position;
                        break;
                }
            }
        }

        foreach (var block in blocks.Items)
            block.FillWayOut(blocks);

        _player.OnEnterStage(this, _playerBornPosition);
    }
}