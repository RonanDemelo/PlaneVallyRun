using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintPasser : MonoBehaviour
{
    [SerializeField]private TutorialHints tutorialHints;
    [TextArea(4, 8)] [SerializeField]private string text;

    private void OnTriggerEnter(Collider other)
    {
        tutorialHints.hint.text = text;
    }

    private void OnTriggerExit(Collider other)
    {
        tutorialHints.hint.text = "";
    }
}
