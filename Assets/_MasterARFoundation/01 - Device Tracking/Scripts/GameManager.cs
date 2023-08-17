using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] int _score;

    [Header("General")]
    [SerializeField] TextMeshProUGUI _scoreText;

    private void Start()
    {
        UpdateScore();
    }

    public void AddScore()
    {
        _score++;
        UpdateScore();
    }
    private void UpdateScore()
    {
        _scoreText.text = string.Format("Score: {0}", _score);
        
    }
}
