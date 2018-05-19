using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
	public GameObject m_FieldPrefab;
	public GameObject m_BorderlinePrefab;
	public GameObject m_OutsidePrefab;
	public GameObject m_LinePrefab;


	public Texture2D m_Texture;

	private int w = 320;
	private int h = 320;

	private int GridSize = 4;
	private GameObject[,] m_Map;
	private State[,] m_State; // Mapの種類(状態)を保存する

	public enum State
	{
		LINE,           // Markerが引く線  
		FIELD,          // Marker移動可能、敵陣
		AREA,           // Marker移動不可、自陣
		OUTSIDE,        // Marker移動不可、範囲外
		BORDER_LINE,    // Marker移動可能、境界線
		ADJACENT_LINE,  // Marker移動不可、隣接線
	}

	public void PreareMap()
	{
		// Map情報を初期化
		m_Map = new GameObject[w / GridSize, h / GridSize];
		m_State = new State[w / GridSize, h / GridSize];

		GameObject map = null;
		State state = State.FIELD;
		for (int x = 0; x < w / GridSize; x++)
		{
			for (int y = 0; y < h / GridSize; y++)
			{
				// Mapの初期状態の画像を読み込み、色情報からGameObjectを生成する
				Color color = m_Texture.GetPixel(x * GridSize, y * GridSize);            
				if (color.Equals(new Color(15.0f / 255.0f, 56.0f / 255.0f, 15.0f / 255.0f)))
				{
					state = State.BORDER_LINE;
					map = m_BorderlinePrefab;
				}

				if (color.Equals(new Color(155.0f / 255.0f, 188.0f / 255.0f, 15.0f / 255.0f)))
				{
					state = State.FIELD;
					map = m_FieldPrefab;
				}

				if (color.Equals(new Color(139.0f / 255.0f, 172.0f / 255.0f, 15.0f / 255.0f)))
				{
					state = State.OUTSIDE;
					map = m_OutsidePrefab;
				}

				//Debug.Log("(" + x*4 + "," + y*4 + ") " + state +"|"+color);
               
				Vector2 position = new Vector2((float)x * GridSize, (float)y * GridSize);
				position = Camera.main.ScreenToWorldPoint(position);
                m_Map[x, y] = Instantiate(map, position, Quaternion.identity);
				m_Map[x, y].transform.parent = transform;

				m_State[x, y] = state;
			}
		}
	}

	public State[,] GetState()
	{
		return m_State;
	}

	public bool CanMoveTo(Vector2 position)
	{
		position = Camera.main.WorldToScreenPoint(position);
		int x = (int)position.x / GridSize;
		int y = (int)position.y / GridSize;
		//Debug.Log("(" + x * 4 + "," + y * 4 + ") " + m_State[x, y]);

		switch (m_State[x, y])
		{
			case State.LINE:
			case State.AREA:
			case State.OUTSIDE:
			case State.ADJACENT_LINE:
				return false;

			case State.FIELD:
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					return true;
				}
				return false;

			case State.BORDER_LINE:
			default:
				return true;
		}
	}

	public void DrawLine(Vector2 position){
		position = Camera.main.WorldToScreenPoint(position);
        int x = (int)position.x / GridSize;
        int y = (int)position.y / GridSize;

		if(m_State[x, y] == State.BORDER_LINE){
			// NOP.
			return;
		}
        
		Destroy(m_Map[x, y]);
		m_Map[x, y] = Instantiate(m_LinePrefab, position, Quaternion.identity);
        m_Map[x, y].transform.parent = transform;

		m_State[x, y] = State.LINE;
	}
}
