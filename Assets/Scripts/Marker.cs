using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
	private float m_Speed = 4f;
	private Vector3 m_PrevPosition;
	private SpriteRenderer m_Renderer;

	void Start()
	{
		// 移動前の初期位置を保存
		m_PrevPosition = transform.position;
		m_Renderer = GetComponent<SpriteRenderer>();

	}

	// 移動
	public void Move()
	{
		// プレイヤーの座標を取得
		Vector3 position = transform.position;

		// 移動前の位置を保存する
		m_PrevPosition = position;

		// 移動量を加える
		// 斜め移動を制限するため、いずれかの方向キーの入力のみ処理する
		if (Input.GetKey(KeyCode.UpArrow))
		{
			position.y += m_Speed;
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			position.y -= m_Speed;
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			position.x -= m_Speed;
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			position.x += m_Speed;
		}

		transform.position = position;
	}

	// 回転
	public void Turn()
	{
		// キー方向にプレイヤーを回転させる
		if (Input.GetKey(KeyCode.UpArrow))
		{
			transform.rotation = Quaternion.Euler(0, 0, 0);
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			transform.rotation = Quaternion.Euler(0, 0, 180);
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			transform.rotation = Quaternion.Euler(0, 0, 90);
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			transform.rotation = Quaternion.Euler(0, 0, 270);
		}
	}

	// 一つ前の座標を返す
	public Vector3 GetPrevousPosition()
	{
		return m_PrevPosition;
	}

	public void TakeDamage()
	{
		// 点滅によるダメージ演出
		StartCoroutine(Blink());
	}

	private IEnumerator Blink()
	{
		// 変更前の色のコピーを保存
		var original = m_Renderer.color;

		// 白色に光らせる
		m_Renderer.color = new Color(1, 0, 0);

		// 0.1秒待機して元の色に戻す
		yield return new WaitForSeconds(0.1f);
		m_Renderer.color = original;
	}
}
