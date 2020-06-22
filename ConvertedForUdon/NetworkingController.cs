using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

public class NetworkingController : UdonSharpBehaviour
{
    #region variables
    public ElevatorRequester _elevatorRequester;
    public ElevatorController _elevatorControllerReception;
    public ElevatorController _elevatorControllerArrivalArea;
    public InsidePanelScriptForDesktop _insidePanelScriptElevatorDesktop_0;
    public InsidePanelScriptForVR _InsidePanelScriptElevatorForVR_0;
    public InsidePanelScriptForDesktop _insidePanelScriptElevatorDesktop_1;
    public InsidePanelScriptForVR _InsidePanelScriptElevatorForVR_1;
    public InsidePanelScriptForDesktop _insidePanelScriptElevatorDesktop_2;
    public InsidePanelScriptForVR _InsidePanelScriptElevatorForVR_2;
    public InsidePanelScriptForDesktop _insidePanelFloorScriptElevatorDesktop_0;
    public InsidePanelScriptForVR _InsidePanelFloorScriptElevatorForVR_0;
    public InsidePanelScriptForDesktop _insidePanelFloorScriptElevatorDesktop_1;
    public InsidePanelScriptForVR _InsidePanelFloorScriptElevatorForVR_1;
    public InsidePanelScriptForDesktop _insidePanelFloorScriptElevatorDesktop_2;
    public InsidePanelScriptForVR _InsidePanelFloorScriptElevatorForVR_2;
    ///<summary> animation-timing-parameters </summary>
    private const float TIME_TO_STAY_CLOSED_AFTER_GOING_OUT_OF_IDLE = 3f;
    private const float TIME_TO_STAY_OPEN = 10f; // 10f is normal
    private const float TIME_TO_STAY_OPEN_RECEPTION = 10f; //30f is normal
    private const float TIME_TO_STAY_CLOSED = 4f; //MUST BE 4f IN UNITY! Because of the closing animation
    private const float TIME_TO_DRIVE_ONE_FLOOR = 2f;
    /// <summary>
    /// elevator states, synced by master
    /// </summary>
    [HideInInspector, UdonSynced(UdonSyncMode.None)]
    public long _syncData1forReal = 0;
    private long _syncData1forRealLocalCopyForSyncCheck = 0;
    private ulong _syncData1 = 0;
    private ulong _syncData1LocalCopyForSyncCheck = 0;
    /// <summary>
    /// elevator request states, synced by master
    /// </summary>
    [HideInInspector, UdonSynced(UdonSyncMode.None)]
    public uint _syncData2 = 0;
    /// <summary>
    /// Current floor level that localPlayer is on
    /// </summary>
    private int _localPlayerCurrentFloor = 7;
    /// <summary>
    /// other public variables
    /// </summary>
    VRCPlayerApi _localPlayer;
    private bool _finishedLocalSetup = false;
    private bool _isMaster = false;
    private bool _worldIsLoaded = false;
    private bool _userIsInVR;
    /// <summary>
    /// Locally storing which elevator is currently working and which isn't, since we only need to read this 
    /// once from the SYNC state and it won't change, so it would be a waste to read it every time again
    /// This is read by LOCAL but also used by MASTER
    /// </summary>
    private bool _elevator0Working = false;
    private bool _elevator1Working = false;
    private bool _elevator2Working = false;
    #endregion variables
    //------------------------------------------------------------------------------------------------------------
    //------------------------------------SYNCBOOL ENUM-----------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------
    #region ENUM_SYNCBOOL
    /// <summary>
    /// "ENUM" of different bools that are synced in _syncData
    /// (ENUM isn't possible in Udon, so we use this here)
    ///  - 55-52 variable_3 (4bits)
    ///  - 0-51 binary bools [0-51]
    ///  - 0-31 binary bools [52-83(?)]
    /// </summary>
    /// 
    private const int SyncBoolReq_BellOn = 0;
    private const int SyncBool_Elevator0open = 1;
    private const int SyncBool_Elevator1open = 2;
    private const int SyncBool_Elevator2open = 3;
    private const int SyncBool_Elevator0idle = 4;
    private const int SyncBool_Elevator1idle = 5;
    private const int SyncBool_Elevator2idle = 6;
    private const int SyncBool_Elevator0goingUp = 7;
    private const int SyncBool_Elevator1goingUp = 8;
    private const int SyncBool_Elevator2goingUp = 9;
    /// <summary>
    /// Sync-data positions for elevator call up buttons
    /// </summary>
    private const int SyncBoolReq_ElevatorCalledUp_0 = 10;
    private const int SyncBoolReq_ElevatorCalledUp_1 = 11;
    private const int SyncBoolReq_ElevatorCalledUp_2 = 12;
    private const int SyncBoolReq_ElevatorCalledUp_3 = 13;
    private const int SyncBoolReq_ElevatorCalledUp_4 = 14;
    private const int SyncBoolReq_ElevatorCalledUp_5 = 15;
    private const int SyncBoolReq_ElevatorCalledUp_6 = 16;
    private const int SyncBoolReq_ElevatorCalledUp_7 = 17;
    private const int SyncBoolReq_ElevatorCalledUp_8 = 18;
    private const int SyncBoolReq_ElevatorCalledUp_9 = 19;
    private const int SyncBoolReq_ElevatorCalledUp_10 = 20;
    private const int SyncBoolReq_ElevatorCalledUp_11 = 21;
    private const int SyncBoolReq_ElevatorCalledUp_12 = 22;
    private const int SyncBoolReq_ElevatorCalledUp_13 = 23;
    /// <summary>
    /// Sync-data positions for elevator call down buttons
    /// </summary>
    private const int SyncBoolReq_ElevatorCalledDown_0 = 24;
    private const int SyncBoolReq_ElevatorCalledDown_1 = 25;
    private const int SyncBoolReq_ElevatorCalledDown_2 = 26;
    private const int SyncBoolReq_ElevatorCalledDown_3 = 27;
    private const int SyncBoolReq_ElevatorCalledDown_4 = 28;
    private const int SyncBoolReq_ElevatorCalledDown_5 = 29;
    private const int SyncBoolReq_ElevatorCalledDown_6 = 30;
    private const int SyncBoolReq_ElevatorCalledDown_7 = 31;
    private const int SyncBoolReq_ElevatorCalledDown_8 = 32;
    private const int SyncBoolReq_ElevatorCalledDown_9 = 33;
    private const int SyncBoolReq_ElevatorCalledDown_10 = 34;
    private const int SyncBoolReq_ElevatorCalledDown_11 = 35;
    private const int SyncBoolReq_ElevatorCalledDown_12 = 36;
    private const int SyncBoolReq_ElevatorCalledDown_13 = 37;
    /// <summary>
    /// Sync-data positions for internal elevator 0 buttons
    /// </summary>
    private const int SyncBoolReq_Elevator0CalledToFloor_0 = 38;
    private const int SyncBoolReq_Elevator0CalledToFloor_1 = 39;
    private const int SyncBoolReq_Elevator0CalledToFloor_2 = 40;
    private const int SyncBoolReq_Elevator0CalledToFloor_3 = 41;
    private const int SyncBoolReq_Elevator0CalledToFloor_4 = 42;
    private const int SyncBoolReq_Elevator0CalledToFloor_5 = 43;
    private const int SyncBoolReq_Elevator0CalledToFloor_6 = 44;
    private const int SyncBoolReq_Elevator0CalledToFloor_7 = 45;
    private const int SyncBoolReq_Elevator0CalledToFloor_8 = 46;
    private const int SyncBoolReq_Elevator0CalledToFloor_9 = 47;
    private const int SyncBoolReq_Elevator0CalledToFloor_10 = 48;
    private const int SyncBoolReq_Elevator0CalledToFloor_11 = 49;
    private const int SyncBoolReq_Elevator0CalledToFloor_12 = 50;
    private const int SyncBoolReq_Elevator0CalledToFloor_13 = 51;
    /// <summary>
    /// Sync-data positions for internal elevator 1 buttons
    /// </summary>
    private const int SyncBoolReq_Elevator1CalledToFloor_0 = 52;
    private const int SyncBoolReq_Elevator1CalledToFloor_1 = 53;
    private const int SyncBoolReq_Elevator1CalledToFloor_2 = 54;
    private const int SyncBoolReq_Elevator1CalledToFloor_3 = 55;
    private const int SyncBoolReq_Elevator1CalledToFloor_4 = 56;
    private const int SyncBoolReq_Elevator1CalledToFloor_5 = 57;
    private const int SyncBoolReq_Elevator1CalledToFloor_6 = 58;
    private const int SyncBoolReq_Elevator1CalledToFloor_7 = 59;
    private const int SyncBoolReq_Elevator1CalledToFloor_8 = 60;
    private const int SyncBoolReq_Elevator1CalledToFloor_9 = 61;
    private const int SyncBoolReq_Elevator1CalledToFloor_10 = 62;
    private const int SyncBoolReq_Elevator1CalledToFloor_11 = 63;
    private const int SyncBoolReq_Elevator1CalledToFloor_12 = 64;
    private const int SyncBoolReq_Elevator1CalledToFloor_13 = 65;
    /// <summary>
    /// Sync-data positions for internal elevator 2 buttons
    /// </summary>
    private const int SyncBoolReq_Elevator2CalledToFloor_0 = 66;
    private const int SyncBoolReq_Elevator2CalledToFloor_1 = 67;
    private const int SyncBoolReq_Elevator2CalledToFloor_2 = 68;
    private const int SyncBoolReq_Elevator2CalledToFloor_3 = 69;
    private const int SyncBoolReq_Elevator2CalledToFloor_4 = 70;
    private const int SyncBoolReq_Elevator2CalledToFloor_5 = 71;
    private const int SyncBoolReq_Elevator2CalledToFloor_6 = 72;
    private const int SyncBoolReq_Elevator2CalledToFloor_7 = 73;
    private const int SyncBoolReq_Elevator2CalledToFloor_8 = 74;
    private const int SyncBoolReq_Elevator2CalledToFloor_9 = 75;
    private const int SyncBoolReq_Elevator2CalledToFloor_10 = 76;
    private const int SyncBoolReq_Elevator2CalledToFloor_11 = 77;
    private const int SyncBoolReq_Elevator2CalledToFloor_12 = 78;
    private const int SyncBoolReq_Elevator2CalledToFloor_13 = 79;
    /// <summary>
    /// Last few state bools
    /// </summary>
    private const int SyncBool_Initialized = 80;
    private const int SyncBool_Elevator0working = 81;
    private const int SyncBool_Elevator1working = 82;
    private const int SyncBool_Elevator2working = 83;
    #endregion ENUM_SYNCBOOL
    #region ENUM_DIRECTSYNCBOOL
    /// <summary>
    /// Direct bit masks and addresses for the "Enum_Syncbool" bools - Remember to update both!!!
    /// 
    /// The GetSyncValue(*) function has been swapped to speed things up a bit.                        
    ///                 
    /// Accessing using mask (if true)
    ///   -"0UL != (_syncData1 & (SyncBool_MaskUlong"
    ///   - "0U != (_syncData2 & (SyncBool_MaskUint"
    /// Or for "not"ed functions (if false)
    ///   -"0UL == (_syncData1 & (SyncBool_MaskUlong"
    ///   - "0U == (_syncData2 & (SyncBool_MaskUint"
    /// 
    /// Accessing using it like an array (aka address based [slower])
    ///  Checks if true:
    ///  - (0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong
    ///  - (0U != (_syncData2 & (1U << (SyncBool_AddressUint
    ///  Checks if false:
    ///  - (0UL == (_syncData1 & (1UL << (SyncBool_AddressUlong
    ///  - (0U == (_syncData2 & (1U << (SyncBool_AddressUint
    /// </summary>
    ///         
    private const ulong SyncBoolReq_MaskUlong_BellOn = (1UL);
    private const int SyncBool_AddressUlong_ElevatorXopen = 1;
    private const ulong SyncBool_MaskUlong_Elevator0open = (1UL << 1);
    private const ulong SyncBool_MaskUlong_Elevator1open = (1UL << 2);
    private const ulong SyncBool_MaskUlong_Elevator2open = (1UL << 3);
    private const int SyncBool_AddressUlong_ElevatorXidle = 4;
    private const ulong SyncBool_MaskUlong_Elevator0idle = (1UL << 4);
    private const ulong SyncBool_MaskUlong_Elevator1idle = (1UL << 5);
    private const ulong SyncBool_MaskUlong_Elevator2idle = (1UL << 6);
    private const int SyncBool_AddressUlong_ElevatorXgoingUp = 7;
    private const ulong SyncBool_MaskUlong_Elevator0goingUp = (1UL << 7);
    private const ulong SyncBool_MaskUlong_Elevator1goingUp = (1UL << 8);
    private const ulong SyncBool_MaskUlong_Elevator2goingUp = (1UL << 9);
    /// <summary>     
    /// Sync-data positions for elevator call up
    /// </summary>            
    private const int SyncBoolReq_AddressUlong_ElevatorCalledUp = 10;
    /*private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_0 = (1UL << 10);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_1 = (1UL << 11);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_2 = (1UL << 12);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_3 = (1UL << 13);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_4 = (1UL << 14);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_5 = (1UL << 15);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_6 = (1UL << 16);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_7 = (1UL 17);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_8 = (1UL << 18);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_9 = (1UL << 19);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_10 = (1UL << 20);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_11 = (1UL << 21);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_12 = (1UL << 22);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledUp_13 = (1UL << 23);*/

    /// <summary>     
    /// Sync-data positions for elevator call down
    /// </summary>     
    private const int SyncBoolReq_AddressUlong_ElevatorCalledDown = 24;
    /*private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_0 = (1UL << 24);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_1 = (1UL << 25);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_2 = (1UL << 26);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_3 = (1UL << 27);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_4 = (1UL << 28);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_5 = (1UL << 29);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_6 = (1UL << 30);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_7 = (1UL << 31);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_8 = (1UL << 32);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_9 = (1UL << 33);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_10 = (1UL << 34);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_11 = (1UL << 35);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_12 = (1UL << 36);
    private const ulong SyncBoolReq_MaskUlong_ElevatorCalledDown_13 = (1UL << 37);*/

    /// <summary>     
    /// Sync-data positions for internal elevator 0
    /// ******THIS CANNOT BE USED****** It spans both the Ulong and Uint, use getSyncValues instead
    /// </summary>    
    private const int SyncBoolReq_AddressUlong_Elevator0CalledToFloor = 38;
    /*private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_0 = (1UL << 38);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_1 = (1UL << 39);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_2 = (1UL << 40);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_3 = (1UL << 41);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_4 = (1UL << 42);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_5 = (1UL << 43);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_6 = (1UL << 44);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_7 = (1UL << 45);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_8 = (1UL << 46);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_9 = (1UL << 47);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_10 = (1UL << 48);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_11 = (1UL << 49);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_12 = (1UL << 50);
    private const ulong SyncBoolReq_MaskUlong_Elevator0CalledToFloor_13 = (1UL << 51);*/
    /// <summary>     
    /// Sync-data positions for internal elevator 1
    /// </summary>     
    private const int SyncBoolReq_AddressUint_Elevator1CalledToFloor = 0;
    /*private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_0 = (1U << 0);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_1 = (1U << 1);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_2 = (1U << 2);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_3 = (1U << 3);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_4 = (1U << 4);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_5 = (1U << 5);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_6 = (1U << 6);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_7 = (1U << 7);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_8 = (1U << 8);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_9 = (1U << 9);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_10 = (1U << 10);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_11 = (1U << 11);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_12 = (1U << 12);
    private const uint SyncBoolReq_MaskUint_Elevator1CalledToFloor_13 = (1U << 13);*/
    /// <summary>     
    /// Sync-data positions for internal elevator 2
    /// </summary>     
    private const int SyncBoolReq_AddressUint_Elevator2CalledToFloor = 14;
    /*private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_0 = (1U << 14);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_1 = (1U << 15);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_2 = (1U << 16);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_3 = (1U << 17);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_4 = (1U << 18);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_5 = (1U << 19);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_6 = (1U << 20);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_7 = (1U << 21);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_8 = (1U << 22);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_9 = (1U << 23);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_10 = (1U << 24);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_11 = (1U << 25);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_12 = (1U << 26);
    private const uint SyncBoolReq_MaskUint_Elevator2CalledToFloor_13 = (1U << 27);*/

    private const uint SyncBool_MaskUint_Initialized = (1U << 28);
    private const int SyncBool_AddressUlong_ElevatorXworking = 29;
    private const uint SyncBool_MaskUint_Elevator0working = (1U << 29);
    private const uint SyncBool_MaskUint_Elevator1working = (1U << 30);
    private const uint SyncBool_MaskUint_Elevator2working = (1U << 31);

    #endregion ENUM_DIRECTSYNCBOOL
    //------------------------------------------------------------------------------------------------------------
    //----------------------------------- START/UPDATE functions -------------------------------------------------
    //------------------------------------------------------------------------------------------------------------
    #region START_UPDATE_FUNCTIONS
    /// <summary>
    /// Initializing the scene
    /// </summary>
    public void Start()
    {
        Debug.Log("[NetworkController] NetworkingController is now in Start()");
        _localPlayer = Networking.LocalPlayer;
        _userIsInVR = _localPlayer.IsUserInVR();
        //the first master has to set the constant scene settings
        if (_localPlayer.isMaster && 0U == (_syncData2 & (SyncBool_MaskUint_Initialized)))
        {
            _isMaster = true;
            MASTER_SetConstSceneElevatorStates();
            MASTER_FirstMasterSetupElevatorControl();
        }
        _elevatorControllerReception.CustomStart();
        _elevatorControllerArrivalArea.CustomStart();
        //for reception level
        _insidePanelScriptElevatorDesktop_0.CustomStart();
        _InsidePanelScriptElevatorForVR_0.CustomStart();
        _insidePanelScriptElevatorDesktop_1.CustomStart();
        _InsidePanelScriptElevatorForVR_1.CustomStart();
        _insidePanelScriptElevatorDesktop_2.CustomStart();
        _InsidePanelScriptElevatorForVR_2.CustomStart();
        //for the floor elevators as well
        _insidePanelFloorScriptElevatorDesktop_0.CustomStart();
        _InsidePanelFloorScriptElevatorForVR_0.CustomStart();
        _insidePanelFloorScriptElevatorDesktop_1.CustomStart();
        _InsidePanelFloorScriptElevatorForVR_1.CustomStart();
        _insidePanelFloorScriptElevatorDesktop_2.CustomStart();
        _InsidePanelFloorScriptElevatorForVR_2.CustomStart();
        Debug.Log("[NetworkController] Elevator NetworkingController is now loaded");
        _worldIsLoaded = true;
    }
    /// <summary>
    /// This update is run every frame
    /// </summary>
    public void Update()
    {
        if (_localPlayer.isMaster)
        {
            if (!_isMaster)
            {
                Debug.Log("[NetworkController] Master has changed!");
                MASTER_OnMasterChanged();
                _isMaster = true;
            }
            //first process network events because Master can't do that else
            LOCAL_OnDeserialization();
            //only the current master does this
            MASTER_RunElevatorControl();
            //WORKAROUND for UInt64 not working
            if (_syncData1 != _syncData1LocalCopyForSyncCheck)
            {
                Debug.Log("[NetworkController] Master has set _syncData1");
                _syncData1forReal = (long)_syncData1; //send to clients
                _syncData1LocalCopyForSyncCheck = _syncData1;
            }
        }
        //Checking if local external call was handled or dropped
        LOCAL_CheckIfElevatorExternalCallWasReceived();
        //Checking if local internal call was handled or dropped
        LOCAL_CheckIfElevatorInternalCallWasReceived();
    }
    #endregion START_UPDATE_FUNCTIONS
    //------------------------------------------------------------------------------------------------------------
    //----------------------------------- MASTER functions -------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------
    #region MASTER_FUNCTIONS
    /// <summary>
    /// locally storing where each elevator is and has to go, these need to be checked against SyncBool states 
    /// </summary>
    private bool[] _elevatorIsDriving_MASTER = new bool[3]; //this array is local only and not synced
    private bool[] _calledToFloorToGoUp_MASTER = new bool[14];
    private int _calledToFloorToGoUp_MASTER_COUNT = 0;
    private bool[] _calledToFloorToGoDown_MASTER = new bool[14];
    private int _calledToFloorToGoDown_MASTER_COUNT = 0;
    private bool[] _elevator0FloorTargets_MASTER = new bool[14];
    private int _elevator0FloorTargets_MASTER_COUNT = 0;
    private bool[] _elevator1FloorTargets_MASTER = new bool[14];
    private int _elevator1FloorTargets_MASTER_COUNT = 0;
    private bool[] _elevator2FloorTargets_MASTER = new bool[14];
    private int _elevator2FloorTargets_MASTER_COUNT = 0;
    private float[] _timeAtCurrentFloorElevatorOpened_MASTER = new float[3];
    private float[] _timeAtCurrentFloorElevatorClosed_MASTER = new float[3];
    private bool[] _floorHasFakeEREQ_MASTER = new bool[14];
    private int _elevatorCheckTick_MASTER = 1;
    /// <summary>
    /// The first Master (on instance start) needs to run this once to set the initial elevator states and positions
    /// </summary>
    private void MASTER_FirstMasterSetupElevatorControl()
    {
        Debug.Log("[NetworkController] FirstMasterSetupElevatorControl started");
        MASTER_SetSyncValue(SyncBool_Elevator0goingUp, false);
        MASTER_SetSyncValue(SyncBool_Elevator1goingUp, false);
        MASTER_SetSyncValue(SyncBool_Elevator2goingUp, false);
        MASTER_SetSyncValue(SyncBool_Elevator0idle, true);
        MASTER_SetSyncValue(SyncBool_Elevator1idle, true);
        MASTER_SetSyncValue(SyncBool_Elevator2idle, true);
        MASTER_SetSyncElevatorFloor(0, 13);
        MASTER_SetSyncElevatorFloor(1, 8);
        MASTER_SetSyncElevatorFloor(2, 2);
        Debug.Log("[NetworkController] FirstMasterSetupElevatorControl finished");
    }
    /// <summary>
    /// When the master changes, we need to load the SyncBool states into local copies to run the elevator controller correct
    /// before we actually run the elevator controller for the first time
    /// </summary>
    private void MASTER_OnMasterChanged()
    {
        //resetting arrays and counters
        _elevatorIsDriving_MASTER = new bool[3];
        _elevator0FloorTargets_MASTER = new bool[14];
        _elevator0FloorTargets_MASTER_COUNT = 0;
        _elevator1FloorTargets_MASTER = new bool[14];
        _elevator1FloorTargets_MASTER_COUNT = 0;
        _elevator2FloorTargets_MASTER = new bool[14];
        _elevator2FloorTargets_MASTER_COUNT = 0;
        _calledToFloorToGoUp_MASTER = new bool[14];
        _calledToFloorToGoUp_MASTER_COUNT = 0;
        _calledToFloorToGoDown_MASTER = new bool[14];
        _calledToFloorToGoDown_MASTER_COUNT = 0;
        _floorHasFakeEREQ_MASTER = new bool[14];
        //taking all content from SyncedData into local arrays
        for (int i = 0; i <= 13; i++)
        {
            //If Elevator0 called to floor i
            if (0UL != (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_Elevator0CalledToFloor + i))))
            {
                _elevator0FloorTargets_MASTER[i] = true;
                _elevator0FloorTargets_MASTER_COUNT++;
            }
            //If Elevator1 called to floor i
            if (0U != (_syncData2 & (1U << (SyncBoolReq_AddressUint_Elevator1CalledToFloor + i))))
            {
                _elevator1FloorTargets_MASTER[i] = true;
                _elevator1FloorTargets_MASTER_COUNT++;
            }
            //If Elevator2 called to floor i
            if (0U != (_syncData2 & (1U << (SyncBoolReq_AddressUint_Elevator2CalledToFloor + i))))
            {
                _elevator2FloorTargets_MASTER[i] = true;
                _elevator2FloorTargets_MASTER_COUNT++;
            }
            //If floor has "Called Up" pressed
            if (0UL != (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_ElevatorCalledUp + i))))
            {
                _calledToFloorToGoUp_MASTER[i] = true;
                _calledToFloorToGoUp_MASTER_COUNT++;
            }
            //If floor has "Called Down" pressed
            if (0UL != (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_ElevatorCalledDown + i))))
            {
                _calledToFloorToGoDown_MASTER[i] = true;
                _calledToFloorToGoDown_MASTER_COUNT++;
            }
        }
        for (int i = 0; i <= 2; i++)
        {
            _timeAtCurrentFloorElevatorOpened_MASTER[i] = Time.time;
            _timeAtCurrentFloorElevatorClosed_MASTER[i] = Time.time;
        }
        _elevatorCheckTick_MASTER = 1;
    }
    /// <summary>
    /// The master runs this elevator controller in every Update()
    /// The load is splitted accross 3 frames to have a better performance
    /// </summary>
    private void MASTER_RunElevatorControl()
    {
        if (_elevator0Working && _elevatorCheckTick_MASTER == 1)
        {
            MASTER_RunElevator(0, _elevator0FloorTargets_MASTER);
        }
        if (_elevator1Working && _elevatorCheckTick_MASTER == 2)
        {
            MASTER_RunElevator(1, _elevator1FloorTargets_MASTER);
        }
        if (_elevator2Working && _elevatorCheckTick_MASTER == 3)
        {
            MASTER_RunElevator(2, _elevator2FloorTargets_MASTER);
        }
        _elevatorCheckTick_MASTER++;
        //TODO: Remove before pushing live
        if (false && _elevatorCheckTick_MASTER == 4)
        {
            if (_elevator0FloorTargets_MASTER_COUNT != 0)
                Debug.Log("_elevator0FloorTargets_MASTER_COUNT:" + _elevator0FloorTargets_MASTER_COUNT);
            if (_elevator1FloorTargets_MASTER_COUNT != 0)
                Debug.Log("_elevator1FloorTargets_MASTER_COUNT:" + _elevator1FloorTargets_MASTER_COUNT);
            if (_elevator2FloorTargets_MASTER_COUNT != 0)
                Debug.Log("_elevator2FloorTargets_MASTER_COUNT:" + _elevator2FloorTargets_MASTER_COUNT);
            if (_calledToFloorToGoUp_MASTER_COUNT != 0)
                Debug.Log("_calledToFloorToGoUp_MASTER_COUNT:" + _calledToFloorToGoUp_MASTER_COUNT);
            if (_calledToFloorToGoDown_MASTER_COUNT != 0)
                Debug.Log("_calledToFloorToGoDown_MASTER_COUNT:" + _calledToFloorToGoDown_MASTER_COUNT);
        }
        if (_elevatorCheckTick_MASTER >= 4)
            _elevatorCheckTick_MASTER = 1;
    }
    /// <summary>
    /// Running a single elevator, is only called by master in every Update
    /// 
    /// TODO: Moving down without internal target! (currently, setting an EREQ is a workaround for cheaper calculations)
    /// TODO: Ignore targets that generated fake-EREQs for other elevators
    /// 
    /// </summary>
    private void MASTER_RunElevator(int elevatorNumber, bool[] elevatorFloorTargets)
    {
        //Debug.Log("Elevator " + elevatorNumber + " has " + MASTER_GetInternalTargetCount(elevatorNumber) + " targets.");
        int currentFloor = GetSyncElevatorFloor(elevatorNumber);
        bool elevatorIdle;
        bool elevatorGoingUp;
        bool elevatorOpen;

        if (elevatorNumber == 0)
        {
            elevatorIdle = (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator0idle)));
            elevatorGoingUp = (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator0goingUp)));
            elevatorOpen = (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator0open)));
        }
        else if (elevatorNumber == 1)
        {
            elevatorIdle = (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator1idle)));
            elevatorGoingUp = (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator1goingUp)));
            elevatorOpen = (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator1open)));
        }
        else
        {
            elevatorIdle = (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator2idle)));
            elevatorGoingUp = (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator2goingUp)));
            elevatorOpen = (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator2open)));
        }

        bool targetFound = false;
        //we can't handle people blocking the elevator, so we will ignore ongoing requests and save them for later
        if (elevatorOpen)
        {
            //an elevator must stay open for n seconds
            if (!(currentFloor == 0) && Time.time - _timeAtCurrentFloorElevatorOpened_MASTER[elevatorNumber] > TIME_TO_STAY_OPEN || currentFloor == 0 && Time.time - _timeAtCurrentFloorElevatorOpened_MASTER[elevatorNumber] > TIME_TO_STAY_OPEN_RECEPTION)
            {
                Debug.Log("[NetworkController] Elevator " + elevatorNumber + " closing on floor " + currentFloor);
                //time to close this elevator
                MASTER_SetSyncValue(SyncBool_Elevator0open + elevatorNumber, false);
                _timeAtCurrentFloorElevatorClosed_MASTER[elevatorNumber] = Time.time;
                //closing it was enough for now
                return;
            }
            else
            {
                //while open, check if there is another internal target, else set the elevator idle
                if (!elevatorIdle && MASTER_GetInternalTargetCount(elevatorNumber) == 0)
                {
                    MASTER_SetElevatorIdle(elevatorNumber);
                    elevatorIdle = true;
                    elevatorGoingUp = false;
                }
                //handle all targets that are pointing in the current direction on the current floor
                MASTER_HandleFloorTarget(elevatorNumber, currentFloor, elevatorGoingUp, elevatorIdle);
                //we can't move an open elevator, so the code ends here
                return;
            }
        }
        else if (!_elevatorIsDriving_MASTER[elevatorNumber])
        {
            if (Time.time - _timeAtCurrentFloorElevatorClosed_MASTER[elevatorNumber] < TIME_TO_STAY_CLOSED)
            {
                //an elevator must stay closed for the duration of the closing animation
                //however, we could still process user-requests to open it again here
                //we can't move an elevator that isn't fully closed yet
                return;
            }
            else
            {
                //Doors closed and timeout exceeded. Set elevator to drive and block door requests
                _elevatorIsDriving_MASTER[elevatorNumber] = true;
            }
        }
        else if (Time.time - _timeAtCurrentFloorElevatorClosed_MASTER[elevatorNumber] < TIME_TO_DRIVE_ONE_FLOOR)
        {
            //driving a floor must take a certain amount of time
            return;
        }
        //an elevator that is going up will only handle internal targets and up requests
        if (!elevatorIdle && elevatorGoingUp)
        {
            //when the current floor was requested and we've arrived
            if (elevatorFloorTargets[currentFloor] || _calledToFloorToGoUp_MASTER[currentFloor])
            {
                Debug.Log("[NetworkController] Elevator " + elevatorNumber + " opening on floor " + currentFloor);
                MASTER_HandleFloorDoorOpening(elevatorNumber, currentFloor, elevatorGoingUp, elevatorIdle);
                //the code must end here since we just stopped the elevator
                return;
            }
            else if (MASTER_GetInternalTargetCount(elevatorNumber) != 0) //checking for next target
            {
                for (int i = currentFloor + 1; i <= 13; i++)
                {
                    if (elevatorFloorTargets[i]) //those are internal targets called from passengers which have priority
                    {
                        targetFound = true;
                        break;
                    }
                }
                if (targetFound) //this means that there is a target on the way up, so we drive one level up
                {
                    Debug.Log("[NetworkController] Elevator " + elevatorNumber + " driving up to floor " + (int)(currentFloor + 1));
                    MASTER_SetSyncElevatorFloor(elevatorNumber, currentFloor + 1);
                    _timeAtCurrentFloorElevatorClosed_MASTER[elevatorNumber] = Time.time; //resetting the timer for next floor
                                                                                          //the code must end here since we are now travelling further
                    return;
                }
                //since there is no internal target on the way up, we now check if there is one on the way down
                //first checking if there is a target on the way down (haha lol rip people on higher floors)
                for (int i = currentFloor - 1; i >= 0; i--)
                {
                    if (elevatorFloorTargets[i]) //those are internal targets called from passengers
                    {
                        targetFound = true;
                        break;
                    }
                }
                if (targetFound)
                {
                    // this means we are now reversing the elevator direction
                    MASTER_SetElevatorDirection(elevatorNumber, goingUp: false);
                    elevatorGoingUp = false;
                    // since the following code will handle this direction, we don't need to do anything else.
                }
            }
        }
        if (!elevatorIdle && !elevatorGoingUp) //this elevator can handle internal targets and external down-requests
        {
            //when the current floor was internally or externally requested and we've arrived
            if (elevatorFloorTargets[currentFloor] || _calledToFloorToGoDown_MASTER[currentFloor])
            {
                Debug.Log("[NetworkController] Elevator " + elevatorNumber + " opening on floor " + currentFloor);
                MASTER_HandleFloorDoorOpening(elevatorNumber, currentFloor, elevatorGoingUp, elevatorIdle);
                //the code must end here since we just opened the elevator
                return;
            }
            else if (MASTER_GetInternalTargetCount(elevatorNumber) != 0) //checking for next target
            {
                if (!targetFound)
                {
                    //first checking if there is a target on the way down
                    for (int i = currentFloor - 1; i >= 0; i--)
                    {
                        if (elevatorFloorTargets[i]) //those are internal targets called from passengers
                        {
                            targetFound = true;
                            break;
                        }
                    }
                }
                if (targetFound) //this means that there is a target on the way down, so we drive one level down
                {
                    Debug.Log("[NetworkController] Elevator " + elevatorNumber + " driving down to floor " + (int)(currentFloor - 1));
                    MASTER_SetSyncElevatorFloor(elevatorNumber, currentFloor - 1);
                    _timeAtCurrentFloorElevatorClosed_MASTER[elevatorNumber] = Time.time; //resetting the timer for next floor
                                                                                          //the code must end here since we are now travelling further
                    return;
                }
                //since there is no internal target on the way down, we now check if there is one on the way up
                for (int i = currentFloor + 1; i <= 13; i++)
                {
                    if (elevatorFloorTargets[i]) //those are internal targets called from passengers
                    {
                        targetFound = true;
                        break;
                    }
                }
                if (targetFound)
                {
                    // this means we are now reversing the elevator direction
                    MASTER_SetSyncValue(SyncBool_Elevator0goingUp + elevatorNumber, true);
                    // since the next loop code will handle this direction, we need to stop execution now
                    return;
                }
            }
        }
        //when we reach this line of code, the elevator found no internal target and is closed
        //when the current floor was requested by anyone and we are already there, we open the elevator
        if (elevatorFloorTargets[currentFloor] || _calledToFloorToGoUp_MASTER[currentFloor] || _calledToFloorToGoDown_MASTER[currentFloor])
        {
            Debug.Log("[NetworkController] Elevator " + elevatorNumber + " opening again on floor " + currentFloor);
            MASTER_HandleFloorDoorOpening(elevatorNumber, currentFloor, elevatorGoingUp, elevatorIdle);
            //the code must end here since we just stopped the elevator
            return;
        }
        //if we reach this line, we need to find the next target first
        int nextTarget = 0;
        if (MASTER_GetInternalTargetCount(elevatorNumber) != 0) //checking for next target
        {
            //Now we need to check if there is an internal target on the way up
            for (int i = currentFloor + 1; i <= 13; i++)
            {
                if (elevatorFloorTargets[i]) //those are all internal targets
                {
                    targetFound = true;
                    nextTarget = i;
                    break;
                }
            }
            //if we found an internal target, we go out of idle mode and set the new direction up
            if (targetFound)
            {
                Debug.Log("[NetworkController] Elevator " + elevatorNumber + " was idle but now has an internal target and is going up");
                MASTER_SetElevatorDirection(elevatorNumber, goingUp: true);
                return;
            }
            //Now we need to check if there is an internal target on the way down
            for (int i = currentFloor - 1; i >= 0; i--)
            {
                if (elevatorFloorTargets[i]) //those are internal targets called from passengers
                {
                    targetFound = true;
                    nextTarget = i;
                    break;
                }
            }
            //if we found an internal target, we go out of idle mode and set the new direction down
            if (targetFound)
            {
                Debug.Log("[NetworkController] Elevator " + elevatorNumber + " was idle but now has an internal target and is going down");
                MASTER_SetElevatorDirection(elevatorNumber, goingUp: false);
                return;
            }
        }
        //--------------------------------------------------------------------------------------------------
        if (_calledToFloorToGoUp_MASTER_COUNT != 0 || _calledToFloorToGoDown_MASTER_COUNT != 0) //checking for next target
        {
            //if we reach this code line, there is no internal target and we need to check external targets next
            for (int i = currentFloor + 1; i <= 13; i++)
            {
                if ((_calledToFloorToGoUp_MASTER[i] || _calledToFloorToGoDown_MASTER[i]) && !_floorHasFakeEREQ_MASTER[i])  //those are external targets
                {
                    targetFound = true;
                    nextTarget = i;
                    break;
                }
            }
            //if we found an internal target, we go out of idle mode and set the new direction up
            if (targetFound)
            {
                Debug.Log("[NetworkController] Elevator " + elevatorNumber + " was idle but now has an external target and is going up");
                MASTER_SetElevatorDirection(elevatorNumber, goingUp: true);
                //this elevator basicly belongs to that floor then, so both targets are handled, but this isn't perfect
                Debug.Log("[NetworkController] We're faking an EREQ next to set an internal target");
                ELREQ_SetInternalTarget(elevatorNumber, nextTarget);
                _floorHasFakeEREQ_MASTER[nextTarget] = true;
                return;
            }
            //Now we need to check if there is an external target on the way down
            for (int i = currentFloor - 1; i >= 0; i--)
            {
                if ((_calledToFloorToGoUp_MASTER[i] || _calledToFloorToGoDown_MASTER[i]) && !_floorHasFakeEREQ_MASTER[i]) //those are external targets
                {
                    targetFound = true;
                    nextTarget = i;
                    break;
                }
            }
            //if we found an internal target, we go out of idle mode and set the new direction down
            if (targetFound)
            {
                Debug.Log("[NetworkController] Elevator " + elevatorNumber + " was idle but now as an external target and is going down");
                MASTER_SetElevatorDirection(elevatorNumber, goingUp: false);
                //this elevator basicly belongs to that floor then, so both targets are handled, but this isn't perfect
                Debug.Log("[NetworkController] We're faking an EREQ next to set an internal target");
                ELREQ_SetInternalTarget(elevatorNumber, nextTarget);
                _floorHasFakeEREQ_MASTER[nextTarget] = true;
                return;
            }
        }
        //------------------------------------
        //reaching this code line means there is no next target and the elevator must go into idle mode
        if (!elevatorIdle)
        {
            Debug.Log("[NetworkController] Elevator " + elevatorNumber + " is now idle since there are no targets.");
            MASTER_SetElevatorIdle(elevatorNumber);
        }
    }
    /// <summary>
    /// Resets the elevator call button network when the elevator opens
    /// </summary>
    /// <param name="floor"></param>
    private void MASTER_HandleFloorDoorOpening(int elevatorNumber, int currentFloor, bool directionUp, bool isIdle)
    {
        //TODO: I tried to solve this issue here, please check/test
        //When the other directional button is pressed on that floor level and we should consider to handle it
        //and set this elevator to idle if the other button on that floor wasn't pressed and there is no internal target
        //on this way, so the elevator would reverse next. We also need to reverse the elevator in that case.
        //now we can actually open the elevator
        if (!isIdle && MASTER_GetInternalTargetCount(elevatorNumber) == 0)
        {
            //time to set it idle
            MASTER_SetElevatorIdle(elevatorNumber);
            isIdle = true; directionUp = false;
        }
        //preparing to check internal targets if there are any
        else if (!isIdle)
        {
            bool internalTargetAboveFound = false;
            bool internalTargetBelowFound = false;
            bool internalTargetOnThisLevel;
            bool[] elevatorFloorTargets;
            switch (elevatorNumber)
            {
                case 0:
                    elevatorFloorTargets = _elevator0FloorTargets_MASTER;
                    break;
                case 1:
                    elevatorFloorTargets = _elevator1FloorTargets_MASTER;
                    break;
                default:
                    elevatorFloorTargets = _elevator2FloorTargets_MASTER;
                    break;
            }
            internalTargetOnThisLevel = elevatorFloorTargets[currentFloor];
            if (internalTargetOnThisLevel && MASTER_GetInternalTargetCount(elevatorNumber) == 1)
            {
                //time to set it idle
                MASTER_SetElevatorIdle(elevatorNumber);
                isIdle = true; directionUp = false;
            }
            else //this means we have at least one internal target below or above
            {
                //we need to check if there is an internal target below
                for (int i = currentFloor - 1; i >= 0; i--)
                {
                    if (elevatorFloorTargets[i]) //those are internal targets called from passengers
                    {
                        internalTargetBelowFound = true;
                        break;
                    }
                }
                //we need to check if there is an internal target above
                for (int i = currentFloor + 1; i <= 13; i++)
                {
                    if (elevatorFloorTargets[i]) //those are internal targets called from passengers
                    {
                        internalTargetAboveFound = true;
                        break;
                    }
                }
                //now check if we need to reverse
                if ((directionUp && !internalTargetAboveFound) || (!directionUp && !internalTargetBelowFound))
                {
                    //this means we need to reverse
                    directionUp = !directionUp;
                    MASTER_SetElevatorDirection(elevatorNumber, directionUp);
                    MASTER_HandleFloorTarget(elevatorNumber, currentFloor, directionUp, isIdle);
                }
            }
        }
        //then handle the floor targets
        MASTER_HandleFloorTarget(elevatorNumber, currentFloor, directionUp, isIdle);
        MASTER_SetSyncValue(SyncBool_Elevator0open + elevatorNumber, true); //opening the elevator
        _elevatorIsDriving_MASTER[elevatorNumber] = false;
        _timeAtCurrentFloorElevatorOpened_MASTER[elevatorNumber] = Time.time;
    }
    /// <summary>
    /// Handles all floor targets on the current floor and direction
    /// </summary>
    /// <param name="floor"></param>
    private void MASTER_HandleFloorTarget(int elevatorNumber, int currentFloor, bool directionUp, bool isIdle)
    {
        if ((directionUp || isIdle) && _calledToFloorToGoUp_MASTER[currentFloor])
        {
            MASTER_SetSyncValue(SyncBoolReq_ElevatorCalledUp_0 + currentFloor, false);
            _calledToFloorToGoUp_MASTER[currentFloor] = false; //this target was now handled
            _calledToFloorToGoUp_MASTER_COUNT--;
        }
        if ((!directionUp || isIdle) && _calledToFloorToGoDown_MASTER[currentFloor])
        {
            MASTER_SetSyncValue(SyncBoolReq_ElevatorCalledDown_0 + currentFloor, false);
            _calledToFloorToGoDown_MASTER[currentFloor] = false; //this target was now handled
            _calledToFloorToGoDown_MASTER_COUNT--;
        }
        if (elevatorNumber == 0 && _elevator0FloorTargets_MASTER[currentFloor])
        {
            MASTER_SetSyncValue(SyncBoolReq_Elevator0CalledToFloor_0 + currentFloor, false);
            _elevator0FloorTargets_MASTER[currentFloor] = false;
            _elevator0FloorTargets_MASTER_COUNT--;
        }
        else if (elevatorNumber == 1 && _elevator1FloorTargets_MASTER[currentFloor])
        {
            MASTER_SetSyncValue(SyncBoolReq_Elevator1CalledToFloor_0 + currentFloor, false);
            _elevator1FloorTargets_MASTER[currentFloor] = false;
            _elevator1FloorTargets_MASTER_COUNT--;
        }
        else if (elevatorNumber == 2 && _elevator2FloorTargets_MASTER[currentFloor])
        {
            MASTER_SetSyncValue(SyncBoolReq_Elevator2CalledToFloor_0 + currentFloor, false);
            _elevator2FloorTargets_MASTER[currentFloor] = false;
            _elevator2FloorTargets_MASTER_COUNT--;
        }
        _floorHasFakeEREQ_MASTER[currentFloor] = false;
    }
    /// <summary>
    /// Sets the elevator travel direction
    /// </summary>
    private void MASTER_SetElevatorDirection(int elevatorNumber, bool goingUp)
    {
        MASTER_SetSyncValue(SyncBool_Elevator0goingUp + elevatorNumber, goingUp);
        //If elevator x is Idle
        if (0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXidle + elevatorNumber))))
        {
            MASTER_SetSyncValue(SyncBool_Elevator0idle + elevatorNumber, false);
            _timeAtCurrentFloorElevatorClosed_MASTER[elevatorNumber] = Time.time + TIME_TO_STAY_CLOSED - TIME_TO_STAY_CLOSED_AFTER_GOING_OUT_OF_IDLE;
        }
    }
    /// <summary>
    /// Setting the elevator in idle mode
    /// </summary>
    private void MASTER_SetElevatorIdle(int elevatorNumber)
    {
        MASTER_SetSyncValue(SyncBool_Elevator0goingUp + elevatorNumber, false);
        MASTER_SetSyncValue(SyncBool_Elevator0idle + elevatorNumber, true);
    }
    /// <summary>
    /// returns the number of internal floor targets for an elevator
    /// </summary>
    private int MASTER_GetInternalTargetCount(int elevatorNumber)
    {
        if (elevatorNumber == 0)
            return _elevator0FloorTargets_MASTER_COUNT;
        if (elevatorNumber == 1)
            return _elevator1FloorTargets_MASTER_COUNT;
        if (elevatorNumber == 2)
            return _elevator2FloorTargets_MASTER_COUNT;
        Debug.Log("ERROR: Unknown elevator number in MASTER_GetInternalTargetCount!");
        return 0; // to make the compiler happy
    }
    //-----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///  sets the elevators into the random state which is determined by master user uwu
    ///  this can't be "random", but we have a pool of 7 allowed states
    /// </summary>
    private void MASTER_SetConstSceneElevatorStates()
    {
        //to make testing easier, we only allow one state right now
        int random = 0; // UnityEngine.Random.Range(0, 7);
        switch (random)
        {
            case 0:
                MASTER_SetSyncValue(SyncBool_Elevator0working, true);
                MASTER_SetSyncValue(SyncBool_Elevator1working, true);
                MASTER_SetSyncValue(SyncBool_Elevator2working, true);
                break;
            case 1:
                MASTER_SetSyncValue(SyncBool_Elevator0working, false);
                MASTER_SetSyncValue(SyncBool_Elevator1working, true);
                MASTER_SetSyncValue(SyncBool_Elevator2working, true);
                break;
            case 2:
                MASTER_SetSyncValue(SyncBool_Elevator0working, true);
                MASTER_SetSyncValue(SyncBool_Elevator1working, false);
                MASTER_SetSyncValue(SyncBool_Elevator2working, true);
                break;
            case 3:
                MASTER_SetSyncValue(SyncBool_Elevator0working, true);
                MASTER_SetSyncValue(SyncBool_Elevator1working, true);
                MASTER_SetSyncValue(SyncBool_Elevator2working, false);
                break;
            case 4:
                MASTER_SetSyncValue(SyncBool_Elevator0working, true);
                MASTER_SetSyncValue(SyncBool_Elevator1working, false);
                MASTER_SetSyncValue(SyncBool_Elevator2working, false);
                break;
            case 5:
                MASTER_SetSyncValue(SyncBool_Elevator0working, false);
                MASTER_SetSyncValue(SyncBool_Elevator1working, true);
                MASTER_SetSyncValue(SyncBool_Elevator2working, false);
                break;
            default:
                MASTER_SetSyncValue(SyncBool_Elevator0working, false);
                MASTER_SetSyncValue(SyncBool_Elevator1working, false);
                MASTER_SetSyncValue(SyncBool_Elevator2working, true);
                break;
        }
        MASTER_SetSyncValue(SyncBool_Initialized, true);
        Debug.Log("[NetworkController] Random elevator states are now set by master");
    }
    #endregion MASTER_FUNCTIONS
    //------------------------------------------------------------------------------------------------------------
    //----------------------------------- LOCAL functions --------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------
    #region LOCAL_FUNCTIONS
    /// <summary>
    /// elevator request states, synced by master
    /// </summary>
    private ulong _localSyncData1 = 0;
    private uint _localSyncData2 = 0;
    private bool[] _localSyncDataBools = new bool[84];
    /// <summary>
    /// The ulong maps as follows:-
    ///  - 63-60 variable_1 (4bits)
    ///  - 59-56 variable_2 (4bits)
    ///  - 55-52 variable_3 (4bits)
    ///  - 0-51 binary bools [0-51]
    ///
    /// The uint maps as follows:-
    ///  - 0-31 binary bools [52-83(?)]
    ///  
    /// Is run every time a network packet is received by localPlayer
    /// </summary>
    private void LOCAL_CheckSyncData()
    {
        //check if something from this synced var has changed
        if (_syncData1 != _localSyncData1)
        {
            //position 52 to position 63 are floor levels that might have changed
            LOCAL_CheckElevatorLevels();
            bool[] cachedSync1Bools = GetBoolArrayUlongONLY();
            //the positions 0-51 are binary bools that might have changed
            for (int i = 0; i < 52; i++) //no need to check bool 0
            {
                if (cachedSync1Bools[i] != _localSyncDataBools[i])
                {
                    LOCAL_HandleSyncBoolChanged(i);
                }
            }
            //store new local sync data 1
            _localSyncData1 = _syncData1;
        }
        //check if something from this synced var has changed
        if (_syncData2 != _localSyncData2)
        {
            bool[] cachedSync2Bools = GetBoolArrayUintONLY();
            //the positions 0-31 are binary bools that might have changed (position 52-83)
            for (int i = 52; i < 84; i++)
            {
                if (cachedSync2Bools[i] != _localSyncDataBools[i])
                {
                    LOCAL_HandleSyncBoolChanged(i);
                }
            }
            //store new local sync data 2
            _localSyncData2 = _syncData2;
        }
    }
    /// <summary>
    /// Is called when the syncbool on position <see cref="syncBoolPosition"/> has changed
    /// </summary>
    private void LOCAL_HandleSyncBoolChanged(int syncBoolPosition)
    {
        //read the new state
        bool newState = !_localSyncDataBools[syncBoolPosition];
        //change the locally known state to it
        _localSyncDataBools[syncBoolPosition] = newState;
        //adjust the scene elements to the new state
        switch (syncBoolPosition)
        {
            case SyncBool_Elevator0open:
                LOCAL_OpenCloseElevator(0, setOpen: newState);
                break;
            case SyncBool_Elevator1open:
                LOCAL_OpenCloseElevator(1, setOpen: newState);
                break;
            case SyncBool_Elevator2open:
                LOCAL_OpenCloseElevator(2, setOpen: newState);
                break;
            case SyncBool_Elevator0idle:
                LOCAL_SetElevatorIdle(0, isIdle: newState);
                break;
            case SyncBool_Elevator1idle:
                LOCAL_SetElevatorIdle(1, isIdle: newState);
                break;
            case SyncBool_Elevator2idle:
                LOCAL_SetElevatorIdle(2, isIdle: newState);
                break;
            case SyncBool_Elevator0goingUp:
                LOCAL_SetElevatorDirection(0, goingUp: newState);
                break;
            case SyncBool_Elevator1goingUp:
                LOCAL_SetElevatorDirection(1, goingUp: newState);
                break;
            case SyncBool_Elevator2goingUp:
                LOCAL_SetElevatorDirection(2, goingUp: newState);
                break;
            //case SyncBoolReq_ElevatorCalled#J2#_#1#:
            //LOCAL_SetElevatorCallButtonState(#1#, buttonUp: #J1#, called: newState);
            //break;
            case SyncBoolReq_ElevatorCalledUp_0:
                LOCAL_SetElevatorCallButtonState(0, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_1:
                LOCAL_SetElevatorCallButtonState(1, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_2:
                LOCAL_SetElevatorCallButtonState(2, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_3:
                LOCAL_SetElevatorCallButtonState(3, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_4:
                LOCAL_SetElevatorCallButtonState(4, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_5:
                LOCAL_SetElevatorCallButtonState(5, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_6:
                LOCAL_SetElevatorCallButtonState(6, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_7:
                LOCAL_SetElevatorCallButtonState(7, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_8:
                LOCAL_SetElevatorCallButtonState(8, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_9:
                LOCAL_SetElevatorCallButtonState(9, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_10:
                LOCAL_SetElevatorCallButtonState(10, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_11:
                LOCAL_SetElevatorCallButtonState(11, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_12:
                LOCAL_SetElevatorCallButtonState(12, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledUp_13:
                LOCAL_SetElevatorCallButtonState(13, buttonUp: true, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_0:
                LOCAL_SetElevatorCallButtonState(0, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_1:
                LOCAL_SetElevatorCallButtonState(1, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_2:
                LOCAL_SetElevatorCallButtonState(2, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_3:
                LOCAL_SetElevatorCallButtonState(3, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_4:
                LOCAL_SetElevatorCallButtonState(4, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_5:
                LOCAL_SetElevatorCallButtonState(5, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_6:
                LOCAL_SetElevatorCallButtonState(6, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_7:
                LOCAL_SetElevatorCallButtonState(7, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_8:
                LOCAL_SetElevatorCallButtonState(8, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_9:
                LOCAL_SetElevatorCallButtonState(9, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_10:
                LOCAL_SetElevatorCallButtonState(10, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_11:
                LOCAL_SetElevatorCallButtonState(11, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_12:
                LOCAL_SetElevatorCallButtonState(12, buttonUp: false, isCalled: newState);
                break;
            case SyncBoolReq_ElevatorCalledDown_13:
                LOCAL_SetElevatorCallButtonState(13, buttonUp: false, isCalled: newState);
                break;
            //case SyncBoolReq_Elevator#J1#CalledToFloor_#1#:
            //    LOCAL_SetElevatorInternalButtonState(#J1#,#1#, called: newState);
            //    break;
            case SyncBoolReq_Elevator0CalledToFloor_0:
                LOCAL_SetElevatorInternalButtonState(0, 4, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_1:
                LOCAL_SetElevatorInternalButtonState(0, 5, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_2:
                LOCAL_SetElevatorInternalButtonState(0, 6, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_3:
                LOCAL_SetElevatorInternalButtonState(0, 7, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_4:
                LOCAL_SetElevatorInternalButtonState(0, 8, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_5:
                LOCAL_SetElevatorInternalButtonState(0, 9, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_6:
                LOCAL_SetElevatorInternalButtonState(0, 10, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_7:
                LOCAL_SetElevatorInternalButtonState(0, 11, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_8:
                LOCAL_SetElevatorInternalButtonState(0, 12, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_9:
                LOCAL_SetElevatorInternalButtonState(0, 13, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_10:
                LOCAL_SetElevatorInternalButtonState(0, 14, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_11:
                LOCAL_SetElevatorInternalButtonState(0, 15, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_12:
                LOCAL_SetElevatorInternalButtonState(0, 16, called: newState);
                break;
            case SyncBoolReq_Elevator0CalledToFloor_13:
                LOCAL_SetElevatorInternalButtonState(0, 17, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_0:
                LOCAL_SetElevatorInternalButtonState(1, 4, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_1:
                LOCAL_SetElevatorInternalButtonState(1, 5, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_2:
                LOCAL_SetElevatorInternalButtonState(1, 6, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_3:
                LOCAL_SetElevatorInternalButtonState(1, 7, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_4:
                LOCAL_SetElevatorInternalButtonState(1, 8, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_5:
                LOCAL_SetElevatorInternalButtonState(1, 9, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_6:
                LOCAL_SetElevatorInternalButtonState(1, 10, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_7:
                LOCAL_SetElevatorInternalButtonState(1, 11, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_8:
                LOCAL_SetElevatorInternalButtonState(1, 12, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_9:
                LOCAL_SetElevatorInternalButtonState(1, 13, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_10:
                LOCAL_SetElevatorInternalButtonState(1, 14, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_11:
                LOCAL_SetElevatorInternalButtonState(1, 15, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_12:
                LOCAL_SetElevatorInternalButtonState(1, 16, called: newState);
                break;
            case SyncBoolReq_Elevator1CalledToFloor_13:
                LOCAL_SetElevatorInternalButtonState(1, 17, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_0:
                LOCAL_SetElevatorInternalButtonState(2, 4, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_1:
                LOCAL_SetElevatorInternalButtonState(2, 5, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_2:
                LOCAL_SetElevatorInternalButtonState(2, 6, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_3:
                LOCAL_SetElevatorInternalButtonState(2, 7, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_4:
                LOCAL_SetElevatorInternalButtonState(2, 8, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_5:
                LOCAL_SetElevatorInternalButtonState(2, 9, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_6:
                LOCAL_SetElevatorInternalButtonState(2, 10, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_7:
                LOCAL_SetElevatorInternalButtonState(2, 11, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_8:
                LOCAL_SetElevatorInternalButtonState(2, 12, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_9:
                LOCAL_SetElevatorInternalButtonState(2, 13, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_10:
                LOCAL_SetElevatorInternalButtonState(2, 14, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_11:
                LOCAL_SetElevatorInternalButtonState(2, 15, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_12:
                LOCAL_SetElevatorInternalButtonState(2, 16, called: newState);
                break;
            case SyncBoolReq_Elevator2CalledToFloor_13:
                LOCAL_SetElevatorInternalButtonState(2, 17, called: newState);
                break;
            case 80: //initialized-bool
            case 81: //elevator1-working
            case 82: //elevator2-working
            case 83: //elevator3-working
                break;
            default:
                Debug.Log("ERROR: UNKNOWN BOOL HAS CHANGED IN SYNCBOOL, position: " + syncBoolPosition);
                break;
        }
    }
    /// <summary>
    /// Storing locally known elevator levels
    /// </summary>
    private int _localElevator0Level = 15;
    private int _localElevator1Level = 15;
    private int _localElevator2Level = 15;
    /// <summary>
    /// Checking all elevatorlevels if they have changed from master
    /// </summary>
    private void LOCAL_CheckElevatorLevels()
    {
        if (_elevator0Working && _localElevator0Level != GetSyncElevatorFloor(0))
        {
            int floorNumber = GetSyncElevatorFloor(0);
            _elevatorControllerReception.SetElevatorLevelOnDisplay(floorNumber, 0);
            _elevatorControllerArrivalArea.SetElevatorLevelOnDisplay(floorNumber, 0);
            _localElevator0Level = floorNumber;
        }
        if (_elevator1Working && _localElevator1Level != GetSyncElevatorFloor(1))
        {
            int floorNumber = GetSyncElevatorFloor(1);
            _elevatorControllerReception.SetElevatorLevelOnDisplay(floorNumber, 1);
            _elevatorControllerArrivalArea.SetElevatorLevelOnDisplay(floorNumber,1);
            _localElevator1Level = floorNumber;
        }
        if (_elevator2Working && _localElevator2Level != GetSyncElevatorFloor(2))
        {
            int floorNumber = GetSyncElevatorFloor(2);
            _elevatorControllerReception.SetElevatorLevelOnDisplay(floorNumber, 2);
            _elevatorControllerArrivalArea.SetElevatorLevelOnDisplay(floorNumber, 2);
            _localElevator2Level = floorNumber;
        }
    }
    /// <summary>
    /// Setting a button inside an elevator to a different state
    /// </summary>
    private void LOCAL_SetElevatorInternalButtonState(int elevatorNumber, int buttonNumber, bool called)
    {
        if(_userIsInVR)
        { 
            switch(elevatorNumber)
            {
                case 0:
                    _InsidePanelScriptElevatorForVR_0.SetElevatorInternalButtonState(buttonNumber, called);
                    break;
                case 1:
                    _InsidePanelScriptElevatorForVR_1.SetElevatorInternalButtonState(buttonNumber, called);
                    break;
                case 2:
                    _InsidePanelScriptElevatorForVR_2.SetElevatorInternalButtonState(buttonNumber, called);
                    break;
            }
        }
        else
        {
            switch (elevatorNumber)
            {
                case 0:
                    _insidePanelScriptElevatorDesktop_0.SetElevatorInternalButtonState(buttonNumber, called);
                    break;
                case 1:
                    _insidePanelScriptElevatorDesktop_1.SetElevatorInternalButtonState(buttonNumber, called);
                    break;
                case 2:
                    _insidePanelScriptElevatorDesktop_2.SetElevatorInternalButtonState(buttonNumber, called);
                    break;
            }
        }
    }
    /// <summary>
    /// When a state of a floor callbutton changed, we need to update that button to on or off
    /// </summary>
    private void LOCAL_SetElevatorCallButtonState(int floor, bool buttonUp, bool isCalled)
    {
        if (floor == 0)
        {
            _elevatorControllerReception.SetCallButtonState(buttonUp, isCalled);
        }
        else if (floor == _localPlayerCurrentFloor)
        {
            _elevatorControllerArrivalArea.SetCallButtonState(buttonUp, isCalled);
        }
    }
    /// <summary>
    /// Is run ONCE by localPlayer on scene load.
    /// Setting up the scene at startup or when it isn't setup yet
    /// </summary>
    private void LOCAL_ReadConstSceneElevatorStates()
    {
        Debug.Log("[NetworkController] Setting random elevator states for reception by localPlayer");
        _elevator0Working = 0U != (_syncData2 & (SyncBool_MaskUint_Elevator0working));
        _elevator1Working = 0U != (_syncData2 & (SyncBool_MaskUint_Elevator1working));
        _elevator2Working = 0U != (_syncData2 & (SyncBool_MaskUint_Elevator2working));
        _elevatorControllerReception._elevator1working = _elevator0Working;
        _elevatorControllerReception._elevator2working = _elevator1Working;
        _elevatorControllerReception._elevator3working = _elevator2Working;
        _elevatorControllerReception.SetupElevatorStates();
        _elevatorControllerArrivalArea._elevator1working = _elevator0Working;
        _elevatorControllerArrivalArea._elevator2working = _elevator1Working;
        _elevatorControllerArrivalArea._elevator3working = _elevator2Working;
        _elevatorControllerArrivalArea.SetupElevatorStates();
        //TODO: Take those just-set bits into the local copy (not needed to work but would be nice to track errors)
        Debug.Log("[NetworkController] Random elevator states for reception are now set by localPlayer");
    }
    /// <summary>
    /// is called when network packets are received (only happens when there are more players except Master in the scene
    /// </summary>
    public override void OnDeserialization()
    {
        if (!_worldIsLoaded)
            return;
        LOCAL_OnDeserialization(); //do nothing else in here or shit will break!
    }
    /// <summary>
    /// Can be called by master (locally in Update) or by everyone else OnDeserialization (when SyncBool states change)
    /// </summary>
    private void LOCAL_OnDeserialization()
    {
        if (!_finishedLocalSetup)
        {
            if (Time.time < 1f) //no scene setup before at least 1 second has passed to ensure the update loop has already started
                return;
            Debug.Log("[NetworkController] Local setup was started");
            if (0U != (_syncData2 & (SyncBool_MaskUint_Initialized)))
            {
                LOCAL_ReadConstSceneElevatorStates();
                _finishedLocalSetup = true;
                Debug.Log("[NetworkController] Local setup was finished");
            }
            else
            {
                return;
            }
        }
        else
        {
            //WORKAROUND for UInt64 not working
            if (_syncData1forReal != _syncData1forRealLocalCopyForSyncCheck)
            {
                Debug.Log("[NetworkController] _syncData1forReal has changed!");
                _syncData1 = (ulong)_syncData1forReal; //received from master
                _syncData1forRealLocalCopyForSyncCheck = _syncData1forReal;
            }
            LOCAL_CheckSyncData();
        }
    }
    /// <summary>
    /// Sending an elevator open/close event to the elevator controller of the right floor
    /// </summary>
    /// <param name="elevatorNumber"></param>
    private void LOCAL_OpenCloseElevator(int elevatorNumber, bool setOpen)
    {
        int floorNumber = GetSyncElevatorFloor(elevatorNumber);
        if (setOpen)
        {
            Debug.Log("[NetworkController] LocalPlayer received to open elevator " + elevatorNumber + " on floor " + floorNumber);
            if (floorNumber == 0)
            {
                //Passes elevatorNumber, (if going up), (if idle)
                _elevatorControllerReception.OpenElevator(elevatorNumber, 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXgoingUp + elevatorNumber))), 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXidle + elevatorNumber))));
            }
            else if (floorNumber == _localPlayerCurrentFloor)
            {
                //Passes elevatorNumber, (if going up), (if idle)
                _elevatorControllerArrivalArea.OpenElevator(elevatorNumber, 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXgoingUp + elevatorNumber))), 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXidle + elevatorNumber))));
            }
        }
        else
        {
            Debug.Log("[NetworkController] LocalPlayer received to close elevator " + elevatorNumber + " on floor " + floorNumber);
            if (floorNumber == 0)
            {
                _elevatorControllerReception.CloseElevator(elevatorNumber);
            }
            else if (floorNumber == _localPlayerCurrentFloor)
            {
                _elevatorControllerArrivalArea.CloseElevator(elevatorNumber);
            }
        }
    }
    /// <summary>
    /// Setting an elevator idle will set all calls handled on that floor and set both arrows active, if the elevator is open, else nothing happens
    /// </summary>
    /// <param name="elevatorNumber"></param>
    private void LOCAL_SetElevatorIdle(int elevatorNumber, bool isIdle)
    {
        //If elevator NOT open
        if (0UL == (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXopen + elevatorNumber))))
        {
            Debug.Log("[NetworkController] LocalPlayer received to set elevator " + elevatorNumber + " IDLE=" + isIdle.ToString() + ", but it isn't open");
            return;
        }
        int floor = GetSyncElevatorFloor(elevatorNumber);
        Debug.Log("[NetworkController] LocalPlayer received to set elevator " + elevatorNumber + " IDLE=" + isIdle.ToString() + " on floor " + floor);
        if (floor == 0)
        {
            //Passes elevatorNumber, isGoingUp, isIdle
            _elevatorControllerReception.SetElevatorDirectionDisplay(elevatorNumber, 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXgoingUp + elevatorNumber))), isIdle);
        }
        else if (floor == _localPlayerCurrentFloor)
        {
            //Passes elevatorNumber, isGoingUp, isIdle
            _elevatorControllerArrivalArea.SetElevatorDirectionDisplay(elevatorNumber, 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXgoingUp + elevatorNumber))), isIdle);
        }
    }
    /// <summary>
    /// Setting the new elevator direction will affect button calls and arrows if the elevator is open
    /// </summary>
    private void LOCAL_SetElevatorDirection(int elevatorNumber, bool goingUp)
    {
        //If elevator NOT open
        if (0UL == (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXopen + elevatorNumber))))
        {
            Debug.Log("[NetworkController] LocalPlayer received to set elevator " + elevatorNumber + " GoingUp=" + goingUp.ToString() + ", but it isn't open");
            return;
        }
        int floor = GetSyncElevatorFloor(elevatorNumber);
        Debug.Log("[NetworkController] LocalPlayer received to set elevator " + elevatorNumber + " GoingUp=" + goingUp.ToString() + " on floor " + floor);
        if (floor == 0)
        {
            //Passes elevatorNumber, goingUp, (if idle)
            _elevatorControllerReception.SetElevatorDirectionDisplay(elevatorNumber, goingUp, 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXidle + elevatorNumber))));
        }
        else if (floor == _localPlayerCurrentFloor)
        {
            //Passes elevatorNumber, goingUp, (if idle)
            _elevatorControllerArrivalArea.SetElevatorDirectionDisplay(elevatorNumber, goingUp, 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXidle + elevatorNumber))));
        }
    }
    //------------------------------------- external elevator calls from floor buttons ------------------------------------------------
    //Local copies of elevator state to check them against SyncBool states
    private bool[] _pendingCallDown_LOCAL_EXT = new bool[14];
    private int _pendingCallDown_COUNT_LOCAL_EXT = 0;
    private bool[] _pendingCallUp_LOCAL_EXT = new bool[14];
    private int _pendingCallUp_COUNT_LOCAL_EXT = 0;
    private float[] _pendingCallTimeUp_LOCAL_EXT = new float[14];
    private float[] _pendingCallTimeDown_LOCAL_EXT = new float[14];
    /// <summary>
    /// In every Update(): Checking if the external elevator was successfully called by master after we've called it, else we drop the request
    /// </summary>
    private void LOCAL_CheckIfElevatorExternalCallWasReceived()
    {
        if (_pendingCallUp_COUNT_LOCAL_EXT != 0)
        {
            //Debug.Log("There is " + _pendingCallUp_COUNT + " pending call up.");
            for (int floor = 0; floor <= 13; floor++)
            {
                //Check if there is a pending request
                if (_pendingCallUp_LOCAL_EXT[floor] && Time.time - _pendingCallTimeUp_LOCAL_EXT[floor] > 1.5f)
                {
                    _pendingCallUp_LOCAL_EXT[floor] = false;
                    _pendingCallUp_COUNT_LOCAL_EXT--;
                    //if NOT called up to floor X
                    if (0UL == (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_ElevatorCalledUp + floor))))
                    {
                        //TODO: link all elevator controllers here in Unity later
                        if (floor == 0)
                        {
                            Debug.Log("Dropped request, SetElevatorNotCalledUp() floor " + floor + " after " + (Time.time - _pendingCallTimeUp_LOCAL_EXT[floor]).ToString() + " seconds.");
                            _elevatorControllerReception.SetCallButtonState(buttonUp: true, isCalled: false);
                        }
                        else if (floor == _localPlayerCurrentFloor)
                        {
                            Debug.Log("Dropped request, SetElevatorNotCalledUp() floor " + floor + " after " + (Time.time - _pendingCallTimeUp_LOCAL_EXT[floor]).ToString() + " seconds.");
                            _elevatorControllerArrivalArea.SetCallButtonState(buttonUp: true, isCalled: false);
                        }
                    }
                }
            }
        }
        if (_pendingCallDown_COUNT_LOCAL_EXT != 0)
        {
            //Debug.Log("There is " + _pendingCallUp_COUNT + " pending call down.");
            for (int floor = 0; floor <= 13; floor++)
            {
                if (_pendingCallDown_LOCAL_EXT[floor] && Time.time - _pendingCallTimeDown_LOCAL_EXT[floor] > 1.5f)
                {
                    _pendingCallDown_LOCAL_EXT[floor] = false;
                    _pendingCallDown_COUNT_LOCAL_EXT--;
                    //if NOT called down to floor X
                    if (0UL == (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_ElevatorCalledDown + floor))))
                    {
                        //TODO: link all elevator controllers here in Unity later
                        if (floor == 0)
                        {
                            Debug.Log("Dropped request, SetElevatorNotCalledDown() floor " + floor + " after " + (Time.time - _pendingCallTimeDown_LOCAL_EXT[floor]).ToString() + " seconds.");
                            _elevatorControllerReception.SetCallButtonState(buttonUp: false, isCalled: false);
                        }
                        else if (floor == _localPlayerCurrentFloor)
                        {
                            Debug.Log("Dropped request, SetElevatorNotCalledDown() floor " + floor + " after " + (Time.time - _pendingCallTimeDown_LOCAL_EXT[floor]).ToString() + " seconds.");
                            _elevatorControllerArrivalArea.SetCallButtonState(buttonUp: false, isCalled: false);
                        }
                    }
                }
            }
        }
    }
    //------------------------------------- internal elevator calls from elevator buttons ------------------------------------------------
    //Local copies of elevator state to check them against SyncBool states
    private bool[] _pendingCallElevator0_LOCAL_INT = new bool[14];
    private int _pendingCallElevator0_COUNT_LOCAL_INT = 0;
    private float[] _pendingCallElevator0Time_LOCAL_INT = new float[14];
    //Local copies of elevator state to check them against SyncBool states
    private bool[] _pendingCallElevator1_LOCAL_INT = new bool[14];
    private int _pendingCallElevator1_COUNT_LOCAL_INT = 0;
    private float[] _pendingCallElevator1Time_LOCAL_INT = new float[14];
    //Local copies of elevator state to check them against SyncBool states
    private bool[] _pendingCallElevator2_LOCAL_INT = new bool[14];
    private int _pendingCallElevator2_COUNT_LOCAL_INT = 0;
    private float[] _pendingCallElevator2Time_LOCAL_INT = new float[14];
    /// <summary>
    /// In every Update(): Checking if the INTernal elevator was successfully called by master after we've called it, else we drop the request
    /// </summary>
    private void LOCAL_CheckIfElevatorInternalCallWasReceived()
    {
        if (_pendingCallElevator0_COUNT_LOCAL_INT != 0)
        {
            //Debug.Log("There is " + _pendingCallElevator0_COUNT + " pending call internally.");
            for (int floor = 0; floor <= 13; floor++)
            {
                if (_pendingCallElevator0_LOCAL_INT[floor] && Time.time - _pendingCallElevator0Time_LOCAL_INT[floor] > 1.5f)
                {
                    _pendingCallElevator0_LOCAL_INT[floor] = false;
                    _pendingCallElevator0_COUNT_LOCAL_INT--;
                    if (0UL == (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_Elevator0CalledToFloor + floor))))
                    {
                        Debug.Log("Dropped request, SetElevatorInternalButtonState() button " + floor + " after " + (Time.time - _pendingCallElevator0Time_LOCAL_INT[floor]).ToString() + " seconds.");
                        LOCAL_SetElevatorInternalButtonState(0, floor + 4, called: false);
                    }
                }
            }
        }
        if (_pendingCallElevator1_COUNT_LOCAL_INT != 0)
        {
            //Debug.Log("There is " + _pendingCallElevator1_COUNT + " pending call internally.");
            for (int floor = 0; floor <= 13; floor++)
            {
                if (_pendingCallElevator1_LOCAL_INT[floor] && Time.time - _pendingCallElevator1Time_LOCAL_INT[floor] > 1.5f)
                {
                    _pendingCallElevator1_LOCAL_INT[floor] = false;
                    _pendingCallElevator1_COUNT_LOCAL_INT--;

                    //if NOT elevator1 called to floor X
                    if (0U == (_syncData2 & (1U << (SyncBoolReq_AddressUint_Elevator1CalledToFloor + floor))))
                    {
                        Debug.Log("Dropped request, SetElevatorInternalButtonState() button " + floor + " after " + (Time.time - _pendingCallElevator1Time_LOCAL_INT[floor]).ToString() + " seconds.");
                        LOCAL_SetElevatorInternalButtonState(0, floor+4, called: false);
                    }
                }
            }
        }
        if (_pendingCallElevator2_COUNT_LOCAL_INT != 0)
        {
            //Debug.Log("There is " + _pendingCallElevator2_COUNT + " pending call internally.");
            for (int floor = 0; floor <= 13; floor++)
            {
                if (_pendingCallElevator2_LOCAL_INT[floor] && Time.time - _pendingCallElevator2Time_LOCAL_INT[floor] > 1.5f)
                {
                    _pendingCallElevator2_LOCAL_INT[floor] = false;
                    _pendingCallElevator2_COUNT_LOCAL_INT--;
                    //if NOT elevator0 called to floor X
                    if (0U == (_syncData2 & (1U << (SyncBoolReq_AddressUint_Elevator2CalledToFloor + floor))))
                    {
                        Debug.Log("Dropped request, SetElevatorInternalButtonState() button " + floor + " after " + (Time.time - _pendingCallElevator2Time_LOCAL_INT[floor]).ToString() + " seconds.");
                        LOCAL_SetElevatorInternalButtonState(0, floor+4, called: false);
                    }
                }
            }
        }
    }
    #endregion LOCAL_FUNCTIONS
    //------------------------------------------------------------------------------------------------------------
    //------------------------------- API for elevator buttons ---------------------------------------------------
    //------------------------------------------------------------------------------------------------------------
    #region API_FUNCTIONS
    public void API_LocalPlayerPressedCallButton(int floorNumber, bool directionUp)
    {
        if (directionUp)
        {
            Debug.Log("[NetworkController] Elevator called to floor " + floorNumber + " by localPlayer (Up)");
            //if something with an array OR Elevator called up on floor X
            if (_pendingCallUp_LOCAL_EXT[floorNumber] || 0UL != (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_ElevatorCalledUp + floorNumber))))
                return;
            _pendingCallUp_LOCAL_EXT[floorNumber] = true;
            _pendingCallTimeUp_LOCAL_EXT[floorNumber] = Time.time;
            _pendingCallUp_COUNT_LOCAL_EXT++;
            _elevatorRequester.RequestElevatorFloorButton(directionUp, floorNumber);
        }
        else
        {
            Debug.Log("[NetworkController] Elevator called to floor " + floorNumber + " by localPlayer (Down)");
            //if something with an array OR Elevator called down on floor X
            if (_pendingCallDown_LOCAL_EXT[floorNumber] || 0UL != (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_ElevatorCalledDown + floorNumber))))
                return;
            _pendingCallDown_LOCAL_EXT[floorNumber] = true;
            _pendingCallTimeDown_LOCAL_EXT[floorNumber] = Time.time;
            _pendingCallDown_COUNT_LOCAL_EXT++;
            _elevatorRequester.RequestElevatorFloorButton(directionUp, floorNumber);
        }
    }
    private bool _elevatorLoliStairsAreEnabled = false;
    private bool _elevatorMirrorIsEnabled = false;
    /// <summary>
    /// When localPlayer pressed a button INSIDE the elevator
    /// </summary>
    public void API_LocalPlayerPressedElevatorButton(int elevatorNumber, int buttonNumber)
    {
        Debug.Log($"[NetworkController] LocalPlayer pressed button {buttonNumber} in elevator {elevatorNumber}");
        if (buttonNumber == 0) //OPEN
        {
            //If NOT elevator X open
            if (0UL == (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXopen + elevatorNumber))))
            {
                _elevatorRequester.RequestElevatorDoorStateChange(elevatorNumber, true);
            }
            return;
        }
        if (buttonNumber == 1) //CLOSE
        {
            //If elevator X open
            if (0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXopen + elevatorNumber))))
            {
                _elevatorRequester.RequestElevatorDoorStateChange(elevatorNumber, false);
            }
            return;
        }
        if (buttonNumber == 2) // (m2) mirror-button
        {
            //toggle loli stairs locally in both ElevatorControllers
            _elevatorControllerReception.ToggleLoliStairs(elevatorNumber);
            _elevatorControllerArrivalArea.ToggleLoliStairs(elevatorNumber);
            _elevatorLoliStairsAreEnabled = !_elevatorLoliStairsAreEnabled;
            LOCAL_SetElevatorInternalButtonState(elevatorNumber, buttonNumber, called: _elevatorLoliStairsAreEnabled);
            return;
        }
        if (buttonNumber == 3) // (m1) loli-stairs button
        {
            //toggle mirror locally in both ElevatorControllers
            _elevatorControllerReception.ToggleMirror(elevatorNumber);
            _elevatorControllerArrivalArea.ToggleMirror(elevatorNumber);
            _elevatorMirrorIsEnabled = !_elevatorMirrorIsEnabled;
            LOCAL_SetElevatorInternalButtonState(elevatorNumber, buttonNumber, called: _elevatorMirrorIsEnabled);
            return;
        }
        if (buttonNumber == 18) // RING-button
        {
            //This button does absolutely nothing atm lol
            return;
        }
        //every other button is an internal floor request, button 4 is floor 0 etc.
        int floorNumber = buttonNumber - 4;
        switch (elevatorNumber)
        {
            case 0:
                _pendingCallElevator0_LOCAL_INT[floorNumber] = true; ;
                _pendingCallElevator0_COUNT_LOCAL_INT++;
                _pendingCallElevator0Time_LOCAL_INT[floorNumber] = Time.time;
                break;
            case 1:
                _pendingCallElevator1_LOCAL_INT[floorNumber] = true; ;
                _pendingCallElevator1_COUNT_LOCAL_INT++;
                _pendingCallElevator1Time_LOCAL_INT[floorNumber] = Time.time;
                break;
            case 2:
                _pendingCallElevator2_LOCAL_INT[floorNumber] = true; ;
                _pendingCallElevator2_COUNT_LOCAL_INT++;
                _pendingCallElevator2Time_LOCAL_INT[floorNumber] = Time.time;
                break;
        }
        _elevatorRequester.RequestElevatorInternalTarget(elevatorNumber, floorNumber);
    }
    #endregion API_FUNCTIONS
    //------------------------------------------------------------------------------------------------------------
    //--------------------------------- Network Call Receivers----------------------------------------------------
    //------------------------------------------------------------------------------------------------------------
    #region ELREQ_FUNCTIONS
    /// <summary>
    /// This function receives a client request (and is run by master-only)
    /// </summary>
    public void ELREQ_CallFromFloor(bool directionUp, int floor)
    {
        Debug.Log("[NetworkingController] Master received Elevator called to floor " + floor + " by localPlayer (DirectionUp: " + directionUp.ToString() + ")");
        //if direction up AND NOT elevator called up to floor x
        if (directionUp && (0UL == (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_ElevatorCalledUp + floor)))))
        {
            if (!MASTER_ElevatorAlreadyThereAndOpen(floor, true))
            {
                MASTER_SetSyncValue(SyncBoolReq_ElevatorCalledUp_0 + floor, true);
                _calledToFloorToGoUp_MASTER[floor] = true;
                _calledToFloorToGoUp_MASTER_COUNT++;
            }
        }
        //if NOT direction up AND NOT elevator called down to floor x
        else if (!directionUp && (0UL == (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_ElevatorCalledDown + floor)))))
        {
            if (!MASTER_ElevatorAlreadyThereAndOpen(floor, false))
            {
                MASTER_SetSyncValue(SyncBoolReq_ElevatorCalledDown_0 + floor, true);
                _calledToFloorToGoDown_MASTER[floor] = true;
                _calledToFloorToGoDown_MASTER_COUNT++;
            }
        }
    }
    /// <summary>
    /// Only Master receives this, it's called by ElevatorRequester
    /// </summary>
    public void ELREQ_CallToChangeDoorState(int elevatorNumber, bool open)
    {
        float test = Time.time - _timeAtCurrentFloorElevatorClosed_MASTER[elevatorNumber];
        Debug.Log("Master received CallToChangeDoorState for elevator " + elevatorNumber + " (Direction open: " + open.ToString() + ") Elevator driving:" + _elevatorIsDriving_MASTER[elevatorNumber]);

        //if (open AND elevator X idle) OR (some timing stuff AND NOT driving)
        if (open && 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXidle + elevatorNumber))) || (Time.time - _timeAtCurrentFloorElevatorClosed_MASTER[elevatorNumber] < 2.5f && !_elevatorIsDriving_MASTER[elevatorNumber]))
        {
            MASTER_HandleFloorDoorOpening(elevatorNumber, GetSyncElevatorFloor(elevatorNumber), 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXgoingUp + elevatorNumber))), 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXidle + elevatorNumber))));
        }
        //if NOT open AND elevator X idle AND some timing stuff
        else if (!open && 0UL != (_syncData1 & (1UL << (SyncBool_AddressUlong_ElevatorXopen + elevatorNumber))) && Time.time - _timeAtCurrentFloorElevatorOpened_MASTER[elevatorNumber] > 6f)
        {
            MASTER_SetSyncValue(SyncBool_Elevator0open + elevatorNumber, false);
            _timeAtCurrentFloorElevatorClosed_MASTER[elevatorNumber] = Time.time;
        }
    }
    /// <summary>
    /// Only Master receives this, it's called by ElevatorRequester
    /// </summary>
    public void ELREQ_SetInternalTarget(int elevatorNumber, int floorNumber)
    {

        Debug.Log("[NetworkController] Master received client request to set target for elevator " + elevatorNumber + " to floor " + floorNumber);
        //if elevatorNumber0 AND NOT elevator0 called to floor X
        if (elevatorNumber == 0 && (0UL == (_syncData1 & (1UL << (SyncBoolReq_AddressUlong_Elevator0CalledToFloor + floorNumber)))))
        {
            Debug.Log("Internal target was now set.");
            MASTER_SetSyncValue(SyncBoolReq_Elevator0CalledToFloor_0 + floorNumber, true);
            _elevator0FloorTargets_MASTER[floorNumber] = true;
            _elevator0FloorTargets_MASTER_COUNT++;
            return;
        }
        //if elevatorNumber1 AND NOT elevator1 called to floor X
        else if (elevatorNumber == 1 && (0U == (_syncData2 & (1U << (SyncBoolReq_AddressUint_Elevator1CalledToFloor + floorNumber)))))
        {
            Debug.Log("Internal target was now set.");
            MASTER_SetSyncValue(SyncBoolReq_Elevator1CalledToFloor_0 + floorNumber, true);
            _elevator1FloorTargets_MASTER[floorNumber] = true;
            _elevator1FloorTargets_MASTER_COUNT++;
            return;
        }
        //if elevatorNumber2 AND NOT elevator2 called to floor X
        else if (elevatorNumber == 2 && (0U == (_syncData2 & (1U << (SyncBoolReq_AddressUint_Elevator2CalledToFloor + floorNumber)))))
        {
            Debug.Log("Internal target was now set.");
            MASTER_SetSyncValue(SyncBoolReq_Elevator2CalledToFloor_0 + floorNumber, true);
            _elevator2FloorTargets_MASTER[floorNumber] = true;
            _elevator2FloorTargets_MASTER_COUNT++;
            return;
        }
        Debug.Log("No target was set since the elevator is already called to that floor");
    }
    /// <summary>
    /// Checks if there is already an open elevator on this floor which is going in the target direction
    /// </summary>
    /// <param name="floor"></param>
    /// <param name="directionUp"></param>
    /// <returns></returns>
    private bool MASTER_ElevatorAlreadyThereAndOpen(int floor, bool directionUp)
    {
        if (_elevator0Working && GetSyncElevatorFloor(0) == floor && 0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator0open)) && (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator0goingUp)) || 0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator0idle))))
        {
            return true;
        }
        if (_elevator1Working && GetSyncElevatorFloor(1) == floor && 0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator1open)) && (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator1goingUp)) || 0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator1idle))))
        {
            return true;
        }
        if (_elevator2Working && GetSyncElevatorFloor(2) == floor && 0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator2open)) && (0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator2goingUp)) || 0UL != (_syncData1 & (SyncBool_MaskUlong_Elevator2idle))))
        {
            return true;
        }
        return false;
    }
    #endregion ELREQ_FUNCTIONS
    //------------------------------------------------------------------------------------------------------------
    //----------------------------------SyncBool Interface -------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------
    #region SYNCBOOL_FUNCTIONS
    //------------------------------------------------------------------------------------------------------------
    //------------------------------------------ SyncBool lowlevel code ------------------------------------------
    //------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This script sets and reads individual bits within a uint as well as encoding three numbers (nibbles) within the most significant bytes
    /// 
    /// The ulong maps as follows:-
    ///  - 63-60 variable_1 (4bits)
    ///  - 59-56 variable_2 (4bits)
    ///  - 55-52 variable_3 (4bits)
    ///  - 51-0 binary bools [51-0]
    ///
    /// The uint maps as follows:-
    ///  - 0-31 binary bools [52-83(?)]
    /// 
    /// Script by NotFish
    /// </summary>''        
    private const byte elevatorOneOffset = 60;
    private const byte elevatorTwoOffset = 56;
    private const byte elevatorThreeOffset = 52;
    private const byte ulongBoolEndPosition = 51; //You will need to recalculate the bool array classes if you modify this
    private const ulong nibbleMask = 15; // ...0000 0000 1111        
                                         /// <summary>
                                         /// Modifies a _syncData1 & _syncData2 on the bit level.
                                         /// Sets "value" to bit "position" of "input".
                                         /// </summary>       
                                         /// <param name="input">uint to modify</param>
                                         /// <param name="position">Bit position to modify (0-83)</param>
                                         /// <param name="value">Value to set the bit</param>        
                                         /// <returns>Returns the modified uint</returns>
    private void MASTER_SetSyncValue(int position, bool value)
    {
        Debug.Log($"SYNC DATA bool {position} set to {value.ToString()}");
        //Not sure if there is something multi-threaded going on in the background, so creating working copies just in case.
        ulong localUlong = _syncData1;
        uint localUint = _syncData2;

        //Sanitise position
        if (position < 0 || position > 83)
        {
            //TODO: remove on live build
            Debug.Log("uintConverter - Position out of range");
            return;
        }

        //Fill ulong then uint            
        if (position <= ulongBoolEndPosition)
        {
            //Store in the ulong
            if (value)
            {
                //We want to set the value to true
                //Set the bit using a bitwise OR. 
                localUlong |= (1UL << position);
            }
            else
            {
                //We want to set the value to false
                //Udon does not currently support bitwise NOT
                //Instead making sure bit is set to true and using a bitwise XOR.
                ulong mask = (1UL << position);
                localUlong |= mask;
                localUlong ^= mask;
            }
        }
        else // position > length of ulong
        {
            //Store in the uint
            //Need to shift to to a valid address first!
            position -= ulongBoolEndPosition + 1;

            if (value)
            {
                //We want to set the value to true
                //Set the bit using a bitwise OR. 
                localUint |= (1U << position);
            }
            else
            {
                //We want to set the value to false
                //Udon does not currently support bitwise NOT
                //Instead making sure bit is set to true and using a bitwise XOR.
                uint mask = (1U << position);
                localUint |= mask;
                localUint ^= mask;
            }
        }

        //Let's not forget to actually write it back to syncData!
        _syncData1 = localUlong;
        _syncData2 = localUint;
    }

    /// <summary>
    /// Reads the value of the bit at "position" of the combined syncData (_syncData1 & _syncData2).
    /// </summary>       
    /// <param name="input">uint to inspect</param>
    /// <param name="position">Bit position to read (0-83)</param>
    /// <returns>Boolean of specified bit position. Returns false on error.</returns>
    private bool GetSyncValue(int position)
    {
        //Sanitise position
        if (position < 0 || position > 83)
        {
            //TODO: remove on live build
            Debug.Log("uintConverter - Position out of range");
            return false;
        }

        //Read from Ulong then uint            

        if (position <= ulongBoolEndPosition)
        {
            //Read from the ulong
            //Inspect using a bitwise AND and a mask.
            //Branched in an IF statment for readability.
            if ((_syncData1 & (1UL << position)) != 0UL)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else // position < ulong length
        {
            //Read from the uint
            //Need to shift to to a valid address first!
            position -= ulongBoolEndPosition + 1;

            //Inspect using a bitwise AND and a mask.
            //Branched in an IF statment for readability.
            if ((_syncData2 & (1U << position)) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Reads out all the booleans at once (preserving mapping compared to direct access)
    /// </summary>               
    /// <returns>Returns all the bools within the uint and ulong</returns>
    private bool[] GetBoolArray()
    {
        bool[] output = new bool[84];

        //Look a precomputed masks and no loops :)
        output[0] = (_syncData1 & 1UL) != 0UL;
        output[1] = (_syncData1 & 2UL) != 0U;
        output[2] = (_syncData1 & 4UL) != 0U;
        output[3] = (_syncData1 & 8UL) != 0U;
        output[4] = (_syncData1 & 16UL) != 0U;
        output[5] = (_syncData1 & 32UL) != 0U;
        output[6] = (_syncData1 & 64UL) != 0U;
        output[7] = (_syncData1 & 128UL) != 0U;
        output[8] = (_syncData1 & 256UL) != 0U;
        output[9] = (_syncData1 & 512UL) != 0U;
        output[10] = (_syncData1 & 1024UL) != 0U;
        output[11] = (_syncData1 & 2048UL) != 0U;
        output[12] = (_syncData1 & 4096UL) != 0U;
        output[13] = (_syncData1 & 8192UL) != 0U;
        output[14] = (_syncData1 & 16384UL) != 0U;
        output[15] = (_syncData1 & 32768UL) != 0U;
        output[16] = (_syncData1 & 65536UL) != 0U;
        output[17] = (_syncData1 & 131072UL) != 0U;
        output[18] = (_syncData1 & 262144UL) != 0U;
        output[19] = (_syncData1 & 524288UL) != 0U;
        output[20] = (_syncData1 & 1048576UL) != 0U;
        output[21] = (_syncData1 & 2097152UL) != 0U;
        output[22] = (_syncData1 & 4194304UL) != 0U;
        output[23] = (_syncData1 & 8388608UL) != 0U;
        output[24] = (_syncData1 & 16777216UL) != 0U;
        output[25] = (_syncData1 & 33554432UL) != 0U;
        output[26] = (_syncData1 & 67108864UL) != 0U;
        output[27] = (_syncData1 & 134217728UL) != 0U;
        output[28] = (_syncData1 & 268435456UL) != 0U;
        output[29] = (_syncData1 & 536870912UL) != 0U;
        output[30] = (_syncData1 & 1073741824UL) != 0U;
        output[31] = (_syncData1 & 2147483648UL) != 0U;
        output[32] = (_syncData1 & 4294967296UL) != 0U;
        output[33] = (_syncData1 & 8589934592UL) != 0U;
        output[34] = (_syncData1 & 17179869184UL) != 0U;
        output[35] = (_syncData1 & 34359738368UL) != 0U;
        output[36] = (_syncData1 & 68719476736UL) != 0U;
        output[37] = (_syncData1 & 137438953472UL) != 0U;
        output[38] = (_syncData1 & 274877906944UL) != 0U;
        output[39] = (_syncData1 & 549755813888UL) != 0U;
        output[40] = (_syncData1 & 1099511627776UL) != 0U;
        output[41] = (_syncData1 & 2199023255552UL) != 0U;
        output[42] = (_syncData1 & 4398046511104UL) != 0U;
        output[43] = (_syncData1 & 8796093022208UL) != 0U;
        output[44] = (_syncData1 & 17592186044416UL) != 0U;
        output[45] = (_syncData1 & 35184372088832UL) != 0U;
        output[46] = (_syncData1 & 70368744177664UL) != 0U;
        output[47] = (_syncData1 & 140737488355328UL) != 0U;
        output[48] = (_syncData1 & 281474976710656UL) != 0U;
        output[49] = (_syncData1 & 562949953421312UL) != 0U;
        output[50] = (_syncData1 & 1125899906842624UL) != 0U;
        output[51] = (_syncData1 & 2251799813685248UL) != 0U;
        output[52] = (_syncData2 & 1U) != 0U;
        output[53] = (_syncData2 & 2U) != 0U;
        output[54] = (_syncData2 & 4U) != 0U;
        output[55] = (_syncData2 & 8U) != 0U;
        output[56] = (_syncData2 & 16U) != 0U;
        output[57] = (_syncData2 & 32U) != 0U;
        output[58] = (_syncData2 & 64U) != 0U;
        output[59] = (_syncData2 & 128U) != 0U;
        output[60] = (_syncData2 & 256U) != 0U;
        output[61] = (_syncData2 & 512U) != 0U;
        output[62] = (_syncData2 & 1024U) != 0U;
        output[63] = (_syncData2 & 2048U) != 0U;
        output[64] = (_syncData2 & 4096U) != 0U;
        output[65] = (_syncData2 & 8192U) != 0U;
        output[66] = (_syncData2 & 16384U) != 0U;
        output[67] = (_syncData2 & 32768U) != 0U;
        output[68] = (_syncData2 & 65536U) != 0U;
        output[69] = (_syncData2 & 131072U) != 0U;
        output[70] = (_syncData2 & 262144U) != 0U;
        output[71] = (_syncData2 & 524288U) != 0U;
        output[72] = (_syncData2 & 1048576U) != 0U;
        output[73] = (_syncData2 & 2097152U) != 0U;
        output[74] = (_syncData2 & 4194304U) != 0U;
        output[75] = (_syncData2 & 8388608U) != 0U;
        output[76] = (_syncData2 & 16777216U) != 0U;
        output[77] = (_syncData2 & 33554432U) != 0U;
        output[78] = (_syncData2 & 67108864U) != 0U;
        output[79] = (_syncData2 & 134217728U) != 0U;
        output[80] = (_syncData2 & 268435456U) != 0U;
        output[81] = (_syncData2 & 536870912U) != 0U;
        output[82] = (_syncData2 & 1073741824U) != 0U;
        output[83] = (_syncData2 & 2147483648U) != 0U;

        return output;
    }

    /// <summary>
    /// Reads out all the Ulong booleans at once (preserving mapping compared to direct access)
    /// </summary>               
    /// <returns>Returns all the bools within the ulong</returns>
    private bool[] GetBoolArrayUlongONLY()
    {
        bool[] output = new bool[52];

        //Look a precomputed masks and no loops :)
        output[0] = (_syncData1 & 1UL) != 0UL;
        output[1] = (_syncData1 & 2UL) != 0U;
        output[2] = (_syncData1 & 4UL) != 0U;
        output[3] = (_syncData1 & 8UL) != 0U;
        output[4] = (_syncData1 & 16UL) != 0U;
        output[5] = (_syncData1 & 32UL) != 0U;
        output[6] = (_syncData1 & 64UL) != 0U;
        output[7] = (_syncData1 & 128UL) != 0U;
        output[8] = (_syncData1 & 256UL) != 0U;
        output[9] = (_syncData1 & 512UL) != 0U;
        output[10] = (_syncData1 & 1024UL) != 0U;
        output[11] = (_syncData1 & 2048UL) != 0U;
        output[12] = (_syncData1 & 4096UL) != 0U;
        output[13] = (_syncData1 & 8192UL) != 0U;
        output[14] = (_syncData1 & 16384UL) != 0U;
        output[15] = (_syncData1 & 32768UL) != 0U;
        output[16] = (_syncData1 & 65536UL) != 0U;
        output[17] = (_syncData1 & 131072UL) != 0U;
        output[18] = (_syncData1 & 262144UL) != 0U;
        output[19] = (_syncData1 & 524288UL) != 0U;
        output[20] = (_syncData1 & 1048576UL) != 0U;
        output[21] = (_syncData1 & 2097152UL) != 0U;
        output[22] = (_syncData1 & 4194304UL) != 0U;
        output[23] = (_syncData1 & 8388608UL) != 0U;
        output[24] = (_syncData1 & 16777216UL) != 0U;
        output[25] = (_syncData1 & 33554432UL) != 0U;
        output[26] = (_syncData1 & 67108864UL) != 0U;
        output[27] = (_syncData1 & 134217728UL) != 0U;
        output[28] = (_syncData1 & 268435456UL) != 0U;
        output[29] = (_syncData1 & 536870912UL) != 0U;
        output[30] = (_syncData1 & 1073741824UL) != 0U;
        output[31] = (_syncData1 & 2147483648UL) != 0U;
        output[32] = (_syncData1 & 4294967296UL) != 0U;
        output[33] = (_syncData1 & 8589934592UL) != 0U;
        output[34] = (_syncData1 & 17179869184UL) != 0U;
        output[35] = (_syncData1 & 34359738368UL) != 0U;
        output[36] = (_syncData1 & 68719476736UL) != 0U;
        output[37] = (_syncData1 & 137438953472UL) != 0U;
        output[38] = (_syncData1 & 274877906944UL) != 0U;
        output[39] = (_syncData1 & 549755813888UL) != 0U;
        output[40] = (_syncData1 & 1099511627776UL) != 0U;
        output[41] = (_syncData1 & 2199023255552UL) != 0U;
        output[42] = (_syncData1 & 4398046511104UL) != 0U;
        output[43] = (_syncData1 & 8796093022208UL) != 0U;
        output[44] = (_syncData1 & 17592186044416UL) != 0U;
        output[45] = (_syncData1 & 35184372088832UL) != 0U;
        output[46] = (_syncData1 & 70368744177664UL) != 0U;
        output[47] = (_syncData1 & 140737488355328UL) != 0U;
        output[48] = (_syncData1 & 281474976710656UL) != 0U;
        output[49] = (_syncData1 & 562949953421312UL) != 0U;
        output[50] = (_syncData1 & 1125899906842624UL) != 0U;
        output[51] = (_syncData1 & 2251799813685248UL) != 0U;

        return output;
    }

    /// <summary>
    /// Reads out all the Uint booleans at once (preserving mapping compared to direct access)
    /// </summary>               
    /// <returns>Returns all the bools within the uint</returns>
    private bool[] GetBoolArrayUintONLY()
    {
        bool[] output = new bool[84];

        //Look a precomputed masks and no loops :)
        output[52] = (_syncData2 & 1U) != 0U;
        output[53] = (_syncData2 & 2U) != 0U;
        output[54] = (_syncData2 & 4U) != 0U;
        output[55] = (_syncData2 & 8U) != 0U;
        output[56] = (_syncData2 & 16U) != 0U;
        output[57] = (_syncData2 & 32U) != 0U;
        output[58] = (_syncData2 & 64U) != 0U;
        output[59] = (_syncData2 & 128U) != 0U;
        output[60] = (_syncData2 & 256U) != 0U;
        output[61] = (_syncData2 & 512U) != 0U;
        output[62] = (_syncData2 & 1024U) != 0U;
        output[63] = (_syncData2 & 2048U) != 0U;
        output[64] = (_syncData2 & 4096U) != 0U;
        output[65] = (_syncData2 & 8192U) != 0U;
        output[66] = (_syncData2 & 16384U) != 0U;
        output[67] = (_syncData2 & 32768U) != 0U;
        output[68] = (_syncData2 & 65536U) != 0U;
        output[69] = (_syncData2 & 131072U) != 0U;
        output[70] = (_syncData2 & 262144U) != 0U;
        output[71] = (_syncData2 & 524288U) != 0U;
        output[72] = (_syncData2 & 1048576U) != 0U;
        output[73] = (_syncData2 & 2097152U) != 0U;
        output[74] = (_syncData2 & 4194304U) != 0U;
        output[75] = (_syncData2 & 8388608U) != 0U;
        output[76] = (_syncData2 & 16777216U) != 0U;
        output[77] = (_syncData2 & 33554432U) != 0U;
        output[78] = (_syncData2 & 67108864U) != 0U;
        output[79] = (_syncData2 & 134217728U) != 0U;
        output[80] = (_syncData2 & 268435456U) != 0U;
        output[81] = (_syncData2 & 536870912U) != 0U;
        output[82] = (_syncData2 & 1073741824U) != 0U;
        output[83] = (_syncData2 & 2147483648U) != 0U;
        return output;
    }

    /// <summary>
    /// Decodes and returns the floor number of the ulong
    /// </summary>           
    /// <param name="elevatorNumber">Number of the elevator 1-3</param>        
    /// <param name="floorNumber">value to set to the elevator variable</param>
    /// <returns>The updated uint</returns>
    private void MASTER_SetSyncElevatorFloor(int elevatorNumber, int floorNumber)
    {
        Debug.Log($"SYNC DATA elevator {elevatorNumber} floor setting to {floorNumber}");
        //Not sure if there is something multi-threaded going on in the background, so creating working copies just in case.
        ulong localUlong = _syncData1;
        //Debug.Log($"SYNC DATA_1 was {localUlong}");
        //Sanitise the size of elevatorNumber
        if (elevatorNumber < 0 || elevatorNumber > 2)
        {
            //TODO: remove on live build
            Debug.Log($"uintConverter - 404 Elevator {elevatorNumber} does not exist");
            return;
        }

        //sanitise floorNumber
        if (floorNumber < 0 || floorNumber > 15)
        {
            //TODO: remove on live build
            Debug.Log($"uintConverter - Elevator  {elevatorNumber} number invalid");
            return;
        }
        ulong modifiedFloorNumber = (ulong)floorNumber;
        //Not sure if Udon likes SWITCH cases, so just doing this with IF statments
        //Setting the variables using the following process        
        //1- Shift the data to the right bit section of the uint
        //2- Create mask to zero the right bits on the orginal uint
        //3- Zero the relevant bits on the original uint.
        //   Udon does not support bitwise NOT, so a clumsy mix of OR, XOR to zero it out...
        //4- Bitwise OR the two variables together to overlay the two, I guess you could also add them.                   
        if (elevatorNumber == 0)
        {
            modifiedFloorNumber = (modifiedFloorNumber << elevatorOneOffset);
            const ulong mask = (nibbleMask << elevatorOneOffset);
            localUlong |= mask;
            localUlong ^= mask;
            localUlong |= modifiedFloorNumber;
        }
        else if (elevatorNumber == 1)
        {
            modifiedFloorNumber = (modifiedFloorNumber << elevatorTwoOffset);
            const ulong mask = (nibbleMask << elevatorTwoOffset);
            localUlong |= mask;
            localUlong ^= mask;
            localUlong |= modifiedFloorNumber;
        }
        else  //Elevator 3
        {
            modifiedFloorNumber = (modifiedFloorNumber << elevatorThreeOffset);
            const ulong mask = (nibbleMask << elevatorThreeOffset);
            localUlong |= mask;
            localUlong ^= mask;
            localUlong |= modifiedFloorNumber;
        }
        _syncData1 = localUlong;
        //Debug.Log($"SYNC DATA_1 is now {localUlong}");
    }

    /// <summary>
    /// Decodes and returns the floor number of the ulong
    /// </summary>              
    /// <param name="elevatorNumber">Number of the elevator 1-3</param>        
    /// <returns>Returns the floorNumber from the uint</returns>
    private int GetSyncElevatorFloor(int elevatorNumber)
    {
        //Debug.Log($"SYNC DATA_1 is now {_syncData1}");
        //Sanitise the size of elevatorNumber
        if (elevatorNumber < 0 || elevatorNumber > 2)
        {
            //TODO: remove on live build if needed
            Debug.Log($"uintConverter - 404 Elevator  {elevatorNumber} does not exist");
            return 0;
        }

        //Not sure if Udon likes SWITCH cases, so just doing this with IF statments
        if (elevatorNumber == 0)
        {
            //No need to mask the higher bits, so a straight return.
            return (int)(_syncData1 >> elevatorOneOffset); ;
        }
        else if (elevatorNumber == 1)
        {
            //Shift data
            ulong shiftedData = (_syncData1 >> elevatorTwoOffset);
            //Mask away the higher bits
            shiftedData &= nibbleMask;
            return (int)(shiftedData & nibbleMask);
        }
        else  //Elevator 3
        {
            //Shift data
            ulong shiftedData = (_syncData1 >> elevatorThreeOffset);
            //Mask away the higher bits                
            return (int)(shiftedData & nibbleMask);
        }
    }
    #endregion SYNCBOOL_FUNCTIONS
}

















////------------------------------------------------------------------------------------------------------------
////------------------------------------------ SyncBool lowlevel code ------------------------------------------
////------------------------------------------------------------------------------------------------------------

///// <summary>
///// This script sets and reads individual bits within a uint as well as encoding three numbers (nibbles) within the most significant bytes
///// 
///// The ulong maps as follows:-
/////  - 63-60 variable_1 (4bits)
/////  - 59-56 variable_2 (4bits)
/////  - 55-52 variable_2 (4bits)
/////  - 51-0 binary bools [0-51]
/////
///// The uint maps as follows:-
/////  - 31-0 binary bools [52-83(?)]
///// 
///// Script by NotFish
///// </summary>''
//private const byte elevatorOneOffset = 60;
//private const byte elevatorTwoOffset = 56;
//private const byte elevatorThreeOffset = 52;
//private const byte ulongBoolStartPosition = 51;
//private const uint nibbleMask = 15; // ...0000 0000 1111 
//private const int elevatorFloorNumberOffset = -2; //Keks floor hack offset

///// <summary>
///// Modifies a _syncData1 & _syncData2 on the bit level.
///// Sets "value" to bit "position" of "input".
///// </summary>       
///// <param name="input">uint to modify</param>
///// <param name="position">Bit position to modify (0-83)</param>
///// <param name="value">Value to set the bit</param>        
///// <returns>Returns the modified uint</returns>
//private void MASTER_SetSyncValue(int position, bool value)
//{
//    Debug.Log($"SYNC DATA bool {position} set to {value.ToString()}");
//    //Not sure if there is something multi-threaded going on in the background, so creating working copies just in case.
//    ulong localUlong = _syncData1;
//    uint localUint = _syncData2;

//    //Sanitise position
//    if (position < 0 || position > 83)
//    {
//        Debug.LogError("uintConverter - Position out of range");
//        return;
//    }

//    //Index the positions back to front (negative index to be stored in the uint)
//    position = ulongBoolStartPosition - position;

//    if (position > 0)
//    {
//        //Store in the ulong
//        if (value)
//        {
//            //We want to set the value to true
//            //Set the bit using a bitwise OR. 
//            localUlong |= ((ulong)(1) << position);
//        }
//        else
//        {
//            //We want to set the value to false
//            //Udon does not currently support bitwise NOT
//            //Instead making sure bit is set to true and using a bitwise XOR.
//            ulong mask = ((ulong)(1) << position);
//            localUlong |= mask;
//            localUlong ^= mask;
//        }
//    }
//    else // position < 0
//    {
//        //Store in the uint
//        //Need to shift to to a valid address first!
//        position += 32;

//        if (value)
//        {
//            //We want to set the value to true
//            //Set the bit using a bitwise OR. 
//            localUint |= ((uint)(1) << position);
//        }
//        else
//        {
//            //We want to set the value to false
//            //Udon does not currently support bitwise NOT
//            //Instead making sure bit is set to true and using a bitwise XOR.
//            uint mask = ((uint)(1) << position);
//            localUint |= mask;
//            localUint ^= mask;
//        }
//    }

//    //Let's not forget to actually write it back to syncData!
//    _syncData1 = localUlong;
//    _syncData2 = localUint;
//}

///// <summary>
///// Reads the value of the bit at "position" of the combined syncData (_syncData1 & _syncData2).
///// </summary>       
///// <param name="input">uint to inspect</param>
///// <param name="position">Bit position to read (0-83)</param>
///// <returns>Boolean of specified bit position. Returns false on error.</returns>
//private bool GetSyncValue(int position)
//{
//    //Sanitise position
//    if (position < 0 || position > 83)
//    {
//        Debug.LogError("uintConverter - Position out of range");
//        return false;
//    }

//    //Index the positions back to front (negative index to be stored in the uint)
//    position = ulongBoolStartPosition - position;

//    if (position > 0)
//    {
//        //Read from the ulong
//        //Inspect using a bitwise AND and a mask.
//        //Branched in an IF statment for readability.
//        if ((_syncData1 & ((ulong)(1) << position)) != 0ul)
//        {
//            return true;
//        }
//        else
//        {
//            return false;
//        }
//    }
//    else // position < 0
//    {
//        //Read from the uint
//        //Need to shift to to a valid address first!
//        position += 32;

//        //Inspect using a bitwise AND and a mask.
//        //Branched in an IF statment for readability.
//        if ((_syncData2 & ((uint)(1) << position)) != 0ul)
//        {
//            return true;
//        }
//        else
//        {
//            return false;
//        }
//    }
//}

///// <summary>
///// Decodes and returns the floor number of the ulong
///// </summary>           
///// <param name="elevatorNumber">Number of the elevator 1-3</param>        
///// <param name="floorNumber">value to set to the elevator variable</param>
///// <returns>The updated uint</returns>
//private void MASTER_SetSyncElevatorFloor(int elevatorNumber, int floorNumber)
//{
//    Debug.Log($"SYNC DATA elevator {elevatorNumber} set to {floorNumber}");
//    //Not sure if there is something multi-threaded going on in the background, so creating working copies just in case.
//    ulong localUlong = _syncData1;

//    //Sanitise the size of elevatorNumber
//    if (elevatorNumber < 0 || elevatorNumber > 2)
//    {
//        Debug.LogError($"uintConverter - 404 Elevator {elevatorNumber} does not exist");
//        return;
//    }

//    //floorNumber needs to be betweeen 0-15, so quick hack for negative floors and sanitise
//    int modifiedFloorNumberTempUint = (floorNumber - elevatorFloorNumberOffset);
//    if (modifiedFloorNumberTempUint < 0 || modifiedFloorNumberTempUint > 15)
//    {
//        Debug.LogError($"uintConverter - Elevator  {elevatorNumber} number invalid");
//        return;
//    }
//    ulong modifiedFloorNumber = (ulong)modifiedFloorNumberTempUint;
//    //Not sure if Udon likes SWITCH cases, so just doing this with IF statments
//    //Setting the variables using the following process        
//    //1- Shift the data to the right bit section of the uint
//    //2- Create mask to zero the right bits on the orginal uint
//    //3- Zero the relevant bits on the original uint.
//    //   Udon does not support bitwise NOT, so a clumsy mix of OR, XOR to zero it out...
//    //4- Bitwise OR the two variables together to overlay the two, I guess you could also add them.                   
//    if (elevatorNumber == 0)
//    {
//        modifiedFloorNumber = (modifiedFloorNumber << elevatorOneOffset);
//        ulong mask = (nibbleMask << elevatorOneOffset);
//        _syncData1 |= mask;
//        _syncData1 ^= mask;
//        _syncData1 |= modifiedFloorNumber;
//    }
//    else if (elevatorNumber == 1)
//    {
//        modifiedFloorNumber = (modifiedFloorNumber << elevatorTwoOffset);
//        ulong mask = (nibbleMask << elevatorTwoOffset);
//        _syncData1 |= mask;
//        _syncData1 ^= mask;
//        _syncData1 |= modifiedFloorNumber;
//    }
//    else  //Elevator 3
//    {
//        modifiedFloorNumber = (modifiedFloorNumber << elevatorThreeOffset);
//        ulong mask = (nibbleMask << elevatorThreeOffset);
//        _syncData1 |= mask;
//        _syncData1 ^= mask;
//        _syncData1 |= modifiedFloorNumber;
//    }
//}

///// <summary>
///// Decodes and returns the floor number of the ulong
///// </summary>              
///// <param name="elevatorNumber">Number of the elevator 1-3</param>        
///// <returns>Returns the floorNumber from the uint</returns>
//private int GetSyncElevatorFloor(int elevatorNumber)
//{
//    //Sanitise the size of elevatorNumber
//    if (elevatorNumber < 0 || elevatorNumber > 2)
//    {
//        Debug.LogError($"uintConverter - 404 Elevator  {elevatorNumber} does not exist");
//        return 0;
//    }

//    //Not sure if Udon likes SWITCH cases, so just doing this with IF statments
//    if (elevatorNumber == 0)
//    {
//        //No need to mask the higher bits, so a straight return.
//        int floorNumber = (int)(_syncData1 >> elevatorOneOffset);
//        floorNumber += elevatorFloorNumberOffset;
//        return floorNumber;
//    }
//    else if (elevatorNumber == 1)
//    {
//        //Shift data
//        ulong shiftedData = (_syncData1 >> elevatorTwoOffset);
//        //Mask away the higher bits
//        shiftedData &= nibbleMask;
//        int floorNumber = (int)(shiftedData) + elevatorFloorNumberOffset;
//        return floorNumber;
//    }
//    else  //Elevator 3
//    {
//        //Shift data
//        ulong shiftedData = (_syncData1 >> elevatorThreeOffset);
//        //Mask away the higher bits
//        shiftedData &= nibbleMask;
//        int floorNumber = (int)(shiftedData) + elevatorFloorNumberOffset;
//        return floorNumber;
//    }
//}





///// <summary>
///// Everyone can read bools from the synced states
///// </summary>
///// <returns></returns>
//private bool GetSyncValue(int position)
//{
//    //return BoolUIntConverter_GetValue(_syncData, position);
//}
///// <summary>
///// Only the master can call this function to change a bool state
///// </summary>
//private void MASTER_SetSyncValue(int position, bool value)
//{
//    Debug.Log("SYNC DATA position " + position + " set to " + value.ToString());
//    //_syncData = BoolUIntConverter_SetValue(_syncData, position, value);
//}
///// <summary>
///// Everyone can read bools from the synced states
///// </summary>
///// <returns></returns>
//private bool GetSyncValue(int position)
//{
//    if (position <= 63)
//    {
//        return BoolInt64Converter_GetValue(_syncData, position);
//    }
//    else //position must start at 13 when the value is at or above 64
//    {
//        return BoolUIntConverter_GetValue(_syncData, position - 51);
//    }
//}
///// <summary>
///// Only the master can call this function to change a bool state
///// </summary>
//private void MASTER_SetSyncValueReq(int position, bool value)
//{
//    Debug.Log("SYNC DATA REQ position " + position + " set to " + value.ToString());
//    if (position <= 63)
//    {
//        _syncDataReq = BoolInt46Converter_SetValue(_syncDataReq, position, value);
//    }
//    else //position must start at 13 when the value is at or above 64
//    {
//        _syncData = BoolUIntConverter_SetValue(_syncData, position - 51, value);
//    }
//}
///// <summary>
///// Allows master to set an elevator to a new floor
///// </summary>
//private void MASTER_SetSyncElevatorFloor(int elevatorNumber, int floorNumber)
//{
//    //_syncData1 = BoolUIntConverter_SetElevatorFloor(_syncData1, elevatorNumber, floorNumber);
//}
///// <summary>
///// returns the synced floor number on which an elevator currently is
///// </summary>
//private int GetSyncElevatorFloor(int elevatorNumber)
//{
//    //return BoolUIntConverter_GetElevatorFloor(_syncData1, elevatorNumber);
//}



//---------------------------------------------------- Bool To Int until 06.06.2020 ---------------------------------------------------------------------
///// <summary>
///// This script sets and reads individual bits within a uint as well as encoding/decoding three numbers (nibble sized) within the most significant bits
///// 
///// The uint maps as follows:-
/////  - 31-28 variable_1 (4bits)
/////  - 27-24 variable_2 (4bits)
/////  - 23-20 variable_3 (4bits)
/////  - 19-0 binary bools
/////  
///// Script by NotFish
///// </summary>

//private const byte elevatorOneOffset = 28;
//private const byte elevatorTwoOffset = 24;
//private const byte elevatorThreeOffset = 20;
//private const uint nibbleMask = 15; // ...0000 0000 1111 
//private const int elevatorFloorNumberOffset = -2; //Keks floor hack offset

///// <summary>
///// Modifies a uint on the bit level.
///// Sets "value" to bit "position" of "input".
///// </summary>       
///// <param name="input">uint to modify</param>
///// <param name="position">Bit position to modify (0-31)</param>
///// <param name="value">Value to set the bit</param>        
///// <returns>Returns the modified uint</returns>
//private uint BoolUIntConverter_SetValue(uint input, int position, bool value)
//{
//    //Sanitise position
//    if (position < 0 || position > 19)
//    {
//        Debug.LogError("[NetworkController] uintConverter - Position out of range");
//        return input;
//    }
//    if (value)
//    {
//        //We want to set the value to true
//        //Set the bit using a bitwise OR. 
//        input |= ((uint)(1) << position);
//    }
//    else
//    {
//        //We want to set the value to false
//        //Udon does not currently support bitwise NOT
//        //Instead making sure bit is set to true and using a bitwise XOR.
//        uint mask = ((uint)(1) << position);
//        input |= mask;
//        input ^= mask;
//    }

//    return input;
//}

///// <summary>
///// Reads the value of the bit at "position" of "input".
///// </summary>       
///// <param name="input">uint to inspect</param>
///// <param name="position">Bit position to read (0-31)</param>
///// <returns>Boolean of specified bit position. Returns false on error.</returns>
//private bool BoolUIntConverter_GetValue(uint input, int position)
//{
//    //Sanitise position
//    if (position < 0 || position > 19)
//    {
//        Debug.LogError("[NetworkController] uintConverter - Position out of range");
//        return false;
//    }

//    //Inspect using a bitwise AND and a mask.
//    //Branched in an IF statment for readability.
//    if ((input & ((uint)(1) << position)) != 0)
//    {
//        return true;
//    }
//    else
//    {
//        return false;
//    }
//}

///// <summary>
///// Returns a bool array of bits within a uint. Use GetValue whenever possible.
///// </summary>       
///// <param name="input">uint to convert</param>    
///// <returns>A bool array representing the uint input</returns>
//private bool[] BoolUIntConverter_GetValues(uint input)
//{
//    bool[] boolArray = new bool[19];

//    //Iterate through all the bits and populate the array
//    for (byte i = 0; i < 19; i++)
//    {
//        boolArray[i] = BoolUIntConverter_GetValue(input, i);
//    }

//    return boolArray;
//}

///// <summary>
///// Takes a bool[] and overlays it on a uint. Use SetValue whenever possible.
///// </summary>       
///// <param name="input">uint to modify</param>    
///// <param name="values">A bool array up to length 32</param>    
///// <returns>A bool array representing the uint input</returns>
//private uint BoolUIntConverter_SetValues(uint input, bool[] values)
//{
//    //Sanitise the size of values
//    if (values == null || values.Length >= 19)
//    {
//        Debug.LogError("[NetworkController] uintConverter - Array null or too long");
//        return input;
//    }

//    //Iterate through all the bools and set the values
//    for (byte i = 0; i < values.Length; i++)
//    {
//        input = BoolUIntConverter_SetValue(input, i, values[i]);
//    }

//    return input;
//}

///// <summary>
///// Decodes and returns the floor number of the uint
///// </summary>       
///// <param name="input">uint to modify</param>    
///// <param name="elevatorNumber">Number of the elevator 1-3</param>        
///// <param name="floorNumber">value to set to the elevator variable</param>
///// <returns>The updated uint</returns>
//private uint BoolUIntConverter_SetElevatorFloor(uint input, int elevatorNumber, int floorNumber)
//{
//    //Sanitise the size of elevatorNumber
//    if (elevatorNumber < 0 || elevatorNumber > 2)
//    {
//        Debug.LogError("[NetworkController] uintConverter - 404 Elevator does not exist");
//        return input;
//    }

//    //floorNumber needs to be betweeen 0-15, so quick hack for negative floors and sanitise
//    uint modifiedFloorNumber = (uint)(floorNumber - elevatorFloorNumberOffset);
//    if (modifiedFloorNumber < 0 || modifiedFloorNumber > 15)
//    {
//        Debug.LogError("[NetworkController] uintConverter - Elevator number invalid");
//        return input;
//    }

//    //Not sure if Udon likes SWITCH cases, so just doing this with IF statments
//    //Setting the variables using the following process        
//    //1- Shift the data to the right section of the uint
//    //2- Create mask to zero the variable's bits on the original uint
//    //3- Zero the relevant bits on the original uint.
//    //   Udon does not support bitwise NOT, so a clumsy mix of OR, XOR to zero it out...
//    //4- Bitwise OR the two variables together to overlay the two, I guess you could also add them.                   
//    if (elevatorNumber == 0)
//    {
//        modifiedFloorNumber = (modifiedFloorNumber << elevatorOneOffset);
//        uint mask = (nibbleMask << elevatorOneOffset);
//        input |= mask;
//        input ^= mask;
//        input |= modifiedFloorNumber;
//        return input;
//    }
//    else if (elevatorNumber == 1)
//    {
//        modifiedFloorNumber = (modifiedFloorNumber << elevatorTwoOffset);
//        uint mask = (nibbleMask << elevatorTwoOffset);
//        input |= mask;
//        input ^= mask;
//        input |= modifiedFloorNumber;
//        return input;
//    }
//    else  //Elevator 2
//    {
//        modifiedFloorNumber = (modifiedFloorNumber << elevatorThreeOffset);
//        uint mask = (nibbleMask << elevatorThreeOffset);
//        input |= mask;
//        input ^= mask;
//        input |= modifiedFloorNumber;
//        return input;
//    }
//}

///// <summary>
///// Decodes and returns the floor number of the uint
///// </summary>       
///// <param name="input">uint to decode</param>    
///// <param name="elevatorNumber">Number of the elevator 1-3</param>        
///// <returns>Returns the floorNumber from the uint</returns>
//private int BoolUIntConverter_GetElevatorFloor(uint input, int elevatorNumber)
//{
//    //Sanitise the size of elevatorNumber
//    if (elevatorNumber < 0 || elevatorNumber > 2)
//    {
//        Debug.LogError("[NetworkController] uintConverter - 404 Elevator does not exist");
//        return 0;
//    }

//    //Not sure if Udon likes SWITCH cases, so just doing this with IF statments
//    if (elevatorNumber == 0)
//    {
//        //No need to mask the higher bits, so a straight return.
//        int floorNumber = (int)(input >> elevatorOneOffset);
//        floorNumber += elevatorFloorNumberOffset;
//        return floorNumber;
//    }
//    else if (elevatorNumber == 1)
//    {
//        //Shift data
//        uint shiftedData = (input >> elevatorTwoOffset);
//        //Mask away the higher bits
//        shiftedData &= nibbleMask;
//        int floorNumber = (int)(shiftedData) + elevatorFloorNumberOffset;
//        return floorNumber;
//    }
//    else  //Elevator 2
//    {
//        //Shift data
//        uint shiftedData = (input >> elevatorThreeOffset);
//        //Mask away the higher bits
//        shiftedData &= nibbleMask;
//        int floorNumber = (int)(shiftedData) + elevatorFloorNumberOffset;
//        return floorNumber;
//    }
//}
////-------------------------------------- end of bool to UInt32 converter ----------------------------------


//-------------------------------------- bool to UInt32 converter old----------------------------------
///// <summary>
///// Modifies a UInt32 on the bit level.
///// Sets "value" to bit "position" of "input".
///// </summary>       
///// <param name="input">UInt32 to modify</param>
///// <param name="position">Bit position to modify (0-31)</param>
///// <param name="value">Value to set the bit</param>        
///// <returns>Returns the modified UInt32</returns>
//public uint BoolUIntConverter_SetValue(uint input, int position, bool value)
//{
//    //Sanitise position
//    if (position < 0 || position > 31)
//    {
//        Debug.LogError("Uint32Converter - Position out of range");
//        return input;
//    }
//    if (value)
//    {
//        //Set the value to true
//        //Set the bit using a bitwise OR. 
//        input |= ((uint)(1) << position);
//    }
//    else
//    {
//        //Set the value to false
//        //Udon does not currently support bitwise NOT
//        //Instead making sure bit is first set to true and then using a bitwise XOR.
//        uint mask = ((uint)(1) << position);
//        input |= mask;
//        input ^= mask;
//    }
//    return input;
//}

///// <summary>
///// Reads the value of the bit at "position" of "input".
///// </summary>       
///// <param name="input">UInt32 to inspect</param>
///// <param name="position">Bit position to read (0-31)</param>
///// <returns>Boolean of specified bit position. Returns false on error.</returns>
//public bool BoolUIntConverter_GetValue(uint input, int position)
//{
//    //Sanitise position
//    if (position < 0 || position > 31)
//    {
//        Debug.LogError("Uint32Converter - Position out of range");
//        return false;
//    }

//    //Inspect using a bitwise AND and a mask.
//    //Branched in an IF statment for readability.
//    if ((input & ((uint)(1) << position)) != 0)
//    {
//        return true;
//    }
//    else
//    {
//        return false;
//    }
//}

///// <summary>
///// Returns a bool array of bits within a UInt32. Use GetValue whenever possible.
///// </summary>       
///// <param name="input">UInt32 to convert</param>    
///// <returns>A bool array representing the UInt32 input</returns>
//public bool[] BoolUIntConverter_GetValues(uint input)
//{
//    bool[] boolArray = new bool[32];

//    //Iterate through all the bits and populate the array
//    for (byte i = 0; i < 32; i++)
//    {
//        boolArray[i] = BoolUIntConverter_GetValue(input, i);
//    }

//    return boolArray;
//}

///// <summary>
///// Takes a bool[] and overlays it on a UInt32. Use SetValue whenever possible.
///// </summary>       
///// <param name="input">UInt32 to overlay</param>    
///// <param name="values">A bool array up to length 32</param>    
///// <returns>A bool array representing the UInt32 input</returns>
//public uint BoolUIntConverter_SetValues(uint input, bool[] values)
//{
//    //Sanitise the size of values
//    if (values == null || values.Length > 32)
//    {
//        Debug.LogError("UintConverter - Array null or too long");
//        return 0;
//    }

//    //Iterate through all the bools and set the values
//    for (byte i = 0; i < values.Length; i++)
//    {
//        input = BoolUIntConverter_SetValue(input, i, values[i]);
//    }

//    return input;
//}