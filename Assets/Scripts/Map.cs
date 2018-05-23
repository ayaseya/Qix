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
	public GameObject m_AreaPrefab;

	public Texture2D m_Texture;

	public UnityEvent OnDrawLineDone;
	public UnityEvent OnFillClosedAreaDone;
	public UnityEvent OnDrawLineStarted;

	private int w = 320;
	private int h = 320;

	private int m_GridSize = 4;
	private GameObject[,] m_Prefab;
	private Type[,] m_Type; // Mapの種類を保存する
	private bool m_IsDrawing = false;
	private int m_InitialFieldValue; // 敵陣の初期マス数をカウント、占領率の計算に使用

	enum Type
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
		if (m_Prefab == null)
		{
			m_Prefab = new GameObject[w / m_GridSize, h / m_GridSize];
		}
		m_Type = new Type[w / m_GridSize, h / m_GridSize];

		GameObject prefab = null;
		Type type = Type.FIELD;
		m_InitialFieldValue = 0;
		for (int x = 0; x < w / m_GridSize; x++)
		{
			for (int y = 0; y < h / m_GridSize; y++)
			{
				// Mapの初期状態の画像を読み込み、色情報からGameObjectを生成する
				Color color = m_Texture.GetPixel(x * m_GridSize, y * m_GridSize);
				if (color.Equals(new Color(15.0f / 255.0f, 56.0f / 255.0f, 15.0f / 255.0f)))
				{
					type = Type.BORDER_LINE;
					prefab = m_BorderlinePrefab;
				}

				if (color.Equals(new Color(155.0f / 255.0f, 188.0f / 255.0f, 15.0f / 255.0f)))
				{
					type = Type.FIELD;
					prefab = m_FieldPrefab;
					m_InitialFieldValue++;
				}

				if (color.Equals(new Color(139.0f / 255.0f, 172.0f / 255.0f, 15.0f / 255.0f)))
				{
					type = Type.OUTSIDE;
					prefab = m_OutsidePrefab;
				}

				//Debug.Log("(" + x*4 + "," + y*4 + ") " + state +"|"+color);

				Vector2 position = new Vector2((float)x * m_GridSize, (float)y * m_GridSize);
				position = Camera.main.ScreenToWorldPoint(position);

				// 2週目以降に前回プレイ時のMapを破棄する
				Destroy(m_Prefab[x, y]);

				m_Prefab[x, y] = Instantiate(prefab, position, Quaternion.identity);
				m_Prefab[x, y].transform.parent = transform;

				m_Type[x, y] = type;
			}
		}
	}

	// 指定したマスが移動可能か確認する
	public bool CanMoveTo(Vector3 position, Vector3 previousPosition)
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

		Vector3 sp = Camera.main.WorldToScreenPoint(position);
		int x = (int)sp.x / m_GridSize;
		int y = (int)sp.y / m_GridSize;
		//Debug.Log("(" + x * 4 + "," + y * 4 + ") " + m_State[x, y]);

		switch (m_Type[x, y])
		{
			case Type.LINE:
				if (m_IsDrawing)
				{
					if (position.Equals(previousPosition))
					{ // 線引き中のバックを禁止
						return false;
					}
					if (m_Prefab[x, y].CompareTag("Line"))
					{
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

			case Type.AREA:
			case Type.OUTSIDE:
				return false;

			case Type.FIELD:
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					return true;
				}
				return false;

			case Type.BORDER_LINE:
				if (IsAdjacentLine(x, y))
				{
					return false;
				}
				return true;

			default:
				return true;
		}
	}

	// 隣接する周囲8マスにFIELDが存在しなければ隣接線(Markerが入れない、)
	private bool IsAdjacentLine(int x, int y)
	{
		return CountSquares(x, y, Type.FIELD) == 0;
	}

	// 隣接する周囲8マスに指定したStateのマスが幾つ存在するか確認する
	private int CountSquares(int x, int y, Type state)
	{
		int count = 0;
		if (m_Type[x, y + 1] == state)      // 上
		{
			count++;
		}
		else if (m_Type[x, y - 1] == state) // 下
		{
			count++;
		}
		else if (m_Type[x - 1, y] == state) // 左
		{
			count++;
		}
		else if (m_Type[x + 1, y] == state) // 右
		{
			count++;
		}
		else if (m_Type[x + 1, y + 1] == state) // 右斜め上
		{
			count++;
		}
		else if (m_Type[x + 1, y - 1] == state) // 右斜め下
		{
			count++;
		}
		else if (m_Type[x - 1, y + 1] == state) // 左斜め上         
		{
			count++;
		}
		else if (m_Type[x - 1, y - 1] == state) // 左斜め下
		{
			count++;
		}
		return count;
	}

	// 線を引く
	public void DrawLine(Vector3 position)
	{
		Vector3 sp = Camera.main.WorldToScreenPoint(position);
		int x = (int)sp.x / m_GridSize;
		int y = (int)sp.y / m_GridSize;

		if (CanDrawPoint(x, y) && !m_IsDrawing)
		{
			//Debug.Log("開始");
			m_IsDrawing = true;
			OnDrawLineStarted.Invoke();
		}

		if (!CanDrawPoint(x, y))
		{
			if (m_IsDrawing)
			{
				//Debug.Log("終了");
				m_IsDrawing = false;
				OnDrawLineDone.Invoke();
			}
			return;
		}

		Destroy(m_Prefab[x, y]);

		m_Prefab[x, y] = Instantiate(m_LinePrefab, position, Quaternion.identity);
		m_Prefab[x, y].transform.parent = transform;

		m_Type[x, y] = Type.LINE;
	}

	// 指定座標が塗り潰せる(線を引く)か確認する
	private bool CanDrawPoint(int x, int y)
	{
		if (m_Type[x, y] == Type.BORDER_LINE)
		{
			return false;
		}
		if (m_Type[x, y] == Type.LINE)
		{
			return false;
		}
		return true;
	}

	// 線で囲った閉領域(QIXが存在しない)を塗りつぶす
	public void FillClosedArea(Vector3 qixPosition)
	{
		// FIELDを一旦NONEに置き換える
		for (int x = 0; x < w / m_GridSize; x++)
		{
			for (int y = 0; y < h / m_GridSize; y++)
			{
				if (m_Type[x, y] == Type.FIELD)
				{
					m_Type[x, y] = Type.NONE;
				}
			}
		}

		// Qixの位置をワールド座標からスクリーン座標に変換
		Vector3 sp = Camera.main.WorldToScreenPoint(qixPosition);
		int qx = (int)sp.x / m_GridSize;
		int qy = (int)sp.y / m_GridSize;

		// Qixの座標を起点にFIELDに塗り潰す
		FillClosedArea(qx, qy, Type.FIELD);

		// 残ったNONEが線で囲った閉領域となるのでAREAに変更する
		for (int x = 0; x < w / m_GridSize; x++)
		{
			for (int y = 0; y < h / m_GridSize; y++)
			{
				if (m_Type[x, y] == Type.NONE)
				{
					m_Type[x, y] = Type.AREA;
					Vector2 position = new Vector2((float)x * m_GridSize, (float)y * m_GridSize);
					position = Camera.main.ScreenToWorldPoint(position);
					Destroy(m_Prefab[x, y]);
					m_Prefab[x, y] = Instantiate(m_AreaPrefab, position, Quaternion.identity);
					m_Prefab[x, y].transform.parent = transform;
				}
				// 塗り潰した閉領域のLineのtagは削除することで
				// Qixの当たり判定の対象外となる            
				if (m_Type[x, y] == Type.LINE)
				{
					m_Prefab[x, y].tag = "Untagged";
				}
			}
		}
		OnFillClosedAreaDone.Invoke();
	}

	// 指定した座標を起点に閉領域を塗り潰す
	private void FillClosedArea(int x, int y, Type state)
	{
		m_Type[x, y] = state;

		if (m_Type[x, y + 1] == Type.NONE) // 上に描き込み可能な領域があるか
			FillClosedArea(x, y + 1, state); // あればその座標で再帰呼び出し

		if (m_Type[x + 1, y] == Type.NONE) // 右
			FillClosedArea(x + 1, y, state);

		if (m_Type[x, y - 1] == Type.NONE) // 下
			FillClosedArea(x, y - 1, state);

		if (m_Type[x - 1, y] == Type.NONE) // 左
			FillClosedArea(x - 1, y, state);
	}

	public float GetOcupancyRate()
	{
		int count = 0;
		for (int x = 0; x < w / m_GridSize; x++)
		{
			for (int y = 0; y < h / m_GridSize; y++)
			{
				if (m_Type[x, y] == Type.AREA)
				{
					count++;
				}
			}
		}
		return (float)count / m_InitialFieldValue * 100;
	}

	// 線を消す
	public void EraseLine()
	{
		if (m_IsDrawing)
		{
			for (int x = 0; x < w / m_GridSize; x++)
			{
				for (int y = 0; y < h / m_GridSize; y++)
				{
					if (m_Type[x, y] == Type.LINE && m_Prefab[x, y].CompareTag("Line"))
					{
						Vector2 position = new Vector2((float)x * m_GridSize, (float)y * m_GridSize);
						position = Camera.main.ScreenToWorldPoint(position);
						Destroy(m_Prefab[x, y]);
						m_Prefab[x, y] = Instantiate(m_FieldPrefab, position, Quaternion.identity);
						m_Prefab[x, y].transform.parent = transform;
						m_Type[x, y] = Type.FIELD;
					}
				}
			}
			m_IsDrawing = false;
		}
	}

	public bool IsDrawing()
	{
		return m_IsDrawing;
	}
}