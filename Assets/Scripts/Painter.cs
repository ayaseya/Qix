using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Painter : MonoBehaviour
{
	private Brush m_Brush;
	private Texture2D m_Texture;

	enum Direction
	{
		None,
		Vertical,
		Horizontal,
	}

	void Awake()
	{
		m_Brush = new Brush();
	}

	public void SetTexture(Texture2D texture2D)
	{
		m_Texture = texture2D;
	}

	public bool LineDraw(Vector2 p1, Vector2 p2)
	{
		if (!m_Texture)
		{
			// 描画先のTexture2Dが設定されていない
			return false;
		}

		// ワールド座標からスクリーン座標に変換
		p1 = Camera.main.WorldToScreenPoint(p1);
		p2 = Camera.main.WorldToScreenPoint(p2);
		//Debug.Log("p1:" + p1 + " p2:" + p2);

		switch (GetDrawDirection(p1, p2))
		{
			case Direction.None:
				// 2点が同じ座標だったので描画しない
				return false;

			case Direction.Horizontal:
				DrawHorizontalLine(p1, p2);
				break;

			case Direction.Vertical:
				DrawVerticalLine(p1, p2);
				break;
		}
		return true;
	}

	// 2点の座標から描画方向を取得する
	private Direction GetDrawDirection(Vector2 p1, Vector2 p2)
	{
		// xy方向の移動量を求める
		int dx = Mathf.Abs((int)p1.x - (int)p2.x);
		int dy = Mathf.Abs((int)p1.y - (int)p2.y);

		if (dy == 0)
		{
			return Direction.Horizontal;
		}
		else if (dx == 0)
		{
			return Direction.Vertical;
		}
		return Direction.None;
	}

    // 横線
	private void DrawHorizontalLine(Vector2 p1, Vector2 p2)
	{
		// 始点と終点を求める
		int sp = (int)p1.x < (int)p2.x ? (int)p1.x : (int)p2.x;
		int ep = (int)p1.x > (int)p2.x ? (int)p1.x : (int)p2.x;

		// 始点から終点まで繰り返しTexture2Dに書き込む
		for (int i = sp; i <= ep; i++)
		{
			m_Texture.SetPixels(i, (int)p1.y,
			                    m_Brush.blockWidth,
			                    m_Brush.blockHeight,
			                    m_Brush.colors);
		}

		// Texture2Dに反映する
		m_Texture.Apply();
	}

    // 縦線
	private void DrawVerticalLine(Vector2 p1, Vector2 p2)
	{
		// 始点と終点を求める
		int sp = (int)p1.y < (int)p2.y ? (int)p1.y : (int)p2.y;
		int ep = (int)p1.y > (int)p2.y ? (int)p1.y : (int)p2.y;

		// 始点から終点まで繰り返しTexture2Dに書き込む
		for (int i = sp; i <= ep; i++)
		{
			m_Texture.SetPixels((int)p1.x, i,
			                    m_Brush.blockWidth,
			                    m_Brush.blockHeight,
			                    m_Brush.colors);
		}

		// Texture2Dに反映する
		m_Texture.Apply();
	}

	// ブラシ
	private class Brush
	{
		public int blockWidth = 4;
		public int blockHeight = 4;
		public Color color = Color.gray;
		public Color[] colors;

		public Brush()
		{
			colors = new Color[blockWidth * blockHeight];
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = color;
			}
		}
	}
}