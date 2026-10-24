using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using NumberSlide.Core;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public readonly Game game = new Game(Session.Difficulty);
    [SerializeField] private GameObject WinPanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        WinPanel.SetActive(false);
    }

    void Update()
    {
        HandleKey();
        
        if (game.IsComplete())
        {
            WinPanel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    void HandleKey()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            game.Move(Direction.Up);
            Session.Moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            game.Move(Direction.Left);
            Session.Moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            game.Move(Direction.Down);
            Session.Moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            game.Move(Direction.Right);
            Session.Moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            game.Restart();
            Session.Moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            game.Undo();
            Session.Moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void RestartButtonOnClick()
    {
        WinPanel.SetActive(false);
        Time.timeScale = 1;
        game.Restart();
    }

    public void QuitButtonOnClick()
    {
        Application.Quit();
    }
}
