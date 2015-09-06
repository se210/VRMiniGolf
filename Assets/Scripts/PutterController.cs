using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Runtime.InteropServices;

namespace UnityStandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof (Rigidbody))]
	public class PutterController : MonoBehaviour
	{
		[Serializable]
		public class MovementSettings
		{
			public float ForwardSpeed = 8.0f;   // Speed when walking forward
			public float BackwardSpeed = 4.0f;  // Speed when walking backwards
			public float StrafeSpeed = 4.0f;    // Speed when walking sideways
			public float RunMultiplier = 2.0f;   // Speed when sprinting
			public KeyCode RunKey = KeyCode.LeftShift;
			public float JumpForce = 30f;
			public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
			[HideInInspector] public float CurrentTargetSpeed = 8f;
			
			#if !MOBILE_INPUT
			private bool m_Running;
			#endif
			
			public void UpdateDesiredTargetSpeed(Vector2 input)
			{
				if (input == Vector2.zero) return;
				if (input.x > 0 || input.x < 0)
				{
					//strafe
					CurrentTargetSpeed = StrafeSpeed;
				}
				if (input.y < 0)
				{
					//backwards
					CurrentTargetSpeed = BackwardSpeed;
				}
				if (input.y > 0)
				{
					//forwards
					//handled last as if strafing and moving forward at the same time forwards speed should take precedence
					CurrentTargetSpeed = ForwardSpeed;
				}
				#if !MOBILE_INPUT
				if (Input.GetKey(RunKey))
				{
					CurrentTargetSpeed *= RunMultiplier;
					m_Running = true;
				}
				else
				{
					m_Running = false;
				}
				#endif
			}
			
			#if !MOBILE_INPUT
			public bool Running
			{
				get { return m_Running; }
			}
			#endif
		}
		
		
		[Serializable]
		public class AdvancedSettings
		{
			public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
			public float stickToGroundHelperDistance = 0.5f; // stops the character
			public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
			public bool airControl; // can the user control the direction that is being moved in the air
		}
		
		
		public Camera cam;
		public MovementSettings movementSettings = new MovementSettings();
		public MouseLook mouseLook = new MouseLook();
		public AdvancedSettings advancedSettings = new AdvancedSettings();
		
		
		private Rigidbody m_RigidBody;
		private CapsuleCollider m_Capsule;
		private float m_YRotation;
		private Vector3 m_GroundContactNormal;
		private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;

		[DllImport("WiiuseUnity")]
		private static extern bool WiimoteInit();

		[DllImport("WiiuseUnity")]
		private static extern void SetDebugLogFptr(IntPtr fp);

		[DllImport("WiiuseUnity")]
		private static extern void handleEvent();

		[DllImport("WiiuseUnity")]
		private static extern IntPtr Wiimote6DOF();

		[DllImport("WiiuseUnity")]
		private static extern void FreeMemory(IntPtr ptr);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void DebugLogDelegate(string str);

		static void DebugLog(string str)
		{
			Debug.Log("WiimoteTracking : " + str);
		}
		
		public Vector3 Velocity
		{
			get { return m_RigidBody.velocity; }
		}
		
		public bool Grounded
		{
			get { return m_IsGrounded; }
		}
		
		public bool Jumping
		{
			get { return m_Jumping; }
		}
		
		public bool Running
		{
			get
			{
				#if !MOBILE_INPUT
				return movementSettings.Running;
				#else
				return false;
				#endif
			}
		}
		
		
		private void Start()
		{
			m_RigidBody = GetComponent<Rigidbody>();
			mouseLook.Init (transform, cam.transform);

			// Setup so that DLL can call Unity's Debug.Log
			DebugLogDelegate callback_delegate = new DebugLogDelegate( DebugLog );
			
			// Convert callback_delegate into a function pointer that can be
			// used in unmanaged code.
			IntPtr intptr_delegate = 
				Marshal.GetFunctionPointerForDelegate(callback_delegate);
			
			// Call the API passing along the function pointer.
			SetDebugLogFptr( intptr_delegate );

			if (!WiimoteInit ())
			{
				Debug.Log("Wiimote initialization failed.");
			}
		}
		
		
		private void Update()
		{
		}
		
		
		private void FixedUpdate()
		{
			// Wiimote
			handleEvent();
			IntPtr pWiimote6dof = Wiimote6DOF();
			if (pWiimote6dof != null)
			{
				float[] result = new float[ 3 ];
				Marshal.Copy( pWiimote6dof, result, 0, 3 );
				FreeMemory(pWiimote6dof);

				transform.parent.localEulerAngles = new Vector3(result[2], 0,  -result[0]-97);
			}

			// Match direction to camera
			Vector3 localEulerAngles = transform.parent.localEulerAngles;
			localEulerAngles.y = cam.transform.localEulerAngles.y;
			transform.parent.localEulerAngles = localEulerAngles;
		}
		
		
		private float SlopeMultiplier()
		{
			float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
			return movementSettings.SlopeCurveModifier.Evaluate(angle);
		}
		
		
		private Vector2 GetInput()
		{
			
			Vector2 input = new Vector2
			{
				x = CrossPlatformInputManager.GetAxis("Horizontal"),
				y = CrossPlatformInputManager.GetAxis("Vertical")
			};
			movementSettings.UpdateDesiredTargetSpeed(input);
			return input;
		}
		
		
		private void RotateView()
		{
			//avoids the mouse looking if the game is effectively paused
			if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;
			
			// get the rotation before it's changed
			float oldYRotation = transform.eulerAngles.y;
			
			mouseLook.LookRotation (transform, cam.transform);
			
			if (m_IsGrounded || advancedSettings.airControl)
			{
				// Rotate the rigidbody velocity to match the new direction that the character is looking
				Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
				m_RigidBody.velocity = velRotation*m_RigidBody.velocity;
			}
		}

	}
}
