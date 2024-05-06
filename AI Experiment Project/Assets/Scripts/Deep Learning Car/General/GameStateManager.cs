using UnityEngine.SceneManagement;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    #region Members

    [SerializeField]
    public string TrackName;

    public static GameStateManager Instance { get; private set; }

    private CarController prevBest, prevSecondBest;

    #endregion

    #region Constructors

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple GameStateManager in the Scene");
            return;
        }

        Instance = this;

        // Load track
        SceneManager.LoadScene(TrackName, LoadSceneMode.Additive);
    }

    private void Start()
    {
        EvolutionManager.Instance.StartEvolution();
    }

    #endregion

    #region Methods

    #endregion
}
