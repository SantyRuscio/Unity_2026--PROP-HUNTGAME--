using Fusion;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionBrowserPanel : MonoBehaviour
{
    // [SerializeField] RunnerHandler _runnerHandler;

    [SerializeField] SessionItem _sessionItemPrefab;
    [SerializeField] SessionInfo _sessionInfo;

    [SerializeField] private VerticalLayoutGroup _contentLayout;
    [SerializeField] private TMP_Text _statusText;


    private void OnEnable()
    {
       // RunnerHandler.OnSessionListUpdate += UpdateList;
    }
    private void OnDisable()
    {
       // RunnerHandler.OnSessionListUpdate -= UpdateList;
    }


    private void UpdateList(List<SessionItem> SessionList)
    {
        //Si no hay nada en lalista prender el texto status

        if(SessionList.Count == 0)
        {
            _statusText.gameObject.SetActive(true);
            return;
        }

        _statusText.gameObject.SetActive(false);

        //Por cada session en la lista inicializar un nuevo prefab
        foreach(var SessionInfo in SessionList)
        {
            var sessionItem = Instantiate(_sessionItemPrefab, _contentLayout.transform);
        }
    }

    void ClearBrowser()
    {
        foreach(Transform child in _contentLayout.transform)
        {
           Destroy(child.gameObject);
        }
    }
}

public class SessionItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _sessionName;
    [SerializeField] private TMP_Text _playerCount;

    [SerializeField] private Button _joinButton;
    public void Initialize(SessionInfo sessionInfo, RunnerHandler runnerHandler)
    {
        _sessionName.text = sessionInfo.Name;

        _playerCount.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";


        //interactuable solo si la casntidad de playercount es menor a la MAXplayers
        _joinButton.interactable = sessionInfo.PlayerCount < sessionInfo.MaxPlayers;

        _joinButton.onClick.AddListener(CallToJoinGame);
    }

    void CallToJoinGame()
    {
        _joinButton.interactable = false;
       // _runnerHandler.JoinGame(_sessionInfo);
    }
}
