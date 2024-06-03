using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class UISimulationController : MonoBehaviour
{
    public static UISimulationController Instance { get; private set; }

    private CarController target;

    public CarController Target
    {
        get { return target; }
        set
        {
            if (target != null)
            {
                target = value;
            }
        }
    }

    [SerializeField]
    private TMP_Text[] InputTexts;
    [SerializeField]
    private TMP_Text Evaluation;
    [SerializeField]
    private TMP_Text GenerationCount;

    private void Awake()
    {
        Instance = this;
        InputTexts = GetComponentsInChildren<TMP_Text>();
        Evaluation = GetComponent<TMP_Text>();
        GenerationCount = GetComponent<TMP_Text>();
    }

    public void UpdateUI()
    {
        if (target != null)
        {
            // Display
            if(target.CurrentControlInputs != null)
            {
                for (int i = 0; i < InputTexts.Length; i++)
                {
                    InputTexts[i].text = target.CurrentControlInputs[i].ToString();
                }

                // Display evaluation and generation count
                Evaluation.text = Target.Agent.Genotype.Evaluation.ToString();
                GenerationCount.text = EvolutionManager.Instance.GenerationCount.ToString();
            }
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}