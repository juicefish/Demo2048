using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class MainController : MonoBehaviour
{
    public Text textScore;
    public Text textGameOver;
    public GameObject goSlidePanel;
    public GameObject goBtnCheat;
    public GameObject goBtnReset;

    public float panelDragLeast = 10.0f;
    private Vector2 panelMouseDownPosition;
    private bool isCheat = true;

    void Start()
    {
        // 註冊方塊更新事件
        BlockManager.instance.OnActionFinish += this.OnBlockActionFinish;

        // 註冊滑鼠/螢幕操作事件
        goSlidePanel.GetUIEventListener().OnMouseDown += this.OnPanelMouseDown;
        goSlidePanel.GetUIEventListener().OnMouseUp += this.OnPanelMouseUp;
        goBtnCheat.GetUIEventListener().OnClick += this.OnCheatClick;
        goBtnReset.GetUIEventListener().OnClick += this.OnResetClick;

        this.OnCheatClick();

        // 起始弄成4*4的格子
        BlockManager.instance.Init(4, 4);
    }

    // Frame更新時
    void Update()
    {
        // 按鍵偵測
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            BlockManager.instance.MoveBlocks(MoveDirection.Left);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            BlockManager.instance.MoveBlocks(MoveDirection.Right);
        if (Input.GetKeyDown(KeyCode.UpArrow))
            BlockManager.instance.MoveBlocks(MoveDirection.Up);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            BlockManager.instance.MoveBlocks(MoveDirection.Down);
    }

    // 方塊更新事件
    private void OnBlockActionFinish()
    {
        // 更新分數
        this.textScore.text = BlockManager.instance.currentTopScore.ToString();

        // 依照數字對應顏色
        this.textScore.color = BlockManager.instance.GetColor(BlockManager.instance.currentTopScore);

        // 更新GameOver狀態
        if (BlockManager.instance.isClear)
        {
            this.textGameOver.text = "Clear";
            this.textGameOver.gameObject.SetActive(true);
        }
        else if (BlockManager.instance.isGameOver)
        {
            this.textGameOver.text = "Game Over";
            this.textGameOver.gameObject.SetActive(true);
        }
        else
            this.textGameOver.gameObject.SetActive(false);
    }

    // 點擊按下面板(螢幕)時
    private void OnPanelMouseDown(GameObject gameObject, Vector2 position)
    {
        // 紀錄點擊座標
        this.panelMouseDownPosition = position;
    }

    // 點擊放開面板(螢幕)時
    private void OnPanelMouseUp(GameObject gameObject, Vector2 position)
    {
        Vector2 delta = position - this.panelMouseDownPosition;
        bool useX = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);

        if (useX)
        {
            if (delta.x <= -this.panelDragLeast) BlockManager.instance.MoveBlocks(MoveDirection.Left);
            else if (delta.x >= this.panelDragLeast) BlockManager.instance.MoveBlocks(MoveDirection.Right);
        }
        else
        {
            if (delta.y <= -this.panelDragLeast) BlockManager.instance.MoveBlocks(MoveDirection.Down);
            else if (delta.y >= this.panelDragLeast) BlockManager.instance.MoveBlocks(MoveDirection.Up);
        }
    }

    // 點擊Cheat時
    private void OnCheatClick(GameObject gameObject = null, Vector2 position = default(Vector2))
    {
        this.isCheat = !this.isCheat;
        this.goBtnCheat.GetComponent<Image>().color = this.isCheat?  Color.yellow : Color.gray;

        // 條動層級使偵測區會不會穿越, 以點擊號碼按鈕
        this.goSlidePanel.GetComponent<Canvas>().sortingOrder = this.isCheat? -1 : 1;
    }

    // 點擊Reset時
    private void OnResetClick(GameObject gameObject, Vector2 position)
    {
        // Reset就是重新Init
        BlockManager.instance.Init(4, 4);
    }
}
