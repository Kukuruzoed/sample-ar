using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class InstantPlacement : MonoBehaviour
{
    public ARRaycastManager rayManager; // set from the Editor Inspector.
    public GameObject hcapPlayerPref;
    private GameObject hcapPlayer;
    public GameObject cubePref;
    private GameObject cube;
    void Update()
    {
        if (Input.touchCount > 0)
        {
            // shoot a raycast from the center of the screen
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            rayManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes);

            // if we hit an AR plane surface, update the position and rotation
            if (hits.Count > 0)
            {
                Debug.Log("hit");
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Stationary)
                {
                    transform.position = hits[0].pose.position;
                    transform.rotation = hits[0].pose.rotation;

                    Destroy(hcapPlayer);
                    hcapPlayer = null;

                    if (cube == null)
                        cube = Instantiate(cubePref, hits[0].pose.position, hits[0].pose.rotation);
                    else
                        cube.transform.position = hits[0].pose.position;
                }
                if(touch.phase == TouchPhase.Ended)
                {
                    if(hcapPlayer == null)
                    {
                        hcapPlayer = Instantiate(hcapPlayerPref, hits[0].pose.position, hits[0].pose.rotation);
                        hcapPlayer.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
                    }
                }
            }
            else
            {
                Debug.Log("unhit");
            }
        }
    }
}
