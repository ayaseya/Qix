using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public Text m_OcupancyRate;

	private Marker m_Marker;
	private Map m_Map;
	private Qix m_Qix;

	private int m_RemainingLives = 3;
	private float mClearPer = 10.0f;

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

	// Mapからのコールバック、線が引き終わったら呼び出される
	public void OnDrawLineDone()
	{
		// 塗りつぶしを開始
		m_Map.FillClosedArea(m_Qix.transform.position);

		m_Qix.OnPause();
	}

	// Mapからのコールバック、塗りつぶしが終わったら呼び出される
	public void OnFillClosedAreaDone()
	{
		// 占有率を更新
		m_OcupancyRate.text = m_Map.GetOcupancyRate().ToString("f1") + " %";
	}

	public void OnQixTouched()
	{
		m_RemainingLives--;
	}

	private IEnumerator GameLoop()
	{
		Debug.Log("Loop() E");
		yield return StartCoroutine(OnGamePrepare());
		yield return StartCoroutine(OnGamePlay());
		yield return StartCoroutine(OnGamePause());


		if (m_Map.GetOcupancyRate() > mClearPer)
		{
			yield return StartCoroutine(OnGameClear());
		}
		else
		{
			if (m_RemainingLives <= 0)
			{
				yield return StartCoroutine(OnGameOver());
			}
			else
			{
				yield return StartCoroutine(OnGameResume());
			}
		}
		Debug.Log("Loop() X");
	}

	private IEnumerator OnGamePrepare()
	{
		Debug.Log("GamePrepare");
		m_Map.PreareMap();
		m_OcupancyRate.text = m_Map.GetOcupancyRate().ToString("f1") + " %";
		m_Marker.transform.position = new Vector3(2, -126, 0);
		m_Qix.transform.position = new Vector3(0, 0, 0);
		m_Qix.AddInitialForce();
		yield return new WaitForSeconds(1.0f);
	}

	private IEnumerator OnGamePlay()
	{
		Debug.Log("GamePlay:");
		while (m_Map.GetOcupancyRate() < mClearPer&&m_RemainingLives > 1)
		{
			//|| m_RemainingLives > 1
			Debug.Log("GamePlay:" + m_Map.GetOcupancyRate() + " " + m_RemainingLives);

			yield return null;
		}
	}

	private IEnumerator OnGamePause()
	{
		Debug.Log("GamePause");
		yield return new WaitForSeconds(1.0f);
	}

	private IEnumerator OnGameResume()
	{
		Debug.Log("GameResume");
		yield return new WaitForSeconds(1.0f);
	}

	private IEnumerator OnGameClear()
	{
		Debug.Log("GameClear");
		//StartCoroutine(GameLoop());
		yield return new WaitForSeconds(1.0f);
	}

	private IEnumerator OnGameOver()
	{
		Debug.Log("GameOver");
		//StartCoroutine(GameLoop());
		yield return new WaitForSeconds(1.0f);
	}
}
