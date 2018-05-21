using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Map : MonoBehaviour
{
	public GameObject m_FieldPrefab;
	public GameObject m_BorderlinePrefab;
	public GameObject m_OutsidePrefab;
	public GameObject m_LinePrefab;
	public GameObject mAreaPrefab;

	public Texture2D m_Texture;

	public UnityEvent OnDrawLineDone;
	public UnityEvent OnFillClosedAreaDone;

	private int w = 320;
	private int h = 320;

	private int m_GridSize = 4;
	private GameObject[,] m_Map;
	private State[,] m_State; // Mapの種類(状態)を保存する
	private bool IsDrawing = false;
	private int mInitialFieldValue;

	enum State
	{
		NONE,
		LINE,           // Marker移動可能(ただし線を引き終わっていない場合は戻れない)、Markerが引く線  
		FIELD,          // Marker移動可能、敵陣
		AREA,           // Marker移動不可、自陣
		OUTSIDE,        // Marker移動不可、範囲外
		BORDER_LINE,    // Marker移動可能、境界線
	}

	public void PreareMap()
	{
		// Map情報を初期化
		// 画面サイズは320x320ピクセル、1マスあたり4ピクセルとして扱う
		// 1マス毎にMap情報を格納する
		// 格納する情報は320/4の80x80の二次元配列となる
		m_Map = new GameObject[w / m_GridSize, h / m_GridSize];
		m_State = new State[w / m_GridSize, h / m_GridSize];

		GameObject map = null;
		State state = State.FIELD;
		for (int x = 0; x < w / m_GridSize; x++)
		{
			for (int y = 0; y < h / m_GridSize; y++)
			{
				// Mapの初期状態の画像を読み込み、色情報からGameObjectを生成する
				Color color = m_Texture.GetPixel(x * m_GridSize, y * m_GridSize);
				if (color.Equals(new Color(15.0f / 255.0f, 56.0f / 255.0f, 15.0f / 255.0f)))
				{
					state = State.BORDER_LINE;
					map = m_BorderlinePrefab;
				}

				if (color.Equals(new Color(155.0f / 255.0f, 188.0f / 255.0f, 15.0f / 255.0f)))
				{
					state = State.FIELD;
					map = m_FieldPrefab;
					mInitialFieldValue++;
				}

				if (color.Equals(new Color(139.0f / 255.0f, 172.0f / 255.0f, 15.0f / 255.0f)))
				{
					state = State.OUTSIDE;
					map = m_OutsidePrefab;
				}

				//Debug.Log("(" + x*4 + "," + y*4 + ") " + state +"|"+color);

				Vector2 position = new Vector2((float)x * m_GridSize, (float)y * m_GridSize);
				position = Camera.main.ScreenToWorldPoint(position);
				m_Map[x, y] = Instantiate(map, position, Quaternion.identity);
				m_Map[x, y].transform.parent = transform;

				m_State[x, y] = state;
			}
		}
	}

	// 指定したマスが移動可能か確認する
	public bool CanMoveTo(Vector2 position, Vector2 previousPosition)
	{
		if (Input.GetKey(KeyCode.UpArrow))
		{
			position.y += m_GridSize;
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			position.y -= m_GridSize;
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			position.x -= m_GridSize;
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			position.x += m_GridSize;
		}
		else
		{
			// 方向キーが押されていない
			return false;
		}

		Vector2 sp = Camera.main.WorldToScreenPoint(position);
		int x = (int)sp.x / m_GridSize;
		int y = (int)sp.y / m_GridSize;
		//Debug.Log("(" + x * 4 + "," + y * 4 + ") " + m_State[x, y]);

		switch (m_State[x, y])
		{
			case State.LINE:
				if (IsDrawing)
				{
					if (position.Equals(previousPosition))
					{ // 線引き中のバックを禁止
						return false;
					}
					if(m_Map[x,y].CompareTag("Line")){
						// 閉じていない線と継なげるのは禁止
						return false;
					}
					return true;
				}
				else
				{
					if (IsAdjacentLine(x, y))
					{
						return false;
					}
					return true;
				}

			case State.AREA:
			case State.OUTSIDE:
				return false;

			case State.FIELD:
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					return true;
				}
				return false;

			case State.BORDER_LINE:
				if (IsAdjacentLine(x, y))
				{
					return false;
				}
				return true;

			default:
				return true;
		}
	}

	// 隣接する周囲8マスにFIELDが存在しなければ隣接線
	private bool IsAdjacentLine(int x, int y)
	{
		return CountSquares(x, y, State.FIELD) == 0;
	}

	// 隣接する周囲8マスに指定したStateのマスが幾つ存在するか確認する
	private int CountSquares(int x, int y, State state)
	{
		int count = 0;
		if (m_State[x, y + 1] == state)      // 上
		{
			count++;
		}
		else if (m_State[x, y - 1] == state) // 下
		{
			count++;
		}
		else if (m_State[x - 1, y] == state) // 左
		{
			count++;
		}
		else if (m_State[x + 1, y] == state) // 右
		{
			count++;
		}
		else if (m_State[x + 1, y + 1] == state) // 右斜め上
		{
			count++;
		}
		else if (m_State[x + 1, y - 1] == state) // 右斜め下
		{
			count++;
		}
		else if (m_State[x - 1, y + 1] == state) // 左斜め上         
		{
			count++;
		}
		else if (m_State[x - 1, y - 1] == state) // 左斜め下
		{
			count++;
		}
		return count;
	}

	// 線を引く
	public void DrawLine(Vector2 position)
	{
		Vector2 sp = Camera.main.WorldToScreenPoint(position);
		int x = (int)sp.x / m_GridSize;
		int y = (int)sp.y / m_GridSize;

		if (CanDrawPoint(x, y) && !IsDrawing)
		{
			//Debug.Log("開始");
			IsDrawing = true;
		}

		if (!CanDrawPoint(x, y))
		{
			if (IsDrawing)
			{
				//Debug.Log("終了");
				IsDrawing = false;
				OnDrawLineDone.Invoke();
			}
			return;
		}

		Destroy(m_Map[x, y]);

		m_Map[x, y] = Instantiate(m_LinePrefab, position, Quaternion.identity);
		m_Map[x, y].transform.parent = transform;

		m_State[x, y] = State.LINE;
	}

	// 指定座標が塗り潰せる(線を引く)か確認する
	private bool CanDrawPoint(int x, int y)
	{
		if (m_State[x, y] == State.BORDER_LINE)
		{
			return false;
		}
		if (m_State[x, y] == State.LINE)
		{
			return false;
		}
		return true;
	}

	// 線で囲った閉領域(QIXが存在しない)を塗りつぶす
	public void FillClosedArea(Vector2 qixPosition)
	{
		// FIELDを一旦NONEに置き換える
		for (int x = 0; x < w / m_GridSize; x++)
		{
			for (int y = 0; y < h / m_GridSize; y++)
			{
				if (m_State[x, y] == State.FIELD)
				{
					m_State[x, y] = State.NONE;
				}
			}
		}

		// Qixの位置をワールド座標からスクリーン座標に変換
		Vector2 sp = Camera.main.WorldToScreenPoint(qixPosition);
		int qx = (int)sp.x / m_GridSize;
		int qy = (int)sp.y / m_GridSize;

		// Qixの座標を起点にFIELDに塗り潰す
		FillClosedArea(qx, qy, State.FIELD);

		// 残ったNONEが線で囲った閉領域となるのでAREAに変更する
		for (int x = 0; x < w / m_GridSize; x++)
		{
			for (int y = 0; y < h / m_GridSize; y++)
			{
				if (m_State[x, y] == State.NONE)
				{
					m_State[x, y] = State.AREA;
					Vector2 position = new Vector2((float)x * m_GridSize, (float)y * m_GridSize);
					position = Camera.main.ScreenToWorldPoint(position);
					m_Map[x, y] = Instantiate(mAreaPrefab, position, Quaternion.identity);
					m_Map[x, y].transform.parent = transform;
				}
                // 塗り潰した閉領域のLineのtagは削除することで
                // Qixの当たり判定の対象外となる            
				if(m_State[x, y] == State.LINE){
					m_Map[x, y].tag = "Untagged";
				}

			}
		}

		OnFillClosedAreaDone.Invoke();
	}

	// 指定した座標を起点に閉領域を塗り潰す
	private void FillClosedArea(int x, int y, State state)
	{
		m_State[x, y] = state;

		if (m_State[x, y + 1] == State.NONE) // 上に描き込み可能な領域があるか
			FillClosedArea(x, y + 1, state); // あればその座標で再帰呼び出し

		if (m_State[x + 1, y] == State.NONE) // 右
			FillClosedArea(x + 1, y, state);

		if (m_State[x, y - 1] == State.NONE) // 下
			FillClosedArea(x, y - 1, state);

		if (m_State[x - 1, y] == State.NONE) // 左
			FillClosedArea(x - 1, y, state);
	}

	public float GetOcupancyRate()
	{
		int count = 0;
		for (int x = 0; x < w / m_GridSize; x++)
		{
			for (int y = 0; y < h / m_GridSize; y++)
			{
				if (m_State[x, y] == State.AREA)
				{
					count++;
				}
			}
		}
		return (float)count / mInitialFieldValue * 100;
	}

	public void OnDestroy()
    {
		for (int x = 0; x < w / m_GridSize; x++)
        {
            for (int y = 0; y < h / m_GridSize; y++)
            {
				Destroy(m_Map[x, y]);
            }
        }
    }
}