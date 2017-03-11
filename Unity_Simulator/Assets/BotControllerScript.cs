using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;


public class BotControllerScript: MonoBehaviour {

    private tcpConnection myTCP;
    public bool player_controller = true;
    //public string msgToServer;
    public Transform[] wheels;
    public float motorPower = 150.0f;
    public float maxTurn = 25.0f;
    private float time = 0;
    private float[] wheel_force = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };

    float instantePower = 0.0f;
    bool brake = false;
    float wheelTurn = 0.0f;

    Rigidbody myRigidbody;

    void Awake()
    {
        myTCP = gameObject.AddComponent<tcpConnection>();
    }

  	// Use this for initialization
    // Starts TCP Connection and creates rigidbody object for minimodbot
	void Start () {
        Debug.Log("Starting TCP Server");
        myTCP = new tcpConnection();
        myRigidbody = this.gameObject.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
    // Not Used
	void Update () {
    }

    // Sets velocity of rigidboy (minimodbot) to 0
    void stopBot ()
    {
        Vector3 prev = myRigidbody.velocity;
        prev.z = 0;
        prev.x = 0;
        myRigidbody.velocity = prev;
    }

    // Runs every fixed interval of time
    void FixedUpdate()
    {
        /*-------------------------------------------------------------------------------------------------------
        --------------------------------------Player Controller Script-------------------------------------------
        --------------------------------------------------------------------------------------------------------*/
        if (player_controller)
        {
            instantePower = Input.GetAxis("Vertical") * motorPower * Time.deltaTime;
            wheelTurn = Input.GetAxis("Horizontal") * maxTurn;
            brake = Input.GetKey("space");


            if (!(Input.GetAxis("Vertical") == 0) || ! (Input.GetAxis("Horizontal") == 0))
            {
                time = 0;
            }


            //Turn Collider
            getCollider(0).steerAngle = wheelTurn;
            getCollider(1).steerAngle = wheelTurn;

            //Turn Wheels
            wheels[0].localEulerAngles = new Vector3(wheels[0].localEulerAngles.x, getCollider(0).steerAngle - wheels[0].localEulerAngles.z + 90, wheels[0].localEulerAngles.z);
            wheels[1].localEulerAngles = new Vector3(wheels[1].localEulerAngles.x, getCollider(1).steerAngle - wheels[0].localEulerAngles.z + 90, wheels[1].localEulerAngles.z);

            //Spin Wheels
            wheels[0].Rotate(0, -getCollider(0).rpm / 60 * 360 * Time.deltaTime, 0);
            wheels[1].Rotate(0, -getCollider(1).rpm / 60 * 360 * Time.deltaTime, 0);
            wheels[2].Rotate(0, -getCollider(2).rpm / 60 * 360 * Time.deltaTime, 0);
            wheels[3].Rotate(0, -getCollider(3).rpm / 60 * 360 * Time.deltaTime, 0);

            //brakes
            if (brake)
            {
                time += Time.deltaTime;
                if (time > 3f)
                {
                    stopBot();
                }
                else
                {
                    Vector3 prev = myRigidbody.velocity;
                    prev.z = prev.z / 2;
                    prev.x = prev.x / 2;
                    myRigidbody.velocity = prev;
                }
            }
            else
            {
                getCollider(0).brakeTorque = 0.0f;
                getCollider(1).brakeTorque = 0.0f;
                getCollider(2).brakeTorque = 0.0f;
                getCollider(3).brakeTorque = 0.0f;

                getCollider(2).motorTorque = instantePower;
                getCollider(3).motorTorque = instantePower;
            }
        } else {
            /*-------------------------------------------------------------------------------------------------------
            ---------------------------------------TCP Controller Script---------------------------------------------
            --------------------------------------------------------------------------------------------------------*/

            string command = myTCP.getTransform();      //Retrieves TCP data
            myTCP.resetTransform();                     //Resets TCP data so that it doesn't retrieve the same data twice
            if (command != "")
            {
                if (command.IndexOf("WHEELS") > 0)      //Checks for WHEEL values
                {
                    print("Command is " + command);

                    //Parses WHEEL command and stores it in wheel_force
                    command = command.Substring(command.IndexOf(",") + 1, command.IndexOf(">") - command.IndexOf(",") - 1);
                    String[] wheel_commands = command.Split(',');
                    wheel_force = Array.ConvertAll<String, float>(wheel_commands, float.Parse);
                    time = 0;
                }
            }

            //Minimodbot should be able to go forward backward OR turn left right
            //If wheel front wheel force are both > 0 ----> forward
            //If Wheel front wheel force are both < - ----> backward
            //if Wheel front wheel has one thats < 0 and the other > 0 ----> you are turnt 
            //Probably repetition in the code, reminder to clean it in the future
            if (wheel_force[0] > 0 && wheel_force[1] > 0)       //forward backward
            {
                if (instantePower < 0)
                {
                    stopBot();
                }
                instantePower = Math.Abs(wheel_force[0] / 100) * motorPower * Time.deltaTime;
                wheelTurn = 0;
            }
            else if (wheel_force[0] < 0 && wheel_force[1] < 0)
            {
                if (instantePower > 0)
                {
                    stopBot();
                }
                instantePower = -Math.Abs(wheel_force[0] / 100) * motorPower * Time.deltaTime;
                wheelTurn = 0;
            }

            if (wheel_force[0] > 0 && wheel_force[1] < 0) {     //turning left right
                if (wheelTurn == 0)
                {
                    stopBot();
                }
                wheelTurn = Mathf.Min(1, wheel_force[0] / 50) * maxTurn;
                instantePower = 0;
            } else if (wheel_force[0] < 0 && wheel_force[1] > 0) {
                if (wheelTurn == 0)
                {
                    stopBot();
                }
                wheelTurn = -Mathf.Min(1, wheel_force[1] / 50) * maxTurn;
                instantePower = 0;
            }

            //Turn Collider
            getCollider(0).steerAngle = wheelTurn;
            getCollider(1).steerAngle = wheelTurn;

            //Turn Wheels
            //Not Needed, its just an animation for wheel turning 
            //wheels[0].localEulerAngles = new Vector3(wheels[0].localEulerAngles.x, getCollider(0).steerAngle - wheels[0].localEulerAngles.z + 90, wheels[0].localEulerAngles.z);
            //wheels[1].localEulerAngles = new Vector3(wheels[1].localEulerAngles.x, getCollider(1).steerAngle - wheels[0].localEulerAngles.z + 90, wheels[1].localEulerAngles.z);

            //Spin Wheels
            wheels[0].Rotate(0, -getCollider(0).rpm / 60 * 360 * Time.deltaTime, 0);
            wheels[1].Rotate(0, -getCollider(1).rpm / 60 * 360 * Time.deltaTime, 0);
            wheels[2].Rotate(0, -getCollider(2).rpm / 60 * 360 * Time.deltaTime, 0);
            wheels[3].Rotate(0, -getCollider(3).rpm / 60 * 360 * Time.deltaTime, 0);

            //brakes
            if (wheel_force[0] == 0 && wheel_force[1] == 0)
            {
                time += Time.deltaTime;
                if (time > 3f)
                {
                    stopBot();      //hard stop
                } else {
                    //Slows down velocity by half each time, for more "natural" stop
                    Vector3 prev = myRigidbody.velocity;
                    prev.z = prev.z / 2;
                    prev.x = prev.x / 2;
                    myRigidbody.velocity = prev;
                }
            } else {
                getCollider(0).brakeTorque = 0.0f;
                getCollider(1).brakeTorque = 0.0f;
                getCollider(2).brakeTorque = 0.0f;
                getCollider(3).brakeTorque = 0.0f;

                //physics for actual forward and backward movement
                getCollider(2).motorTorque = instantePower;
                getCollider(3).motorTorque = instantePower; 

                if (wheelTurn != 0)
                {
                    //Rotates rigidbody (minimodbot) in place
                    Transform pivot = myRigidbody.transform.Find("PivotPoint");
                    if (wheelTurn > 0)
                    {
                        myRigidbody.transform.RotateAround(pivot.position, Vector3.up, 1);
                    }
                    else
                    {
                        myRigidbody.transform.RotateAround(pivot.position, Vector3.up, -1);
                    }
                }
            }

        }

    }

    WheelCollider getCollider(int n)
    {
        return wheels[n].gameObject.GetComponent<WheelCollider>();
    }
}
