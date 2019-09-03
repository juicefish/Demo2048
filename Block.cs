using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

// 數字方塊
public class Block : MonoBehaviour 
{
    // 數字的Text
    public Text uiNumberText;

    // 底圖的Image
    public Image uiBackImage;

    void Start()
    {
        // 註冊點擊事件
        this.gameObject.AddComponent<UIEventListener>().OnClick += this.OnClick;
    }

    // 升級用Block連結
    private Block upgradeMasterBlock = null;
    private Block upgradeChildBlock = null;
    
    // 目前的移動/升級狀態
    private MoveDirection movingDirection = MoveDirection.Stop;
    private int x;
    private int y;

    // 自己是否正在移動
    public bool isMoving { get { return this.movingDirection != MoveDirection.Stop; } }

    // 自己的數字
    public int number
    {
        get 
        {
            return int.Parse(this.uiNumberText.text); 
        }
        set
        {
            this.uiNumberText.text = value.ToString();
            this.uiBackImage.color = BlockManager.instance.GetColor(this.number);
        }
    }

    // 自己是否正在升級
    public bool isUpgrade { get { return this.upgradeChildBlock != null; } }

    // 初始化座標
    public void InitDimension(int newX, int newY)
    {
        this.x = newX;
        this.y = newY;
        this.transform.localScale = Vector3.zero;
        this.transform.localPosition = this.GetCorrectPixel();
    }

    // 朝指定方向移動方塊, 直到不能移動為止
    public void MoveBlock(MoveDirection direction)
    {
        // 用移動方向決定測試格偏移值
        Vector2 checkDelta = Vector2.zero;
        switch (direction)
        {
            case MoveDirection.Left:
                checkDelta = new Vector2(-1, 0);
                break;
            case MoveDirection.Right:
                checkDelta = new Vector2(1, 0);
                break;
            case MoveDirection.Up:
                checkDelta = new Vector2(0, 1);
                break;
            case MoveDirection.Down:
                checkDelta = new Vector2(0, -1);
                break;
        }

        // 持續從自己所在的位置往指定方向偏移偵測是否可移動/合併
        // 最遠的位置理論上會偵測多次
        for (Vector2 checker = new Vector2(this.x + checkDelta.x, this.y + checkDelta.y);
            0 <= checker.x && checker.x < BlockManager.instance.currentCol &&
            0 <= checker.y && checker.y < BlockManager.instance.currentRow
            ; checker = new Vector2(checker.x + checkDelta.x, checker.y + checkDelta.y))
        {
            // 取得自己網當前方向移動的格子
            Block checkBlock = BlockManager.instance.GetBlock((int)checker.x, (int)checker.y);

            // 目標為空, 則進行移動
            if (checkBlock == null)
            {
                // 將自己從面板上位移, 並標記自己成移動狀態
                BlockManager.instance.SetBlock(this.x, this.y, null);
                this.movingDirection = direction;
                this.x = (int)checker.x;
                this.y = (int)checker.y;
                BlockManager.instance.SetBlock((int)checker.x, (int)checker.y, this);
            }
            // 目標不為空, 若數字相同且目標不處於升級狀態
            else if (checkBlock != null && checkBlock.number == this.number && !checkBlock.isUpgrade)
            {
                // 將自己從面板上移除, 防止重複偵測
                // 並與目標互相標記連結
                BlockManager.instance.SetBlock(this.x, this.y, null);
                this.movingDirection = MoveDirection.Collapse;
                this.x = (int)checker.x;
                this.y = (int)checker.y;
                this.upgradeMasterBlock = checkBlock;
                checkBlock.SetUpgradeLink(this);
                return;
            }
            // 目標不為空, 且因上述判斷無法合併, 則當作牆壁停止後續偵測
            else if (checkBlock != null)
            {
                return;
            }
        }
    }
    
    // 坐標系轉換:從邏輯座標轉為螢幕座標
    private int ConvertDimensionInPixel(int dimension, int dimensionMax)
    {
        return (dimension - dimensionMax / 2) * 100 + 50;
    }

    // 整體座標轉換
    private Vector3 GetCorrectPixel()
    {
        return new Vector3( this.ConvertDimensionInPixel(this.x, BlockManager.instance.currentCol),
                            this.ConvertDimensionInPixel(this.y, BlockManager.instance.currentRow),
                            1);
    }

    // 強制結束移動, 用以正確化動畫顯示
    public void ForceMoving()
    {
        if (this.movingDirection == MoveDirection.Collapse)
        {
            this.Collapsed();
            return;
        }

        if (this.upgradeChildBlock != null)
            this.upgradeChildBlock.ForceMoving();

        this.transform.localScale = Vector3.one;
        this.transform.localPosition = this.GetCorrectPixel();
        this.movingDirection = MoveDirection.Stop;

        // 通知更新狀態
        BlockManager.instance.UpdateStatus();
    }

    // 設定升級連結
    public void SetUpgradeLink(Block collapseBlock)
    {
        this.upgradeChildBlock = collapseBlock;
    }

    // 升級的實際動作
    public void Upgrade()
    {
        this.upgradeChildBlock = null;
        this.movingDirection = MoveDirection.Stop;
        this.number = this.number + this.number;

        // 通知更新狀態
        BlockManager.instance.UpdateStatus();
    }

    // 被收縮的實際動作
    private void Collapsed()
    {
        this.upgradeMasterBlock.Upgrade();
        GameObject.Destroy(this.gameObject);
    }

    // 每Frame刷新動作
	void FixedUpdate () 
    {
        // 出生動畫化相關
        if (this.transform.localScale != Vector3.one)
        {
            float currentScaleUp = BlockManager.instance.BlockBornSpeed * Time.fixedDeltaTime;
            if (this.transform.localScale.x + currentScaleUp >= 1.0f)
                this.transform.localScale = Vector3.one;
            else
                this.transform.localScale = this.transform.localScale + Vector3.one * currentScaleUp;
        }

        // 當正在移動才需要計算
        if (!this.isMoving)
            return;

        // 座標移動相關
        Vector3 targetLocalPosition = this.GetCorrectPixel();

        if (this.transform.localPosition != targetLocalPosition)
        {
            float currentMove = BlockManager.instance.BlockMovingSpeed * Time.fixedDeltaTime;
            if (Vector3.Distance(targetLocalPosition, transform.localPosition) < currentMove)
                transform.localPosition = targetLocalPosition;
            else
                transform.localPosition = transform.localPosition + (targetLocalPosition - transform.localPosition) * currentMove;
        }

        // 當座標移到指定位置後, 若自己是被折疊的, 則進行實際的折疊動作
        if (this.transform.localPosition == targetLocalPosition && this.movingDirection == MoveDirection.Collapse)
            this.Collapsed();
        // 當座標移到指定位置後, 無其他因素, 設定旗標為停止
        if (this.transform.localPosition == targetLocalPosition)
        {
            this.movingDirection = MoveDirection.Stop;

            // 通知更新狀態
            BlockManager.instance.UpdateStatus();
        }
	}

    // 點擊方塊(Cheat用)
    private void OnClick(GameObject gameObject, Vector2 position)
    {
        this.ForceMoving();

        this.number += this.number;
        BlockManager.instance.UpdateStatus();
    }
}
