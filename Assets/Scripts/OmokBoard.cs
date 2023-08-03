using UnityEngine;

public class OmokBoard : MonoBehaviour
{
    private const int boardSizeX = 19;
    private const int boardSizeY = 19;
    private const int stoneSpacing = 4;
    private int myTurn = 0;
    private int currentTurn = 0;
    private bool gameOver = false;
    private Camera currentCamera;

    private int[,] boardState = new int[boardSizeX, boardSizeY];
    [SerializeField] private GameObject[] posIndicator;

    private void Start()
    {
        currentCamera = Camera.main;

        for (int i = 0; i < boardSizeX; i++)
        {
            for (int j = 0; j < boardSizeY; j++)
            {
                boardState[i, j] = -1;
            }
        }
    }

    private void Update()
    {
        if (gameOver == true)
        {
            return;
        }

        if (currentTurn != myTurn)
        {
            if (posIndicator[myTurn].activeSelf == true)
            {
                posIndicator[myTurn].SetActive(false);
            }

            return;
        }

        ShowPosIndicator();
    }

    // 놓는게 가능한 자리인 경우 인디케이터 띄움
    private void ShowPosIndicator()
    {
        var mousePos = currentCamera.ScreenToWorldPoint(Input.mousePosition);

        // 배열에서의 돌의 좌표 구해오기
        var bPos = WorldPosToBoard(mousePos.x, mousePos.y);


        if (boardState[bPos.x, bPos.y] == -1)
        {
            posIndicator[myTurn].SetActive(true);
            var wPos = BoardPosToWorld(bPos.x, bPos.y);
            posIndicator[myTurn].transform.position = new Vector2(wPos.x, wPos.y
            );


            if (Input.GetMouseButtonDown(0) == true)
            {
                SpawnStone(myTurn, bPos.x, bPos.y);
            }
        }
        else
        {
            posIndicator[myTurn].SetActive(false);
        }
    }

    // 보드판의 해당 x,y 위치에 돌을 생성한다.
    public void SpawnStone(int team, int x, int y)
    {
        var wPos = BoardPosToWorld(x, y);

        var stone = Instantiate(Resources.Load<SpriteRenderer>("Prefabs/Stone"));
        stone.color = team == 0 ? Color.black : Color.white;
        stone.transform.position = new Vector2(wPos.x, wPos.y);
        boardState[x, y] = team;

        posIndicator[currentTurn].SetActive(false);

        if (CheckWin(team, x, y) == true)
        {
            gameOver = true;
            GameUI.Instance.OnGameEnd(team);
            return;
        }

        currentTurn = (currentTurn + 1) % 2;
        myTurn = currentTurn;
    }

    // 월드에서의 위치 -> 보드판에서의 위치
    private Vector2Int WorldPosToBoard(float x, float y)
    {
        int bx = Mathf.Clamp(((int)x + stoneSpacing * boardSizeX / 2) / stoneSpacing, 0, boardSizeX - 1);
        int by = Mathf.Clamp(((int)y + stoneSpacing * boardSizeY / 2) / stoneSpacing, 0, boardSizeY - 1);
        return new Vector2Int(bx, by);
    }

    // 보드판의 위치 -> 월드에서의 위치
    private Vector2Int BoardPosToWorld(int x, int y)
    {
        return new Vector2Int(x * stoneSpacing - boardSizeX / 2 * stoneSpacing,
            y * stoneSpacing - boardSizeY / 2 * stoneSpacing);
    }


    // 현재 위치에 돌을 놓았을 때 해당 team의 승리로 결정나는지 체크
    // **최적화를 위해 현재 돌의 근처에서만 가로, 세로, 대각선으로 체크하여 승리를 판단한다.


    public bool CheckWin(int team, int x, int y)
    {
        for (int i = 0; i < 4; i++)
        {
            if (CheckLine(i, team, x, y) == true)
            {
                return true;
            }
        }

        return false;
    }

    private int[] dx = { 1, 0, 1, 1 };
    private int[] dy = { 0, 1, -1, 1 };

    // dx,dy 배열 index의 방향으로 뻗어 나갔을 때 오목이 성립하는지 체크
    public bool CheckLine(int index, int team, int x, int y)
    {
        int cnt = 1;
        int dirX = dx[index];
        int dirY = dy[index];
        // 음수로 진행
        for (int i = -1; -5 < i; i--)
        {
            int nx = x + dirX * i;
            int ny = y + dirY * i;

            // 보드밖으로 나가는 경우 break
            if ((nx >= 0 && nx < boardSizeX && ny >= 0 && ny < boardSizeY) == false)
            {
                break;
            }

            // 같은 팀의 돌이면 카운트 +1
            if (boardState[x + dirX * i, y + dirY * i] == team)
            {
                cnt++;
            }
            else
            {
                break;
            }
        }

        for (int i = 1; i < 5; i++)
        {
            int nx = x + dirX * i;
            int ny = y + dirY * i;

            // 보드밖으로 나가는 경우 break
            if ((nx >= 0 && nx < boardSizeX && ny >= 0 && ny < boardSizeY) == false)
            {
                break;
            }

            // 같은 팀의 돌이면 카운트 +1
            if (boardState[x + dirX * i, y + dirY * i] == team)
            {
                cnt++;
            }
            else
            {
                break;
            }
        }

        // 6목이상은 허용안함. 정확히 5목인 경우에 return
        return cnt == 5;
    }
}