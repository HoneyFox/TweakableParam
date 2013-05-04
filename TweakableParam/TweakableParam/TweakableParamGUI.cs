﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TweakableParam
{
	class TweakableParamGUI
	{
		private static TweakableParamGUI s_singleton = null;
		public static TweakableParamGUI GetInstance() { return CreateInstance(); }
		private static TweakableParamGUI CreateInstance() { if (s_singleton == null) s_singleton = new TweakableParamGUI(); return s_singleton; }

		#region GUI Related
		Rect WindowPos = new Rect(Screen.width - 350, 10, 10, 10);
		bool isMinimized = false;
		bool windowUpdated = false;
		bool editorLocked = false;
		Vector2 verticalScroll = Vector2.zero;
		#endregion

		private List<TweakableParamGUIItem> m_guiItems = new List<TweakableParamGUIItem>();

		public void RegisterGUIItem(TweakableParamGUIItem item)
		{
			if (m_guiItems.Count == 0)
				AddGUI();

			if (m_guiItems.Contains(item)) return;
			m_guiItems.Add(item);
		}

		public void UnregisterGUIItem(TweakableParamGUIItem item)
		{
			if (m_guiItems.Contains(item))
				m_guiItems.Remove(item);

			if (m_guiItems.Count == 0) DeleteGUI();
		}

		public void ClearGUIItem()
		{
			m_guiItems.Clear();
			DeleteGUI();
		}

		public void CheckClear()
		{
			Debug.Log("CheckClear()");
			for (int i = 0; i < m_guiItems.Count; ++i)
			{
				if (m_guiItems[i] == null)
				{
					m_guiItems.RemoveAt(i);
					--i;
				}
				else if (m_guiItems[i].CheckValid() == false)
				{
					m_guiItems.RemoveAt(i);
					--i;
				}
			}
			if (m_guiItems.Count == 0)
			{
				Debug.Log("Nothing left.");
				DeleteGUI();
			}
		}

		private void AddGUI()
		{
			Debug.Log("AddGUI");
			RenderingManager.AddToPostDrawQueue(3, DrawGUI);
		}

		private void DeleteGUI()
		{
			Debug.Log("DeleteGUI");
			RenderingManager.RemoveFromPostDrawQueue(3, DrawGUI);
		}

		public void DrawGUI()
		{
			if (windowUpdated)
			{
				WindowPos.width = WindowPos.height = 10;
				windowUpdated = false;
			}

			WindowPos = GUILayout.Window(2121316, WindowPos, WindowFunc, "Tweakable Parametrs", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(200));
			Vector3 mousePos = Input.mousePosition;         //Mouse location; based on Kerbal Engineer Redux code
			mousePos.y = Screen.height - mousePos.y;
			bool cursorInGUI = WindowPos.Contains(mousePos);
			//This locks and unlocks the editor as necessary; cannot constantly call the lock or unlock functions as that causes the editor to be constantly locked
			if (cursorInGUI && !editorLocked && !EditorLogic.editorLocked)
			{
				EditorLogic.fetch.Lock(true, true, true);
				editorLocked = true;
			}
			else if (!cursorInGUI && editorLocked && EditorLogic.editorLocked)
			{
				EditorLogic.fetch.Unlock();
				editorLocked = false;
			}
		}

		public void WindowFunc(int id)
		{
			bool isButtonClicked = false;
			if (isMinimized)
			{
				GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(Screen.width / 4), GUILayout.MaxWidth(Screen.width / 3));
			}
			else
			{
				GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(Screen.width / 4), GUILayout.MaxWidth(Screen.width / 3), GUILayout.MinHeight(Screen.height / 2));
			}
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(Screen.width / 4));
				{
					isButtonClicked = GUILayout.Button((isMinimized ? "Restore" : "Minimize"), GUILayout.ExpandWidth(true));
				}
				GUILayout.EndHorizontal();

				for (int i = 0; i < m_guiItems.Count; ++i)
				{
					TweakableParamGUIItem item = m_guiItems[i];
					if (item.CheckValid() == false)
					{
						item.UnregisterGUI(this);
						--i;
					}
				}

				if (isMinimized == false)
				{
					verticalScroll = GUILayout.BeginScrollView(verticalScroll, GUILayout.ExpandWidth(true), GUILayout.MinWidth(Screen.width / 4), GUILayout.ExpandHeight(true), GUILayout.MinHeight(Screen.height / 8), GUILayout.MaxHeight(Screen.height / 2));
					{
						foreach (TweakableParamGUIItem item in m_guiItems)
						{
							if (item.CheckAttached())
								item.RenderGUI();
						}
					}
					GUILayout.EndScrollView();
				}
			}
			GUILayout.EndVertical();

			GUI.DragWindow(new Rect(0, 0, 2000, 30));

			if (isButtonClicked)
			{
				isMinimized = !isMinimized;
				windowUpdated = true;
			}
		}
	}

	class TweakableParamGUIItem
	{
		public TweakableParamGUI m_gui = null;
		public ModuleTweakableParam m_controller = null;

		#region GUI Related
		bool isExpanded = false;
		#endregion

		public TweakableParamGUIItem(TweakableParamGUI gui, ModuleTweakableParam controller)
		{
			m_gui = gui;
			m_controller = controller;

			RegisterGUI(gui);
		}

		public void RegisterGUI(TweakableParamGUI gui)
		{
			TweakableParamGUI.GetInstance().RegisterGUIItem(this);
		}

		public void UnregisterGUI(TweakableParamGUI gui)
		{
			TweakableParamGUI.GetInstance().UnregisterGUIItem(this);
		}

		public bool CheckValid()
		{
			if (this.m_controller == null)
				return false;

			if (this.m_controller.part == null)
				return false;

			return true;
		}

		public bool CheckAttached()
		{
			if (CheckValid() == false) return false;

			if (this.m_controller.part.localRoot == EditorLogic.startPod)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public void RenderGUI()
		{
			bool isIncreaseButtonClicked = false;
			bool isDecreaseButtonClicked = false;
			bool isExpandButtonClicked = false;

			GUIStyle sty = new GUIStyle(GUI.skin.button);
			sty.normal.textColor = sty.focused.textColor = Color.white;
			sty.hover.textColor = sty.active.textColor = Color.yellow;
			sty.onNormal.textColor = sty.onFocused.textColor = sty.onHover.textColor = sty.onActive.textColor = Color.green;
			sty.padding = new RectOffset(4, 4, 4, 4);

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			{
				isExpandButtonClicked = GUILayout.Button((isExpanded ? "-" : "+"), sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
				GUILayout.Label(m_controller.part.partInfo.title, sty, GUILayout.ExpandWidth(true));
			}
			GUILayout.EndHorizontal();
			if (isExpanded)
			{
				m_controller.part.SetHighlightType(Part.HighlightType.AlwaysOn);
				m_controller.part.SetHighlight(true);
				GUILayout.Label("Field: " + m_controller.targetField, sty, GUILayout.ExpandWidth(true));
				GUILayout.Label("Adjustment Range: (" + m_controller.minValue.ToString() + " - " + m_controller.maxValue.ToString() + "), Step: " + m_controller.stepValue.ToString(), sty, GUILayout.ExpandWidth(true));
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				{
					isDecreaseButtonClicked = GUILayout.Button("-", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
					GUILayout.Box(m_controller.tweakedValue.ToString(), sty, GUILayout.ExpandWidth(true));
					isIncreaseButtonClicked = GUILayout.Button("+", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));

				}
				GUILayout.EndHorizontal();
			}
			else
			{
				if (m_controller.part.highlightType == Part.HighlightType.AlwaysOn)
				{
					m_controller.part.SetHighlightType(Part.HighlightType.OnMouseOver);
					m_controller.part.SetHighlight(false);
				}
			}

			if (isExpandButtonClicked)
				isExpanded = !isExpanded;
			if (isDecreaseButtonClicked)
				m_controller.DecreaseValue();
			if (isIncreaseButtonClicked)
				m_controller.IncreaseValue();
		}
	}
}
