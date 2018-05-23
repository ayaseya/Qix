using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public Text m_OcupancyRate;
	public Text m_Message;

	public GameObject m_RemainingLives_1;
	public GameObject m_RemainingLives_2;
	public GameObject m_RemainingLives_3;

	private Marker m_Marker;
	private Map m_Map;
	private Qix m_Qix;

	private int m_RemainingLives = 3;
	private float mClearPer = 75.0f;

	private bool m_IsPlaying = false;

	private float m_TimeElapsed;

	private Vector3 m_LineStartPoint; // 線引き中に被弾した場合、線引き開始地点に戻すために使用


	void Awake()
	{
		m_Marker = GameObject.FindWithTag("Marker").GetComponent<Marker>();
		m_Map = GameObject.FindWithTag("Map").GetComponent<Map>();

		//m_Map.PreareMap();
		m_Qix = GameObject.FindWithTag("Qix").GetComponent<Qix>();
	}

	void Start()
	{
		StartCoroutine(GameLoop());
	}

	void Update()
	{
		// ゲームプレイ中以外はキー入力を排他する
		if (!m_IsPlaying)
		{
			return;
		}

		// 移動可能か確認
		if (m_Map.CanMoveTo(m_Marker.transform.position, m_Marker.GetPrevousPosition()))
		{
			// 移動
			m_Marker.Move();

			// Shiftキーが押下中は線引きモード
			if (IsShiftKeyPressed())
			{
				m_Map.DrawLine(m_Marker.transform.position);
			}
		}

		// Markerを回転
		m_Marker.Turn();
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

	// Mapからのコールバック、線引き開始め時に呼び出される
	public void OnDrawLineStarted()
	{
		m_LineStartPoint = m_Marker.GetPrevousPosition();
	}

	// Mapからのコールバック、線引き終了時に呼び出される
	public void OnDrawLineDone()
	{
		// 塗りつぶしを開始
		m_Map.FillClosedArea(m_Qix.transform.position);

		// 15フレームだけ移動を停止する
		m_Qix.TemporaryStop(15);
	}

	// Mapからのコールバック、塗りつぶしが終わったら呼び出される
	public void OnFillClosedAreaDone()
	{
		// 占有率を更新
		m_OcupancyRate.text = m_Map.GetOcupancyRate().ToString("f1") + " %";
	}

	// QixとMarker(もしくは引いている線)が接触した時に呼び出される
	public void OnQixTouched()
	{
		// 短時間に続けて呼び出された場合に2回目以降の呼び出しをスキップする
		m_TimeElapsed += Time.deltaTime;
		Debug.Log(m_TimeElapsed);
		if (m_TimeElapsed > 0.0167f)
		{
			// 処理をスキップ  
			return;
		}

		StartCoroutine(ResetTimer());

		// 被弾演出
		m_Marker.TakeDamage();

		// 線引き開始地点にMarkerを戻す
		m_Marker.transform.position = m_Map.IsDrawing() ?
			m_LineStartPoint :
			m_Marker.GetPrevousPosition();

		// 線を消す
		m_Map.EraseLine();

		// 残機を減少させる
		m_RemainingLives--;

		UpdateRemainingLives();
	}

	public IEnumerator ResetTimer()
	{
		yield return new WaitForSeconds(0.5f);
		Debug.Log("ResetTimer");

		m_TimeElapsed = 0;
	}


	private IEnumerator GameLoop()
	{
		Debug.Log("Loop() E");
		yield return StartCoroutine(Prepare());
		yield return StartCoroutine(Play());

		if (m_Map.GetOcupancyRate() > mClearPer)
		{
			yield return StartCoroutine(GameClear());
		}
		else
		{
			yield return StartCoroutine(GameOver());
		}
		Debug.Log("Loop() X");
	}

	private IEnumerator Prepare()
	{
		Debug.Log("GamePrepare() E");
		m_Message.text = "PRESS SPACE KEY";
		m_Map.PreareMap();
		m_OcupancyRate.text = m_Map.GetOcupancyRate().ToString("f1") + " %";
		m_Marker.transform.position = new Vector3(2, -126, 0);
		m_Qix.transform.position = new Vector3(2, 96, 0);
		m_RemainingLives = 3;
		UpdateRemainingLives();

		while (!Input.GetKey(KeyCode.Space))
		{
			yield return null;
		}
		m_Message.text = "";
		m_Qix.Resume();
		Debug.Log("GamePrepare() X");
		yield return null;
	}

	private IEnumerator Play()
	{
		Debug.Log("Play() E");
		m_IsPlaying = true;
		// 占領率が75%を超えた時、もしくは残機が残っていない時にゲームを中断する
		while (m_Map.GetOcupancyRate() < mClearPer && m_RemainingLives > 0)
		{
			// 毎フレーム上記条件を満たす限りループする
			yield return null;
		}
		m_Qix.Pause();
		m_IsPlaying = false;
		Debug.Log("Play() X");
	}

	private IEnumerator GameClear()
	{
		Debug.Log("GameClear");
		m_Message.text = "GAME CLEAR!";
		yield return new WaitForSeconds(3.0f);
		StartCoroutine(GameLoop());
	}

	private IEnumerator GameOver()
	{
		Debug.Log("GameOver");
		m_Message.text = "GAME OVER!";
		yield return new WaitForSeconds(3.0f);
		StartCoroutine(GameLoop());
	}

    // 残機表示を更新
	private void UpdateRemainingLives()
	{
		if (m_RemainingLives == 3)
		{
			m_RemainingLives_1.GetComponent<Renderer>().enabled = true;
			m_RemainingLives_2.GetComponent<Renderer>().enabled = true;
			m_RemainingLives_3.GetComponent<Renderer>().enabled = true;
		}
		else if (m_RemainingLives == 2)
		{
			m_RemainingLives_3.GetComponent<Renderer>().enabled = false;
		}
		else if (m_RemainingLives == 1)
		{
			m_RemainingLives_2.GetComponent<Renderer>().enabled = false;
		}
		else if (m_RemainingLives == 0)
		{
			m_RemainingLives_1.GetComponent<Renderer>().enabled = false;
		}
	}
}
