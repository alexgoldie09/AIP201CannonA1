using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
* This class is used for switching between scenes.
* - It uses methods that are accessed via buttons in Unity.
*   + Methods load scene using Unity's SceneManager.
* - Inherits from monobehaviour to be added as a component.
*/

public class MainMenuController : MonoBehaviour
{
    // LoadScene01() loads in this specific scene (Name has to be spelt correctly).
    public void LoadScene01()
    {
        SceneManager.LoadScene("Scene01");
    }

    // LoadScene02() loads in this specific scene (Name has to be spelt correctly).
    public void LoadScene02()
    {
        SceneManager.LoadScene("Scene02");
    }

    // ReturnToMainMenu() loads in this specific scene (Name has to be spelt correctly).
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // QuitGame() ends the game session.
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Stop playing in the editor.
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
