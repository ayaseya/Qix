﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	private float speed = 1f;

	void Start()
	{

	}

	void Update()
	{
		// 移動と回転
		Move();
		Turn();

	}

	// 移動
	private void Move()
	{
		// プレイヤーの座標を取得
		Vector2 position = transform.position;

		// 移動量を加える
		// 斜め移動を制限するため、いずれかの方向キーの入力のみ処理する
		if (Input.GetKey(KeyCode.UpArrow))
		{
			position.y += speed * Time.deltaTime;
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			position.y -= speed * Time.deltaTime;
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			position.x -= speed * Time.deltaTime;
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			position.x += speed * Time.deltaTime;
		}

		// 画面左下のワールド座標をビューポートから取得
		Vector2 min = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));

		// 画面右上のワールド座標をビューポートから取得
		Vector2 max = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

		// プレイヤーの位置が画面内に収まるように制限をかける
		position.x = Mathf.Clamp(position.x, min.x, max.x);
		position.y = Mathf.Clamp(position.y, min.y, max.y);

		// 制限をかけた値をプレイヤーの位置とする
		transform.position = position;
	}

	// 回転
	private void Turn()
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
}
