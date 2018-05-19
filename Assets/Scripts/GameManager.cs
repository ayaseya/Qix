using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private Marker m_Marker;
	private Map m_Map;


	void Awake()
	{
		m_Marker = GameObject.FindWithTag("Marker").GetComponent<Marker>();
		m_Map = GameObject.FindWithTag("Map").GetComponent<Map>();
		m_Map.PreareMap();
	}

	void Start()
	{

	}

	void Update()
	{
		if (m_Map.CanMoveTo(m_Marker.GetNextMovePosition()))
		{
			m_Marker.Move();
		}
		m_Marker.Turn();

		if (IsShiftKeyPressed())
		{
			m_Map.DrawLine(m_Marker.transform.position);
		}
	}

	// Shiftキーが押下中か確認する
	private bool IsShiftKeyPressed()
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			return true;
		}
		return false;
	}
}
