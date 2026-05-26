using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionBrowserPanel : MonoBehaviour
{
    [SerializeField] RunnerHandler _runnerHandler;

    [SerializeField] SessionItem _sessionItemPrefab;
    [SerializeField] SessionInfo _sessionInfo;

    [SerializeField] private VerticalLayoutGroup _contentLayout;
    [SerializeField] private TMP_Text _statusText;


    private void OnEnable()
    {
        _runnerHandler.OnSessionListUpdate += UpdateList;
    }

    private void OnDisable()
    {
        _runnerHandler.OnSessionListUpdate -= UpdateList;
    }


    private void UpdateList(List<SessionInfo> SessionList)
   {
        ClearBrowser();
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
            sessionItem.Initialize(SessionInfo, _runnerHandler);    
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
