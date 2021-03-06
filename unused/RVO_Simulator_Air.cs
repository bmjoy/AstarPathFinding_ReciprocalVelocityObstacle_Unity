﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;

public class RVO_Simulator_Air : MonoBehaviour
{

    public float speed_target;
    public float thresholdToMove;
    List<RVO.Vector2> agentPositions;
    List<GameObject> rvoGameObjs;
    // Use this for initialization
    void Start()
    {
        agentPositions = new List<RVO.Vector2>();
        rvoGameObjs = new List<GameObject>();

        Simulator.Instance.setTimeStep(0.01f);
        // refer to RVO2 library for parameter details
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 2.5f, 40.0f, new RVO.Vector2(0.0f, 0.0f));

    }

    //some helper functions
    Vector3 toUnityPosition(RVO.Vector2 agentVector)
    {
        return new Vector3(agentVector.x(), transform.position.y, agentVector.y());
    }

    RVO.Vector2 toRVOVector(Vector3 agentPosition)
    {
        return new RVO.Vector2(agentPosition.x, agentPosition.z);
    }

    public RVO.Vector2 getAgentPosition(int agentIndex)
    {
        return Simulator.Instance.getAgentPosition(agentIndex);
    }

    public int addAgentToSimulator(Vector3 position, GameObject agent)
    {

        Simulator.Instance.Clear();

        // reinitialize the simulator
        Simulator.Instance.setTimeStep(0.01f);
        // refer to RVO2 library for parameter details
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 2.5f, 40.0f, new RVO.Vector2(0.0f, 0.0f));

        // add all existing agents prior to the current agent being added
        int agentCount = agentPositions.Count;

        for (int i = 0; i < agentCount; i++)
        {
            Simulator.Instance.addAgent(agentPositions[i]);
        }

        // add new agent 
        rvoGameObjs.Add(agent);
        agentPositions.Add(toRVOVector(position));

        //return the index of the newly added agent
        return Simulator.Instance.addAgent(toRVOVector(position));
    }

    // Update is called once per frame
    void Update()
    {

        int agentNum = Simulator.Instance.getNumAgents();
        try
        {
            for (int i = 0; i < agentNum; i++)
            {
                RVO.Vector2 agentPos = Simulator.Instance.getAgentPosition(i);
                RVO.Vector2 goalVector = rvoGameObjs[i].GetComponent<RVO_AirAgentMove>().goalVector();

                if (RVOMath.absSq(goalVector) > thresholdToMove)
                {
                    goalVector = goalVector * speed_target;
                }

                Simulator.Instance.setAgentPrefVelocity(i, goalVector);

                rvoGameObjs[i].transform.position = toUnityPosition(agentPos);
            }
            Simulator.Instance.doStep();

        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.StackTrace);
        }
    }
}

