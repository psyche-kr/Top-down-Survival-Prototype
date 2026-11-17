using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 게임 전체 입력을 Input System 액션으로 모아 두는 곳.
/// 키 바인딩은 여기서만 정의하고, 다른 스크립트는 액션만 읽는다.
/// (Netcode 도입 후에는 로컬 소유자(IsOwner)의 스크립트만 이 입력을 읽도록 한다)
/// </summary>
public static class GameInput
{
    public static InputActionMap Gameplay { get; private set; }
    public static InputActionMap UI { get; private set; }

    // Gameplay
    public static InputAction Move { get; private set; }               // 우클릭 이동 / 상호작용
    public static InputAction Stop { get; private set; }               // S 정지
    public static InputAction Attack { get; private set; }             // A 공격 (예정)
    public static InputAction CameraFollowHold { get; private set; }   // Space 누르는 동안 카메라 고정
    public static InputAction CameraFollowToggle { get; private set; } // Y 카메라 고정 토글

    // UI
    public static InputAction Point { get; private set; }              // 마우스 위치
    public static InputAction ToggleInventory { get; private set; }    // Tab 인벤토리
    public static InputAction Cancel { get; private set; }             // Esc 닫기

    /// <summary>현재 마우스(포인터) 화면 좌표</summary>
    public static Vector2 PointerPosition => Point.ReadValue<Vector2>();

    // Domain Reload를 꺼도 플레이할 때마다 새로 만들도록 씬 로드 전에 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        Gameplay?.Dispose();
        UI?.Dispose();

        Gameplay = new InputActionMap("Gameplay");
        Move = Gameplay.AddAction("Move", InputActionType.Button, "<Mouse>/rightButton");
        Stop = Gameplay.AddAction("Stop", InputActionType.Button, "<Keyboard>/s");
        Attack = Gameplay.AddAction("Attack", InputActionType.Button, "<Keyboard>/a");
        CameraFollowHold = Gameplay.AddAction("CameraFollowHold", InputActionType.Button, "<Keyboard>/space");
        CameraFollowToggle = Gameplay.AddAction("CameraFollowToggle", InputActionType.Button, "<Keyboard>/y");

        UI = new InputActionMap("UI");
        Point = UI.AddAction("Point", InputActionType.PassThrough, "<Pointer>/position");
        Point.expectedControlType = "Vector2";
        ToggleInventory = UI.AddAction("ToggleInventory", InputActionType.Button, "<Keyboard>/tab");
        Cancel = UI.AddAction("Cancel", InputActionType.Button, "<Keyboard>/escape");

        Gameplay.Enable();
        UI.Enable();
    }
}
