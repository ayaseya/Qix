using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private Marker m_Marker;
	private Painter m_Painter;

	void Awake()
	{
		m_Marker = GameObject.FindWithTag("Marker").GetComponent<Marker>();

		Texture2D texture = GameObject.FindWithTag("Encampment")
							  .GetComponent<SpriteRenderer>()
							  .sprite
							  .texture;

		m_Painter = GetComponent<Painter>();
		m_Painter.SetTexture(texture);
	}

	void Start()
	{

	}

	void Update()
	{
		m_Marker.Move();
		m_Marker.Turn();

		if (IsShiftKeyPressed())
		{
			bool success =
				m_Painter.LineDraw(m_Marker.transform.position,
								   m_Marker.GetPreviousPosition());
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
