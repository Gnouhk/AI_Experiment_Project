using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class UISimulationController : MonoBehaviour
{
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
    private TextMeshProUGUI[] InputTexts;
    [SerializeField]
    private TextMeshProUGUI Evaluation;
    [SerializeField]
    private TextMeshProUGUI GenerationCount;

    private void Update()
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