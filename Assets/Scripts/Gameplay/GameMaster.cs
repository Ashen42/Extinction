using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : IntEventInvoker
{
    //step and turn control
    int step = 0;
    int turn = 1;
    int numberOfSteps = 5;

    Timer stepTimer;
    float stepDuration = .001f;

    // Start is called before the first frame update
    void Awake()
    {
        //set up the step timer
        stepTimer = gameObject.AddComponent<Timer>();
        stepTimer.Duration = stepDuration;
        stepTimer.AddTimerFinishedEventListener(UpdateStep);

        //initialize the event manager
        EventManager.Initialize();

        //create the events and set the GameMaster as an invoker
        unityEvents.Add(EventName.StepUpkeepEvent, new StepUpkeepEvent());
        EventManager.AddInvoker(EventName.StepUpkeepEvent, this);
        unityEvents.Add(EventName.StepNPCMovementEvent, new StepNPCMovementEvent());
        EventManager.AddInvoker(EventName.StepNPCMovementEvent, this);
        unityEvents.Add(EventName.StepP1MovementEvent, new StepP1MovementEvent());
        EventManager.AddInvoker(EventName.StepP1MovementEvent, this);
        unityEvents.Add(EventName.StepP2MovementEvent, new StepP2MovementEvent());
        EventManager.AddInvoker(EventName.StepP2MovementEvent, this);
        unityEvents.Add(EventName.StepP3MovementEvent, new StepP3MovementEvent());
        EventManager.AddInvoker(EventName.StepP3MovementEvent, this);
        unityEvents.Add(EventName.StepP4MovementEvent, new StepP4MovementEvent());
        EventManager.AddInvoker(EventName.StepP4MovementEvent, this);
        
        //start the first step
        UpdateStep();
    }

    private void UpdateStep ()
    {
        if (step < numberOfSteps) step++;
        else step = 0;
        switch (step)
        {
            case 0:
                unityEvents[EventName.StepUpkeepEvent].Invoke(0);
                break;
            case 1:
                unityEvents[EventName.StepNPCMovementEvent].Invoke(0);
                break;
            case 2:
                unityEvents[EventName.StepP1MovementEvent].Invoke(0);
                break;
            case 3:
                unityEvents[EventName.StepP2MovementEvent].Invoke(0);
                break;
            case 4:
                unityEvents[EventName.StepP3MovementEvent].Invoke(0);
                break;
            case 5:
                unityEvents[EventName.StepP4MovementEvent].Invoke(0);
                UpdateTurn();
                break;
        }
        stepTimer.Run();
    }

    private void UpdateTurn ()
    {
        turn++;
    }

    public float getRoundDuration() { return numberOfSteps * stepDuration; }

}
