using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TweakableParam
{
	class TweakableFloatCurveEditor
	{
		private static TweakableFloatCurveEditor s_singleton = null;
		public static TweakableFloatCurveEditor GetInstance() { return CreateInstance(); }
		private static TweakableFloatCurveEditor CreateInstance() { if (s_singleton == null) s_singleton = new TweakableFloatCurveEditor(); return s_singleton; }

		#region GUI Related
		Rect WindowPos = new Rect(Screen.width - 350, 400, 10, 10);
		bool windowUpdated = false;
		bool editorLocked = false;
		bool m_visible = false;
		Texture2D canvas = new Texture2D(200, 200);
		bool canvasInvalidated = false;
		int recordIndex;
		string recordKeyStr;
		string recordValueStr;
		float recordKey;
		float recordValue;
		bool xAxisUseLog = false;
		#endregion

		public List<Vector4> m_values = new List<Vector4>();
		public FloatCurve m_curve = new FloatCurve();

		public ModuleTweakableSubParam receiver = null;

		public void AddKeyValuePair(float key, float value, float tanIn = 0.0f, float tanOut = 0.0f)
		{
			m_values.Add(new Vector4(key, value, tanIn, tanOut));
			canvasInvalidated = true;
		}

		public void EditKeyValuePair(int index, float newKey, float newValue, float newTanIn = 0.0f, float newTanOut = 0.0f)
		{
			m_values[index] = new Vector4(newKey, newValue, newTanIn, newTanOut);
			canvasInvalidated = true;
		}

		public void RemoveKeyValuePair(int index)
		{
			m_values.RemoveAt(index);
			canvasInvalidated = true;
		}

		public void SetReceiver(ModuleTweakableSubParam receiver)
		{
			this.receiver = receiver;
			m_values.Clear();
			for (int i = 0; i < receiver.tweakedCurve.Count / 2; i++)
			{
				AddKeyValuePair(receiver.tweakedCurve[i * 2], receiver.tweakedCurve[i * 2 + 1]);
			}
			canvasInvalidated = true;
		}

		public void SubmitDataToReceiver()
		{
			if (receiver != null)
			{
				receiver.ClearCurvePoints();
				for (int i = 0; i < m_values.Count; ++i)
				{
					receiver.AddCurvePoint(m_values[i].x, m_values[i].y);
				}
			}
		}

		public void UpdateCurve()
		{
			m_curve = new FloatCurve();
			foreach(Vector4 record in m_values)
			{
				m_curve.Add(record.x, record.y, record.z, record.w);
			}

			// Clear the background to black.
			for (int i = 0; i < canvas.width; ++i)
			{
				for (int j = 0; j < canvas.height; ++j)
				{
					canvas.SetPixel(i, j, Color.black);
				}
			}

			// Render the border.
			for (int i = 0; i < canvas.width; ++i)
			{
				canvas.SetPixel(i, 0, Color.white);
			}
			for (int j = 0; j < canvas.width; ++j)
			{
				canvas.SetPixel(0, j, Color.white);
			}
			for (int i = 0; i < canvas.width; ++i)
			{
				canvas.SetPixel(i, canvas.height - 1, Color.white);
			}
			for (int j = 0; j < canvas.width; ++j)
			{
				canvas.SetPixel(canvas.width - 1, j, Color.white);
			}

			// Prepare min/max.
			float minX = float.MaxValue;
			float maxX = float.MinValue;
			float minY = float.MaxValue;
			float maxY = float.MinValue;
			foreach(Vector4 record in m_values)
			{
				if (minX > record.x) minX = record.x;
				if (maxX < record.x) maxX = record.x;
				if (minY > record.y) minY = record.y;
				if (maxY < record.y) maxY = record.y;
			}
			
			float minXLog = Mathf.Sign(minX) * Mathf.Log10(Mathf.Abs(minX) + 1);
			float maxXLog = Mathf.Sign(maxX) * Mathf.Log10(Mathf.Abs(maxX) + 1);

			// Render the curve.
			if(m_values.Count > 0)
			{
				float prevProgressY = float.NaN;
				for (int i = 1; i < canvas.width - 2; ++i)
				{
					float progressX = (i - 1.0f) / (canvas.width - 2.0f);
					float actualX = Mathf.Lerp(minX, maxX, progressX);
					if(xAxisUseLog)
					{
						actualX = Mathf.Lerp(minXLog, maxXLog, progressX);
						actualX = Mathf.Sign(actualX) * (Mathf.Pow(10, Math.Abs(actualX)) - 1.0f);
					}

					float actualY = m_curve.Evaluate(actualX);
					//Debug.Log("TFCE: Eval(" + actualX.ToString("F2") + ") = " + actualY.ToString("F2"));
					float progressY = (actualY - minY) / (maxY - minY);
					if (float.IsNaN(prevProgressY))
					{
						// This is the first point.
						prevProgressY = progressY;
					}
					else
					{
						if (Mathf.Abs(progressY - prevProgressY) >= 1.5f / (canvas.height - 2.0f))
						{ 
							// We need to insert more mid-points to smooth the curve.
							if(progressY > prevProgressY)
							{
								for(float y = prevProgressY; y < progressY; y += 1.0f / (canvas.height - 2.0f))
								{
									if(y - prevProgressY < (progressY - prevProgressY) / 2.0)
										canvas.SetPixel(i - 1, (int)(y * (canvas.height - 2.0f) + 1.0f), Color.green);
									else
										canvas.SetPixel(i, (int)(y * (canvas.height - 2.0f) + 1.0f), Color.green);
								}
							}
							else
							{
								for (float y = prevProgressY; y > progressY; y -= 1.0f / (canvas.height - 2.0f))
								{
									if (y - progressY > (prevProgressY - progressY) / 2.0)
										canvas.SetPixel(i - 1, (int)(y * (canvas.height - 2.0f) + 1.0f), Color.green);
									else
										canvas.SetPixel(i, (int)(y * (canvas.height - 2.0f) + 1.0f), Color.green);
								}
							}
						}
						prevProgressY = progressY;
					}
					canvas.SetPixel(i, (int)(progressY * (canvas.height - 2.0f) + 1.0f), Color.green);
				}
			}

			canvas.Apply();
			canvasInvalidated = false;
		}

		#region GUI Codes
		public void AddGUI()
		{
			if (m_visible == true) return;

			// Show the GUI.
			Debug.Log("FloatCurveEditor: AddGUI");
			m_visible = true;
			RenderingManager.AddToPostDrawQueue(3, DrawGUI);
		}

		public void DeleteGUI()
		{
			if (m_visible == false) return;

			// Hide the GUI.
			Debug.Log("FloatCurveEditor: DeleteGUI");
			m_visible = false;
			if (editorLocked)
			{
				EditorLogic.fetch.Unlock("TFCE");
				editorLocked = false;
			} 
			RenderingManager.RemoveFromPostDrawQueue(3, DrawGUI);
		}

		public void DrawGUI()
		{
			GUI.skin = HighLogic.Skin;

			if (windowUpdated)
			{
				WindowPos.width = WindowPos.height = 10;
				windowUpdated = false;
			}

			WindowPos = GUILayout.Window(2121319, WindowPos, WindowFunc, "Float Curve Editor", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			Vector3 mousePos = Input.mousePosition;         //Mouse location; based on Kerbal Engineer Redux code
			mousePos.y = Screen.height - mousePos.y;
			bool cursorInGUI = WindowPos.Contains(mousePos);
			//This locks and unlocks the editor as necessary; cannot constantly call the lock or unlock functions as that causes the editor to be constantly locked
			if (cursorInGUI && !editorLocked)
			{
				EditorLogic.fetch.Lock(true, true, true, "TFCE");
				editorLocked = true;
			}
			else if (!cursorInGUI && editorLocked)
			{
				EditorLogic.fetch.Unlock("TFCE");
				editorLocked = false;
			}
		}

		public void WindowFunc(int id)
		{
			bool isOkClicked = false;
			bool isCancelClicked = false;

			bool isPrevClicked = false;
			bool isNextClicked = false;
			bool isAddClicked = false;
			bool isEditClicked = false;
			bool isRemoveClicked = false;

			GUI.skin = HighLogic.Skin;
			GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			{
				if (canvasInvalidated == true)
					UpdateCurve();
				GUILayout.Label(canvas, GUILayout.Width(200), GUILayout.Height(200));
				
				bool newXAxisUseLog = GUILayout.Toggle(xAxisUseLog, "Use ln() on X-axis", GUILayout.ExpandWidth(true));
				if (xAxisUseLog != newXAxisUseLog) canvasInvalidated = true;
				xAxisUseLog = newXAxisUseLog;
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				{
					// Index Selector, Two Input Boxes.
					isPrevClicked = GUILayout.Button("<-", GUILayout.Width(40));
					isNextClicked = GUILayout.Button("->", GUILayout.Width(40));
					if (isPrevClicked == true)
					{
						recordIndex--;
						if(recordIndex < 0)
							recordIndex = m_values.Count - 1;
					}
					if (isNextClicked == true)
					{
						recordIndex++;
						recordIndex = recordIndex % m_values.Count;
					}

					recordKey = m_values[recordIndex].x;
					recordValue = m_values[recordIndex].y;
					recordKeyStr = recordKey.ToString("F2");
					recordValueStr = recordValue.ToString("F2");
					Single.TryParse(GUILayout.TextField(recordKeyStr, GUILayout.ExpandWidth(true)), out recordKey);
					recordKeyStr = recordKey.ToString("F2");
					Single.TryParse(GUILayout.TextField(recordValueStr, GUILayout.ExpandWidth(true)), out recordValue);
					recordValueStr = recordValue.ToString("F2"); 
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				{ 
					// Add, Edit, Remove Buttons.
					//isAddClicked = GUILayout.Button("Add", GUILayout.ExpandWidth(true));
					//isEditClicked = GUILayout.Button("Edit", GUILayout.ExpandWidth(true));
					//isRemoveClicked = GUILayout.Button("Remove", GUILayout.ExpandWidth(true));
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				{
					// Ok & Cancel.
					GUILayout.FlexibleSpace();
					isOkClicked = GUILayout.Button("OK", GUILayout.Width(80));
					isCancelClicked = GUILayout.Button("Cancel", GUILayout.Width(80));
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUI.DragWindow(new Rect(0, 0, 2000, 30));
			
			if (isOkClicked == true || isCancelClicked == true)
			{
				if (isOkClicked == true)
					SubmitDataToReceiver();
				receiver = null;
				canvasInvalidated = true;
				m_values.Clear();
				DeleteGUI();
			}
		}
		#endregion

	}
}
