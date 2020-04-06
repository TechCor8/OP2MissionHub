using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface
{
	/// <summary>
	/// Navigates selectables like InputField. User can press Tab to select the next index and Shift-Tab to select the previous index.
	/// 
	/// Usage:
	/// Attach script to a UI form game object and assign the selectables to the NavList array in the inspector.
	/// 
	/// Only one TabNavigation component should be active at a time.
	/// </summary>
	public class TabNavigation : MonoBehaviour
	{
		[SerializeField] private Selectable[] m_NavList			= default;


		// Use this for initialization
		void Awake()
		{
			if (m_NavList.Length == 0)
				throw new System.Exception("TabNavigation list is empty!");
		}

		// Update is called once per frame
		void Update()
		{
			if (!Input.GetKeyDown(KeyCode.Tab))
				return;

			if (m_NavList.Length == 0)
				return;

			int selectedIndex = GetSelectedIndex();

			// Allow up to (NavList.Length) passes for finding an active selectable
			for (int passes=0; passes < m_NavList.Length; ++passes)
			{
				if (Input.GetKey(KeyCode.LeftShift))
					--selectedIndex;
				else
					++selectedIndex;

				// Keep index in list bounds
				if (selectedIndex < 0)
					selectedIndex = m_NavList.Length - 1;
				else if (selectedIndex >= m_NavList.Length)
					selectedIndex = 0;

				if (!m_NavList[selectedIndex].gameObject.activeInHierarchy)		continue;
				if (!m_NavList[selectedIndex].IsInteractable())					continue;

				m_NavList[selectedIndex].Select();
				break;
			}
		}

		private int GetSelectedIndex()
		{
			for (int i=0; i < m_NavList.Length; ++i)
			{
				if (m_NavList[i].gameObject == EventSystem.current.currentSelectedGameObject)
					return i;
			}

			return -1;
		}
	}
}
