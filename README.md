# 概要
* [連結：2048 WebGL](https://juicefish.github.io/2048WebGL/index.html)
* [連結：2048 WebPlayer (IE 啟用ActiveX)](https://juicefish.github.io/2048WebPlayer/2048WebPlayer.html)
* [連結：YouTube影片](https://youtu.be/oN2cKORYqec)
* 展示用2048遊戲
* 製作時間約12hr

## 遊戲說明
* 使用鍵盤方向鍵/滑鼠拖曳來移動方塊
* Reset可重置盤面
* Cheat亮燈時可點擊數字方塊進行升階

## 程式碼
### MainController.cs
主控制器：主程式進入點，包含介面UI互動操作判斷

### BlockManager.cs
方塊管理器：數字方塊的主要管理器

### Block.cs
方塊：數字方塊本體

### UIEventListener.cs
介面事件觸發：方便操作使用介面事件
