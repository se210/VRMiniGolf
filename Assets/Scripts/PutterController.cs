using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Runtime.InteropServices;
using System.IO.Ports;

namespace UnityStandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof (Rigidbody))]
	public class PutterController : MonoBehaviour
	{
		public Camera cam;
		public MouseLook mouseLook = new MouseLook();

		private CapsuleCollider m_Capsule;
		private float m_YRotation;
		private Vector3 m_GroundContactNormal;
		private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;

		private float targetAngle;

		SerialPort sp = new SerialPort("COM4", 9600);

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
		
		private void Start()
		{
			mouseLook.Init (transform, cam.transform);
			targetAngle = 0.0f;

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

			sp.Open();
		}
		
		
		private void Update()
		{
//			String strIn = sp.ReadLine();
//			Debug.Log(strIn);
		}
		
		
		private void FixedUpdate()
		{
			// Wiimote
			handleEvent();
			IntPtr pWiimote6dof = Wiimote6DOF();
			if (pWiimote6dof != IntPtr.Zero)
			{
				float[] result = new float[ 3 ];
				Marshal.Copy( pWiimote6dof, result, 0, 3 );
				FreeMemory(pWiimote6dof);

//				transform.parent.localEulerAngles = new Vector3(0, 0,  -result[0]-97);
				targetAngle = -result[0]-97;
				Vector3 angles = transform.parent.localEulerAngles;
				angles.z = Mathf.LerpAngle(angles.z, targetAngle, Time.deltaTime);
				transform.parent.localEulerAngles = angles;

				sp.Write(((int) angles.z).ToString() + " ");
			}

			// Match direction to camera
			Vector3 localEulerAngles = transform.parent.localEulerAngles;
			localEulerAngles.y = cam.transform.localEulerAngles.y;
			transform.parent.localEulerAngles = localEulerAngles;
		}

		void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.name == "GolfBall")
			{
				GameController.gameController.strokes++;
				GameController.gameController.mStrokeStarted = true;
				sp.Write(12345.ToString() + " ");
			}
		}

	}
}
