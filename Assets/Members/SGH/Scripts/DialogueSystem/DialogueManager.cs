using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour, IPointerDownHandler
{
    public TMP_Text dialogueText;
    public GameObject nextText;

    public Queue<string> sentences;

    private string _currentSentence;

    public float typingSpeed = 0.1f;

    private bool _isTyping = false;

    private void Start()
    {
        sentences = new Queue<string>();   
    }

    private void Update()
    {
        if (dialogueText.text.Equals(_currentSentence))
        {
            _isTyping = false;

        }
    }

    public void Ondialogue(string[] lines)
    {
        sentences.Clear();
        foreach (string line in lines)
        {
            sentences.Enqueue(line);
        }
    }

    public void NextSentence()
    {
        if (sentences.Count != 0)
        {
            _currentSentence = sentences.Dequeue();
            _isTyping = true;
            StartCoroutine(Typing(_currentSentence));
        }
    }

  private IEnumerator Typing(string line)
    {
        dialogueText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!_isTyping)
        NextSentence();
    }
}
