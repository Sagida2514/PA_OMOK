using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    private static GameUI instance;

    public static GameUI Instance
    {
        get { return instance; }
    }

    [SerializeField] private OmokBoard board;
    [SerializeField] private GameObject gameEndBG;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void OnStartButton()
    {
        board.gameObject.SetActive(true);
    }

    public void OnGameEnd(int team)
    {
        gameEndBG.SetActive(true);

        string teamText = team == 0 ? "흑" : "백";
        gameEndBG.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{teamText}팀 승리";
    }
}