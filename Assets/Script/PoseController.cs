using UnityEngine;
using System.Collections;
using Tango;
using System;
using UnityEngine.UI;

public class PoseController : MonoBehaviour , ITangoPose {
	public Text countText;
	public Text winText;
	private int count;

	private TangoApplication m_tangoApplication; // Instance for Tango Client
	private Vector3 m_tangoPosition; // Position from Pose Callback
	private Quaternion m_tangoRotation; // Rotation from Pose Callback
	private Vector3 m_startPosition; // Start Position of the camera
	
	private float m_movementScale = 10.0f;
	
	// Use this for initialization
	void Start ()
	{
		// Initialize some variables
		m_tangoRotation = Quaternion.identity;
		m_tangoPosition = Vector3.zero;
		m_startPosition = transform.position;
		m_tangoApplication = FindObjectOfType<TangoApplication>();
		if(m_tangoApplication != null)
		{
			// Request Tango permissions
			m_tangoApplication.RegisterPermissionsCallback(PermissionsCallback);
			m_tangoApplication.RequestNecessaryPermissionsAndConnect();
			m_tangoApplication.Register(this);
		}
		else
		{
			Debug.Log("No Tango Manager found in scene.");
		}
		count = 0;
		SetCountText ();
		winText.text = " ";
	}
	
	// Permissions callback
	private void PermissionsCallback(bool success)
	{
		if(success)
		{
			m_tangoApplication.InitApplication(); // Initialize Tango Client
			m_tangoApplication.InitProviders(string.Empty); // Initialize listeners
			m_tangoApplication.ConnectToService(); // Connect to Tango Service
		}
		else
		{
			AndroidHelper.ShowAndroidToastMessage("Motion Tracking Permissions Needed", true);
		}
	}
	
	// Pose callbacks from Project Tango
	public void OnTangoPoseAvailable(Tango.TangoPoseData pose)
	{
		// Do nothing if we don't get a pose
		if (pose == null)
		{
			Debug.Log("TangoPoseData is null.");
			return;
		}
		// The callback pose is for device with respect to start of service pose.
		if (pose.framePair.baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE &&
		    pose.framePair.targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE)
		{
			if (pose.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
			{
				// Cache the position and rotation to be set in the update function.
				// This needs to be done because this callback does not
				// happen in the main game thread.
				m_tangoPosition = new Vector3((float)pose.translation [0],
				                              (float)pose.translation [1],
				                              (float)pose.translation [2]);
				
				m_tangoRotation = new Quaternion((float)pose.orientation [0],
				                                 (float)pose.orientation [1],
				                                 (float)pose.orientation [2],
				                                 (float)pose.orientation [3]);
			}
			else // if the current pose is not valid we set the pose to identity
			{
				m_tangoPosition = Vector3.zero;
				m_tangoRotation = Quaternion.identity;
			}
		}
	}
	
	/// <summary>
	/// Transforms the Tango pose which is in Start of Service to Device frame to Unity coordinate system.
	/// </summary>
	/// <returns>The Tango Pose in unity coordinate system.</returns>
	/// <param name="translation">Translation.</param>
	/// <param name="rotation">Rotation.</param>
	/// <param name="scale">Scale.</param>
	Matrix4x4 TransformTangoPoseToUnityCoordinateSystem(Vector3 translation, Quaternion rotation, Vector3 scale)
	{
		// Matrix for Tango coordinate frame to Unity coordinate frame conversion.
		// Start of service frame with respect to Unity world frame.
		Matrix4x4 m_uwTss;
		// Unity camera frame with respect to device frame.
		Matrix4x4 m_dTuc;
		
		m_uwTss = new Matrix4x4();
		m_uwTss.SetColumn (0, new Vector4 (1.0f, 0.0f, 0.0f, 0.0f));
		m_uwTss.SetColumn (1, new Vector4 (0.0f, 0.0f, 1.0f, 0.0f));
		m_uwTss.SetColumn (2, new Vector4 (0.0f, 1.0f, 0.0f, 0.0f));
		m_uwTss.SetColumn (3, new Vector4 (0.0f, 0.0f, 0.0f, 1.0f));
		
		m_dTuc = new Matrix4x4();
		m_dTuc.SetColumn (0, new Vector4 (1.0f, 0.0f, 0.0f, 0.0f));
		m_dTuc.SetColumn (1, new Vector4 (0.0f, 1.0f, 0.0f, 0.0f));
		m_dTuc.SetColumn (2, new Vector4 (0.0f, 0.0f, -1.0f, 0.0f));
		m_dTuc.SetColumn (3, new Vector4 (0.0f, 0.0f, 0.0f, 1.0f));
		
		Matrix4x4 ssTd = Matrix4x4.TRS(translation, rotation, scale);
		return m_uwTss * ssTd * m_dTuc;
	}
	
	// Update is called once per frame
	void Update()
	{
		Matrix4x4 uwTuc = TransformTangoPoseToUnityCoordinateSystem(m_tangoPosition, m_tangoRotation, Vector3.one);
		
		// Extract new local position
		transform.position = (uwTuc.GetColumn(3))* m_movementScale;
		transform.position = transform.position + m_startPosition;
		
		// Extract new local rotation
		transform.rotation = Quaternion.LookRotation(uwTuc.GetColumn(2), uwTuc.GetColumn(1));
	}
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag ("PickUp")) 
		{
			other.gameObject.SetActive (false);
			count+=1;
			SetCountText ();
		}
	}
	
	void SetCountText()
	{
		countText.text = "Count: " + count.ToString ();
		if (count >= 12) {
			winText.text = "You win!!!";
		}
	}
}
