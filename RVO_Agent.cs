﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using RVO;

public class RVO_Agent : MonoBehaviour {

    public Camera cam;

    [SerializeField]
    Vector3 target;
    int currentNodeIndex = 0;
    int agentIndex = -1;

    RVO_Simulator simulator = null;
    private List<Vector3> pathNodes = null;
    bool isAbleToStart = false;
    float thresholdToNode = 4.0f;

    Seeker seeker;
    Path path;
    CharacterController characterController;
    // Use this for initialization
    IEnumerator Start()
    {
        currentNodeIndex = 0;
        simulator = GameObject.FindGameObjectWithTag("RVO_sim").GetComponent<RVO_Simulator>();
        characterController = GetComponent<CharacterController>();
        pathNodes = new List<Vector3>();
        yield return StartCoroutine(StartPaths());
        agentIndex = simulator.addAgentToSimulator(transform.position, gameObject, pathNodes);

        //isAbleToStart = true;
    }
    IEnumerator StartPaths()
    {
        seeker = gameObject.GetComponent<Seeker>();
        target = transform.position + transform.forward * simulator.speed_target * 0.1f;
        //target = Input.mousePosition;
        path = seeker.StartPath(transform.position, target, OnPathComplete);
        yield return StartCoroutine(path.WaitForPath());
    }

    public void OnPathComplete(Path p)
    {
        // We got our path back
        if (p.error)
        {
            // Nooo, a valid path couldn't be found
            Debug.Log("the error is at StartPath!" + p.error);
        }
        else
        {
            // Yay, now we can get a Vector3 representation of the path
            // from p.vectorPath
            path = p;
            pathNodes = p.vectorPath;

            currentNodeIndex = 0;
        }
    }

	// Update is called once per frame
	void FixedUpdate () {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("New click activated! Current target at click: " + Input.mousePosition.ToString());

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                target = hit.point;
                Debug.Log("current target AFTER update: " + target.ToString());
                seeker.StartPath(transform.position, target, OnPathComplete);
                Debug.Log("new path AFTER update has waypoint list length: " + path.vectorPath.Count.ToString());
            }
        }
         
        if (path == null)
        {
            Debug.Log("No available path found!");
            return;

        }
        if (currentNodeIndex >= (path.vectorPath.Count - 1))
        {
            //Debug.Log("Path Finding ended!");
            return;
        }
        if (agentIndex != -1){
            RVO.Vector2 agent_update = simulator.getAgentPosition(agentIndex);
            Vector3 moveTowards = new Vector3(agent_update.x(), transform.position.y, agent_update.y());
            //Vector3 velocity = (moveTowards - transform.position).normalized * speed;
            transform.position = moveTowards;
            // Slow down smoothly upon approaching the end of the path
            // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
            //var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

            //Debug.Log("Current WayPoint = " + path.vectorPath[currentNodeIndex].ToString());
            //Debug.Log("Current Position = " + transform.position.ToString());

            //Debug.Log("Distance to move = " + velocity.ToString());

            //characterController.SimpleMove(velocity);
        }
		
	}
    //obtain the next Path node point 
    public RVO.Vector2 nextPathNode(){
        Vector3 node_pos;
        if(currentNodeIndex < pathNodes.Count)
        {
            node_pos = pathNodes[currentNodeIndex];
            float distance = Vector3.Distance(node_pos, transform.position);

            if (distance < thresholdToNode){
                currentNodeIndex++;
                node_pos = pathNodes[currentNodeIndex];
            }
        }
        else{
            // last node of the A* path
            node_pos = pathNodes[pathNodes.Count - 1];
        }
        return new RVO.Vector2(node_pos.x, node_pos.z);
    }
}
