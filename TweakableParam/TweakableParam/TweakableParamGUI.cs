using System;
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

		public List<TweakableParamGUIGroup> m_guiGroups = new List<TweakableParamGUIGroup>();
		private List<TweakableParamGUIItem> m_guiItems = new List<TweakableParamGUIItem>();

		public void RegisterGUIItem(TweakableParamGUIItem item)
		{
			if (m_guiItems.Count == 0)
				AddGUI();

			if (m_guiItems.Contains(item)) return;
			m_guiItems.Add(item);

			Part controllerPart = null;
			if (item.m_controller is ModuleTweakableParam)
				controllerPart = (item.m_controller as ModuleTweakableParam).part;
			else if (item.m_controller is ModuleTweakableSubParam)
				controllerPart = (item.m_controller as ModuleTweakableSubParam).parentModule.part; 
			
			foreach (TweakableParamGUIGroup group in m_guiGroups)
			{
				
				if (group.m_part == controllerPart)
				{
					// Already has a group of the same part.
					group.m_guiItems.Add(item);
					return;
				}
			}
			
			TweakableParamGUIGroup newGroup = new TweakableParamGUIGroup(this, controllerPart);
			m_guiGroups.Add(newGroup);
			newGroup.m_guiItems.Add(item);
		}

		public void UnregisterGUIItem(TweakableParamGUIItem item)
		{
			if (m_guiItems.Contains(item))
				m_guiItems.Remove(item);

			if (m_guiItems.Count == 0) DeleteGUI();

			Part controllerPart = null;
			if (item.m_controller is ModuleTweakableParam)
				controllerPart = (item.m_controller as ModuleTweakableParam).part;
			else if (item.m_controller is ModuleTweakableSubParam)
				controllerPart = (item.m_controller as ModuleTweakableSubParam).parentModule.part;

			foreach (TweakableParamGUIGroup group in m_guiGroups)
			{
				if (group.m_part == controllerPart)
				{
					// Already has a group of the same part.
					group.m_guiItems.Remove(item);
					if (group.m_guiItems.Count == 0)
					{
						m_guiGroups.Remove(group);
					}
					return;
				}
			}
		}

		public void ClearGUIItem()
		{
			m_guiGroups.Clear();
			m_guiItems.Clear();
			DeleteGUI();
		}

		public void CheckClear()
		{
			Debug.Log("CheckClear()");
			for (int i = 0; i < m_guiGroups.Count; ++i)
			{
				for (int j = 0; j < m_guiGroups[i].m_guiItems.Count; ++j)
				{
					if (m_guiGroups[i].m_guiItems[j] == null || m_guiGroups[i].m_guiItems[j].CheckValid() == false)
					{
						m_guiGroups[i].m_guiItems.RemoveAt(j);
						--j;
					}
				}
			}

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
			GUI.skin = HighLogic.Skin;
			
			if (windowUpdated)
			{
				WindowPos.width = WindowPos.height = 10;
				windowUpdated = false;
			}

			WindowPos = GUILayout.Window(2121316, WindowPos, WindowFunc, "Tweakable Parameters", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(200));
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
				GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(Screen.width / 4), GUILayout.MaxWidth(Screen.width / 3));
			else
				GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(Screen.width / 4), GUILayout.MaxWidth(Screen.width / 3), GUILayout.MinHeight(Screen.height / 2));
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
						foreach (TweakableParamGUIGroup group in m_guiGroups)
						{
							group.RenderGUI();
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

	class TweakableParamGUIGroup
	{
		public TweakableParamGUI m_gui = null;
		public Part m_part = null;
		public List<TweakableParamGUIItem> m_guiItems = new List<TweakableParamGUIItem>();

		#region GUI Related
		public bool isExpanded = false;
		#endregion

		public TweakableParamGUIGroup(TweakableParamGUI gui, Part part)
		{
			m_gui = gui;
			m_part = part;
		}

		public void RenderGUI()
		{
			bool isExpandButtonClicked = false;

			if (m_gui == null || m_part == null) return;

			GUIStyle sty = new GUIStyle(GUI.skin.button);
			sty.normal.textColor = sty.focused.textColor = Color.white;
			sty.hover.textColor = sty.active.textColor = Color.yellow;
			sty.onNormal.textColor = sty.onFocused.textColor = sty.onHover.textColor = sty.onActive.textColor = Color.green;
			sty.padding = new RectOffset(4, 4, 4, 4);

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			{
				isExpandButtonClicked = GUILayout.Button((isExpanded ? "<" : ">"), sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
				GUILayout.Label(m_part.partInfo.title, sty, GUILayout.ExpandWidth(true));
			}
			GUILayout.EndHorizontal();

			if (isExpanded)
			{
				m_part.SetHighlightType(Part.HighlightType.AlwaysOn);
				m_part.SetHighlight(true);

				foreach (TweakableParamGUIItem item in m_guiItems)
				{
					if (item.CheckAttached())
						item.RenderGUI();
				}
			}
			else
			{
				if (m_part.highlightType == Part.HighlightType.AlwaysOn)
				{
					m_part.SetHighlightType(Part.HighlightType.OnMouseOver);
					m_part.SetHighlight(false);
				}
			}

			if (isExpandButtonClicked)
				isExpanded = !isExpanded;
			if (isExpanded == true)
			{
				foreach (TweakableParamGUIGroup group in m_gui.m_guiGroups)
				{
					if (group != this)
					{
						group.isExpanded = false;
						if (group.m_part.highlightType == Part.HighlightType.AlwaysOn)
						{
							group.m_part.SetHighlightType(Part.HighlightType.OnMouseOver);
							group.m_part.SetHighlight(false);
						}
					}
				}
			}
		}

	}

	class TweakableParamGUIItem
	{
		public TweakableParamGUI m_gui = null;
		public object m_controller = null;

		public TweakableParamGUIItem(TweakableParamGUI gui, object controller)
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

			if (this.m_controller is ModuleTweakableParam)
			{
				if ((this.m_controller as ModuleTweakableParam).part == null)
					return false;
			}
			else if (this.m_controller is ModuleTweakableSubParam)
			{
				if ((this.m_controller as ModuleTweakableSubParam).parentModule.part == null)
					return false;
			}

			return true;
		}

		public bool CheckAttached()
		{
			if (CheckValid() == false) return false;

			Part part = null;
			if (this.m_controller is ModuleTweakableParam)
				part = (this.m_controller as ModuleTweakableParam).part;
			else if (this.m_controller is ModuleTweakableSubParam)
				part = (this.m_controller as ModuleTweakableSubParam).parentModule.part;
			if (part.localRoot == EditorLogic.startPod)
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
			bool isIncreaseRepeatButtonClicked = false;
			bool isDecreaseRepeatButtonClicked = false;

			GUIStyle sty = new GUIStyle(GUI.skin.button);
			sty.normal.textColor = sty.focused.textColor = Color.white;
			sty.hover.textColor = sty.active.textColor = Color.yellow;
			sty.onNormal.textColor = sty.onFocused.textColor = sty.onHover.textColor = sty.onActive.textColor = Color.green;
			sty.padding = new RectOffset(4, 4, 4, 4);

			if (m_controller is ModuleTweakableParam)
			{
				ModuleTweakableParam controller = m_controller as ModuleTweakableParam;
				GUILayout.Label("Field: " + controller.targetField, sty, GUILayout.ExpandWidth(true));
				GUILayout.Label("Adjustment Range: (" + controller.minValue.ToString() + " - " + controller.maxValue.ToString() + "), Step: " + controller.stepValue.ToString(), sty, GUILayout.ExpandWidth(true));
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				{
					isDecreaseRepeatButtonClicked = GUILayout.RepeatButton("--", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(35.0f));
					isDecreaseButtonClicked = GUILayout.Button("-", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
					GUILayout.Box(Math.Round(controller.tweakedValue, 2).ToString("F2"), sty, GUILayout.ExpandWidth(true));
					isIncreaseButtonClicked = GUILayout.Button("+", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
					isIncreaseRepeatButtonClicked = GUILayout.RepeatButton("++", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(35.0f));
				}
				GUILayout.EndHorizontal();

				if (isDecreaseButtonClicked || isDecreaseRepeatButtonClicked)
					controller.DecreaseValue();
				if (isIncreaseButtonClicked || isIncreaseRepeatButtonClicked)
					controller.IncreaseValue();
			}
			else
			{
				ModuleTweakableSubParam controller = m_controller as ModuleTweakableSubParam;
				GUILayout.Label("Field: " + controller.targetField, sty, GUILayout.ExpandWidth(true));
				GUILayout.Label("Adjustment Range: (" + controller.minValue.ToString() + " - " + controller.maxValue.ToString() + "), Step: " + controller.stepValue.ToString(), sty, GUILayout.ExpandWidth(true));
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				{
					isDecreaseRepeatButtonClicked = GUILayout.RepeatButton("--", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(35.0f));
					isDecreaseButtonClicked = GUILayout.Button("-", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
					GUILayout.Box(Math.Round(controller.tweakedValue, 2).ToString("F2"), sty, GUILayout.ExpandWidth(true));
					isIncreaseButtonClicked = GUILayout.Button("+", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
					isIncreaseRepeatButtonClicked = GUILayout.RepeatButton("++", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(35.0f));
				}
				GUILayout.EndHorizontal();

				if (isDecreaseButtonClicked || isDecreaseRepeatButtonClicked)
					controller.DecreaseValue();
				if (isIncreaseButtonClicked || isIncreaseRepeatButtonClicked)
					controller.IncreaseValue();
			}
		}
	}
}
