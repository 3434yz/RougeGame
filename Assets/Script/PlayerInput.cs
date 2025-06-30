using UnityEngine;

partial class Player
{
    private InputActions.PlayerActions _iapa;
    private bool _kbMovingUp; // 键盘 W/UP
    private bool _kbMovingDown; // 键盘 S/Down
    private bool _kbMovingLeft; // 键盘 A/Left
    private bool _kbMovingRight; // 键盘 D/Right

    private bool _usingKeyboard; // false: 手柄
    public bool _jumping { get; private set; } // 键盘 Space 或 手柄按钮 A / X
    public bool _moving { get; private set; } // 是否正在移动( 键盘 ASDW 或 手柄左 joy 均能触发 )
    public Vector2 playerMoveValue; // 归一化之后的移动方向( 读前先判断 playerMoving )
    private Vector2 playerLastMoveValue = new(1, 0); // 上一个非 0 移动值的备份( 当前值如果为 0, 该值可供参考 )

    public Vector2 Direction
    {
        // 获取玩家朝向
        get
        {
            if (_moving)
            {
                return playerMoveValue;
            }
            else
            {
                return playerLastMoveValue;
            }
        }
    }

    public void InitInputAction()
    {
        var ia = new InputActions();
        _iapa = ia.Player;
        _iapa.Enable();

        // keyboard
        _iapa.KBJump.started += c =>
        {
            _usingKeyboard = true;
            _jumping = true;
        };
        _iapa.KBJump.canceled += c =>
        {
            _usingKeyboard = true;
            _jumping = false;
        };

        _iapa.KBMoveUp.started += c =>
        {
            _usingKeyboard = true;
            _kbMovingUp = true;
        };
        _iapa.KBMoveUp.canceled += c =>
        {
            _usingKeyboard = true;
            _kbMovingUp = false;
        };

        _iapa.KBMoveDown.started += c =>
        {
            _usingKeyboard = true;
            _kbMovingDown = true;
        };
        _iapa.KBMoveDown.canceled += c =>
        {
            _usingKeyboard = true;
            _kbMovingDown = false;
        };

        _iapa.KBMoveLeft.started += c =>
        {
            _usingKeyboard = true;
            _kbMovingLeft = true;
        };
        _iapa.KBMoveLeft.canceled += c =>
        {
            _usingKeyboard = true;
            _kbMovingLeft = false;
        };

        _iapa.KBMoveRight.started += c =>
        {
            _usingKeyboard = true;
            _kbMovingRight = true;
        };
        _iapa.KBMoveRight.canceled += c =>
        {
            _usingKeyboard = true;
            _kbMovingRight = false;
        };
        // gamepad
        _iapa.GPJump.started += c =>
        {
            _usingKeyboard = false;
            _jumping = true;
        };
        _iapa.GPJump.canceled += c =>
        {
            _usingKeyboard = false;
            _jumping = false;
        };

        _iapa.GPMove.started += c =>
        {
            _usingKeyboard = false;
            _moving = true;
        };
        _iapa.GPMove.performed += c =>
        {
            _usingKeyboard = false;
            _moving = true;
        };
        _iapa.GPMove.canceled += c =>
        {
            _usingKeyboard = false;
            _moving = false;
        };
    }

    public void HandlePlayerInput()
    {
        if (_usingKeyboard)
        {
            // 键盘需要每帧判断, 合并方向, 计算最终矢量
            if (!_kbMovingUp && !_kbMovingDown && !_kbMovingLeft && !_kbMovingRight
                || _kbMovingUp && _kbMovingDown && _kbMovingLeft && _kbMovingRight)
            {
                playerMoveValue.x = 0f;
                playerMoveValue.y = 0f;
                _moving = false;
            }
            else if (!_kbMovingUp && _kbMovingDown && _kbMovingLeft && _kbMovingRight)
            {
                playerMoveValue.x = 0f;
                playerMoveValue.y = 1f;
                _moving = true;
            }
            else if (_kbMovingUp && !_kbMovingDown && _kbMovingLeft && _kbMovingRight)
            {
                playerMoveValue.x = 0f;
                playerMoveValue.y = -1f;
                _moving = true;
            }
            else if (_kbMovingUp && _kbMovingDown && !_kbMovingLeft && _kbMovingRight)
            {
                playerMoveValue.x = 1f;
                playerMoveValue.y = 0f;
                _moving = true;
            }
            else if (_kbMovingUp && _kbMovingDown && _kbMovingLeft && !_kbMovingRight)
            {
                playerMoveValue.x = -1f;
                playerMoveValue.y = 0f;
                _moving = true;
            }
            else if (_kbMovingUp && _kbMovingDown
                     || _kbMovingLeft && _kbMovingRight)
            {
                playerMoveValue.x = 0f;
                playerMoveValue.y = 0f;
                _moving = false;
            }
            else if (_kbMovingUp && _kbMovingLeft)
            {
                playerMoveValue.x = -Scene.Sqrt21;
                playerMoveValue.y = -Scene.Sqrt21;
                _moving = true;
            }
            else if (_kbMovingUp && _kbMovingRight)
            {
                playerMoveValue.x = Scene.Sqrt21;
                playerMoveValue.y = -Scene.Sqrt21;
                _moving = true;
            }
            else if (_kbMovingDown && _kbMovingLeft)
            {
                playerMoveValue.x = -Scene.Sqrt21;
                playerMoveValue.y = Scene.Sqrt21;
                _moving = true;
            }
            else if (_kbMovingDown && _kbMovingRight)
            {
                playerMoveValue.x = Scene.Sqrt21;
                playerMoveValue.y = Scene.Sqrt21;
                _moving = true;
            }
            else if (_kbMovingUp)
            {
                playerMoveValue.x = 0;
                playerMoveValue.y = -1;
            }
            else if (_kbMovingDown)
            {
                playerMoveValue.x = 0;
                playerMoveValue.y = 1;
            }
            else if (_kbMovingLeft)
            {
                playerMoveValue.x = -1;
                playerMoveValue.y = 0;
                _moving = true;
            }
            else if (_kbMovingRight)
            {
                playerMoveValue.x = 1;
                playerMoveValue.y = 0;
                _moving = true;
            }
            //if (playerMoving) {
            //    Debug.Log(playerKBMovingUp + " " + playerKBMovingDown + " " + playerKBMovingLeft + " " + playerKBMovingRight + " " + playerMoveValue);
            //}
        }
        else
        {
            // 手柄不需要判断
            var v = _iapa.GPMove.ReadValue<Vector2>();
            //v.Normalize();
            playerMoveValue.x = v.x;
            playerMoveValue.y = -v.y;
            // todo: playerMoving = 距离 > 死区长度 ?
        }

        if (_moving)
        {
            playerLastMoveValue = playerMoveValue;
        }
    }

    public bool InvokerDash()
    {
        return _iapa.KBDash.WasPressedThisFrame();
    }

    public bool InvokerAttack()
    {
        return _iapa.Attack.WasPressedThisFrame();
    }
}