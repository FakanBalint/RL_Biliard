using System;
using System.Collections.Generic;
using UnityEngine;



public class BallEventManager : MonoBehaviour
{
    public static BallEventManager Instance;

    private List<IObserver<BallEvent>> observers = new List<IObserver<BallEvent>>();

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    [SerializeField]private  List<Balls> balls = new List<Balls>();
    
    public void Reset()
    {
        foreach (var ball in balls)
        {
            ball.Reset();
        }
    }

    public void Subscribe(IObserver<BallEvent> observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void Unsubscribe(IObserver<BallEvent> observer)
    {
        observers.Remove(observer);
    }

    public void Notify(BallEvent ballEvent)
    {
        foreach (var observer in observers)
        {
            observer.OnNotify(this, ballEvent);
        }
    }

    public void TriggerBallEvent(Team team)
    {
        BallEvent ballEvent = new BallEvent(team);
        Notify(ballEvent);
    }
}
