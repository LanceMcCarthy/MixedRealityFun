using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSpaceStation : MonoBehaviour, IInputClickHandler
{
    public void OnInputClicked(InputClickedEventData eventData)
    {
        SceneManager.LoadScene("SpaceStation", LoadSceneMode.Single);

        eventData.Use(); 
    }
}
