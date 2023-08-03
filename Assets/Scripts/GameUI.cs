using System;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    private static GameUI instance;

    public static GameUI Instance
    {
        get { return instance; }
    }

    public Server server;
    public Client client;

    [Header("UI")] [SerializeField] private OmokBoard board;
    [SerializeField] private GameObject gameEndBG;
    [SerializeField] private Animator animator;
    [SerializeField] private TMP_InputField addressInput;

    
    public Action<bool> StartGame;
    
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
        
        RegisterEvents();
    }


    public void OnOnlineButton()
    {
        animator.SetTrigger("OnlineMenu");
    }

    public void OnHostButton()
    {
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
        animator.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        client.Init(addressInput.text, 8007);
    }

    public void OnOnlineBackButton()
    {
        animator.SetTrigger("MainMenu");
    }

    public void OnHostBackButton()
    {
        server.Shutdown();
        client.Shutdown();
        animator.SetTrigger("OnlineMenu");
    }


    public void OnGameEnd(int team)
    {
        gameEndBG.SetActive(true);

        string teamText = team == 0 ? "흑" : "백";
        gameEndBG.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{teamText}팀 승리";
    }
    
    
    private void RegisterEvents()
    {
        NetUtility.C_START_GAME += OnStartGameClient;
    }

    
    private void OnStartGameClient(NetMessage obj)
    {
        animator.SetTrigger("InGame");
    }
}