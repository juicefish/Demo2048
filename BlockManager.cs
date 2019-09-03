using UnityEngine;
using System;
using System.Collections;


// 移動方向(朝向)
public enum MoveDirection
{
    Stop,
    Left,
    Right,
    Up,
    Down,
    Collapse,
}

// Block管理器
public class BlockManager : MonoBehaviour
{
    // 變數控制
    public float BlockBornSpeed = 1.0f;
    public float BlockMovingSpeed = 1.0f;
    public int BlockStartNumber = 2;
    public Color[] BlockColorTable;
    public int ClearScore = 2048;

    // 當所有方塊移動完畢時, 觸發這個訊息
    public Action OnActionFinish;

    // Singleton, 使Manager唯一
    private static BlockManager _instance = null;
    public static BlockManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(BlockManager)) as BlockManager;

                if (_instance == null)
                {
                    GameObject go = new GameObject("BlockManager");
                    _instance = go.AddComponent<BlockManager>();
                }
            }

            return _instance;
        }
    }

    // Block的GameObject擺放Panel位置
    public GameObject goBlockPanel;

    // Block的GameObject樣本模板
    public GameObject goBlockSample;

    // 管理用變數
    private int col;
    private int row;
    private Block[,] blockArray;

    public int currentCol { get { return this.col; } }
    public int currentRow { get { return this.row; } }

    // 取得方塊的最高號碼
    public int currentTopScore 
    { 
        get
        {
            ArrayList checkList =  this.GetAvilableBlocks();
            checkList.Sort(new CurrentTopSort());

            if (checkList.Count > 0)
                return (checkList[0] as Block).number;
            return 0;
        } 
    }

    public class CurrentTopSort : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            return (y as Block).number - (x as Block).number;
        }

    }

    // 取得分數是否通關
    public bool isClear
    {
        get
        {
            return this.currentTopScore >= this.ClearScore;
        }
    }

    // 取得是否遊戲結束
    public bool isGameOver
    {
        get
        {
            // 當場面上都塞滿方塊 且 所有方向都不能移動
            // 則宣告為遊戲結束
            return (!this.hasEmpty &&
                    !this.DetermindMoveable(MoveDirection.Left) &&
                    !this.DetermindMoveable(MoveDirection.Up) &&
                    !this.DetermindMoveable(MoveDirection.Right) &&
                    !this.DetermindMoveable(MoveDirection.Down));
        }
    }

    // 依照數字取得對應顏色表的色彩
    public Color GetColor(int score)
    {
        try
        {
            // 依照數字對應顏色
            return this.BlockColorTable[(int)Mathf.Log(score, 2)];
        }
        catch (Exception e)
        {
            // 取得失敗則弄回白色
            return Color.white;
        }
    }

    // 取得Block
    public Block GetBlock(int x, int y)
    {
        if (0 <= x && x < this.col && 0 <= y && y < this.row)
            return this.blockArray[x, y];
        return null;
    }

    // 設定Block
    public void SetBlock(int x, int y, Block block)
    {
        if (0 <= x && x < this.col && 0 <= y && y < this.row)
             this.blockArray[x, y] = block;
    }

    // 取得現存可用方塊列表
    public ArrayList GetAvilableBlocks()
    {   
        ArrayList checkBlocks = new ArrayList();

        if (this.blockArray == null)
            return checkBlocks;

        for (int x = 0; x < this.col; x++)
            for (int y = 0; y < this.row; y++)
            {
                if (blockArray[x, y] != null)
                    checkBlocks.Add(blockArray[x, y]);
            }
        return checkBlocks;
    }

    // 檢查是否可移動
    public bool DetermindMoveable(MoveDirection direction)
    {
        // 用移動方向決定測試格偏移值
        Vector2 checkDelta = Vector2.zero;
        switch(direction)
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

        // 對所有已存在資料的格子進行移動測試
        for (int x = 0; x < this.col; x++)
            for (int y = 0; y < this.row; y++)
                if (blockArray[x, y] != null)
                {
                    // 當檢查目標格在正常界限內
                    if( 0 <= x + checkDelta.x && x + checkDelta.x < this.col &&
                        0 <= y + checkDelta.y && y + checkDelta.y < this.row)
                    {
                        // 檢查方位格是空位, 可以移動(至少一格)
                        if (blockArray[x + (int)checkDelta.x, y + (int)checkDelta.y] == null)
                            return true;
                        // 檢查方位格不是是空位但可以合併, 可以移動
                        else if (blockArray[x + (int)checkDelta.x, y + (int)checkDelta.y].number == blockArray[x, y].number)
                            return true;
                    }
                }

        // 全部的格子檢查完畢都沒辦法正確對應, 則為該方向不可移動
        return false;
    }

    // 檢查是否有空位
    public bool hasEmpty
    {
        get
        {
            for (int x = 0; x < this.col; x++)
                for (int y = 0; y < this.row; y++)
                    if (blockArray[x, y] == null)
                        return true;
            return false;
        }
    }

    // 初始化
    public void Init(int col, int row)
    {
        // 清除目前場上方塊
        this.DestoryBlocks();

        // 進行創新方塊
        blockArray = new Block[col, row];
        this.col = col;
        this.row = row;
        this.InitNewBlock();

        // 通知更新狀態
        this.UpdateStatus();
    }

    // 創新方塊
    private void InitNewBlock(MoveDirection direction = MoveDirection.Stop)
    {
        // 如果沒空位就不創
        if (!this.hasEmpty)
            return;

        //Vector2 initPosition = new Vector2(this.col / 2, this.row / 2);
        Vector2 initPosition = this.GetEmptyDimension();
        GameObject goNewBlock = Instantiate<GameObject>(goBlockSample);
        Block newblok = goNewBlock.GetComponent<Block>();

        // 設定初始數字
        newblok.number = this.BlockStartNumber;

        // 註冊方塊位置
        this.SetBlock((int)initPosition.x, (int)initPosition.y, newblok);

        // 方塊繪圖層級移動
        newblok.transform.SetParent(goBlockPanel.transform);

        // 初始化方塊座標
        newblok.InitDimension((int)initPosition.x, (int)initPosition.y);
        goNewBlock.SetActive(true);
    }

    // 取得空位座標
    private Vector2 GetEmptyDimension()
    {
        if (!this.hasEmpty)
            return new Vector2(-1, -1);

        ArrayList emptyList = new ArrayList();
        for (int x = 0; x < this.col; x++)
            for (int y = 0; y < this.row; y++)
                if (blockArray[x, y] == null)
                    emptyList.Add(new Vector2(x, y));

        return (Vector2)emptyList[Mathf.FloorToInt(UnityEngine.Random.value * emptyList.Count)];
    }
    
    // 往指定方向移動方塊
    public void MoveBlocks(MoveDirection direction)
    {
        // 先確定是否能至少移動/合併一格
        if (!this.DetermindMoveable(direction))
            return;

        // 取得現存方塊列表
        ArrayList checkBlocks = this.GetAvilableBlocks();

        // 確立檢查方向
        // 上右為正向, 下左為逆向
        switch (direction)
        {
            case MoveDirection.Left:
            case MoveDirection.Down:
                break;

            case MoveDirection.Right:
            case MoveDirection.Up:
                checkBlocks.Reverse();
                break;
        }

        // 依序停止所有動畫(快進)
        for (int index = 0; index < checkBlocks.Count; index++)
        {
            Block currentBlock = (Block)checkBlocks[index];
            currentBlock.ForceMoving();
        }

        // 依序移動所有現存Block
        for (int index = 0; index < checkBlocks.Count; index++)
        {
            Block currentBlock = (Block)checkBlocks[index];
            currentBlock.MoveBlock(direction);
        }

        // 移動完之後試著新增方塊
        this.InitNewBlock(direction);
    }

    public void UpdateStatus()
    {
        // 取得現存方塊列表
        ArrayList checkBlocks = this.GetAvilableBlocks();

        if(checkBlocks.Count == 0)
        {
            this.OnActionFinish();
            return;
        }

        for (int index = 0; index < checkBlocks.Count; index++)
            if ((checkBlocks[index] as Block).isMoving || (checkBlocks[index] as Block).isUpgrade)
                return;

        this.OnActionFinish();
    }

    // 破壞所有方塊
    private void DestoryBlocks()
    {
        if (this.blockArray == null)
            return;

        // 取得現存方塊列表
        ArrayList checkBlocks = this.GetAvilableBlocks();

        // 依序停止所有動畫(快進), 並進行清算
        for (int index = 0; index < checkBlocks.Count; index++)
        {
            Block currentBlock = (Block)checkBlocks[index];
            currentBlock.ForceMoving();
            GameObject.Destroy(currentBlock.gameObject);
        }
    }
}
