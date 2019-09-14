using System;
using System.Xml.Linq;
using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
using AIChara;
using BepInEx.Harmony;
using HarmonyLib;
using BepInEx.Logging;

namespace GrabRave
{
    [BepInPlugin(GUID, "GrabRave", VERSION)]
    public class GrabRavePlugin : BaseUnityPlugin
    {
        public const string GUID = "com.hooh.grabrave";
        public const string VERSION = "1.0.0";

        public static ConfigWrapper<string> keyGrabHeadPYR { get; private set; }
        public static ConfigWrapper<string> keyGrabHeadR { get; private set; }
        public static ConfigWrapper<string> keyTogga { get; private set; }

        readonly static string commonString = "BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/";
        readonly static string[] names =
        {
            "Head",
            "Neck",
            "Eyes"
        };
        readonly string[][] targetArray =
        {
            new string[]{ commonString + "cf_J_Neck/cf_J_Head" },
            new string[]{ commonString + "cf_J_Neck" },
            new string[]
            {
                commonString + "cf_J_Neck/cf_J_Head/cf_J_Head_s/p_cf_head_bone/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_t_L/cf_J_Eye_s_L/cf_J_EyePos_rz_L/cf_J_look_L/cf_J_eye_rs_L",
                commonString + "cf_J_Neck/cf_J_Head/cf_J_Head_s/p_cf_head_bone/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_t_R/cf_J_Eye_s_R/cf_J_EyePos_rz_R/cf_J_look_R/cf_J_eye_rs_R",
            }
        };
        Vector3[] posMate =
        {
            new Vector3(),
            new Vector3()
        };
        int currentIndex = 0;

        Vector3 currentMousePosition;
        Vector3 lastMousePosition;
        Vector3 deltaMousePosition;

        internal static new ManualLogSource Logger;

        ChaControl character;

        private void Start()
        {
            Logger = base.Logger;

            keyGrabHeadPYR = Config.Wrap<string>("Private Mod", "Grab Pitch Yaw", "Grab head's pitch and yaw", "q");
            keyTogga = Config.Wrap<string>("Private Mod", "Grab Change", "Change Grab Target", "w");

            HarmonyWrapper.PatchAll(typeof(GrabRavePlugin));
        }

        private void LateUpdate()
        {
            if (character == null)
                character = FindObjectOfType<ChaControl>();

            currentMousePosition = Input.mousePosition;
            deltaMousePosition = currentMousePosition - lastMousePosition;
            deltaMousePosition *= (float) 0.1;

            if (Input.GetKeyDown(keyGrabHeadPYR.Value))
                Camera.main.GetComponent<CameraControl_Ver2>().enabled = false;
            else if (Input.GetKeyUp(keyGrabHeadPYR.Value))
                Camera.main.GetComponent<CameraControl_Ver2>().enabled = true;

            if (Input.GetKey(keyGrabHeadPYR.Value.ToLower()))
            {
                if (Input.GetMouseButton(0))
                    GrabEmByThePussy(0);
                else if (Input.GetMouseButton(1))
                    GrabEmByThePussy(1);

                if (Input.GetMouseButtonDown(2))
                {
                    for (int i = 0; i < posMate.Length; i++)
                    {
                        for (int a = 0; a < targetArray[i].Length; a++)
                        {
                            posMate[i] = Vector3.zero;
                            string targetString = targetArray[i][a];
                            Transform thatTransform = character.gameObject.transform.Find(targetString);
                            thatTransform.localEulerAngles = new Vector3();
                        }
                    }
                }
            }

            if (Input.GetKeyDown(keyTogga.Value.ToLower()))
            {
                currentIndex++;
                currentIndex = currentIndex % targetArray.Length;
                Logger.LogMessage(String.Format("GrabRave Mode: {0}", names[currentIndex]));
            }

            if (character != null)
            {
                for (int i = 0; i < posMate.Length; i++)
                {
                    for (int a = 0; a < targetArray[i].Length; a++)
                    {
                        string targetString = targetArray[i][a];
                        Transform thatTransform = character.gameObject.transform.Find(targetString);
                        if (posMate[i] == Vector3.zero)
                            posMate[i] = thatTransform.localEulerAngles;
                        else
                            thatTransform.localEulerAngles = posMate[i];
                    }
                }
            }

            lastMousePosition = currentMousePosition;
        }

        private void GrabEmByThePussy(int shit)
        {
            if (character != null)
            {
                for (int i = 0; i < targetArray[currentIndex].Length; i++)
                {
                    string targetString = targetArray[currentIndex][i];
                    Transform thatTransform = character.gameObject.transform.Find(targetString);

                    if (thatTransform != null)
                    {
                        if (currentIndex <= 1)
                        {
                            if (shit == 0)
                                posMate[currentIndex] -= new Vector3(deltaMousePosition.y, deltaMousePosition.x);
                            else
                                posMate[currentIndex] -= new Vector3(0,0, deltaMousePosition.y);
                        } else
                        {
                            if (shit == 0)
                                thatTransform.localEulerAngles -= new Vector3(deltaMousePosition.y, deltaMousePosition.x);
                            else
                                thatTransform.localEulerAngles -= new Vector3(0, 0, deltaMousePosition.y);
                        }
                    }
                }
            }
        }
    }
}
