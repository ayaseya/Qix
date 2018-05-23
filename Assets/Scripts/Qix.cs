using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Qix : MonoBehaviour
{
	private Vector3 m_LastVelocity;
	private Rigidbody2D m_Rigidbody;

	public UnityEvent OnQixTouched;

	private bool m_IsPausing = false;

	void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate()
	{
		m_LastVelocity = m_Rigidbody.velocity;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Line")||
		    collision.gameObject.CompareTag("Marker"))
		{
			OnQixTouched.Invoke();
		}

		Vector3 refrectVec = Vector3.Reflect(m_LastVelocity, collision.contacts[0].normal);
		m_Rigidbody.velocity = refrectVec;
	}

	// 符号(+-)を取得する
	private int GetRandamSign()
	{
		return Random.Range(0, 1) == 0 ? -1 : 1;
	}

	public void Resume()
	{
		m_IsPausing = false;
		m_Rigidbody.WakeUp();
		m_Rigidbody.velocity = new Vector3(Random.Range(64, 96) * GetRandamSign(),
										   Random.Range(64, 96) * GetRandamSign());
	}

	public void TemporaryStop(int frame)
	{
		Vector3 velocity = m_Rigidbody.velocity; 
		m_Rigidbody.Sleep();
		StartCoroutine(OnResume(velocity, frame));
	}

	private IEnumerator OnResume(Vector3 velocity, float frame)
	{
		for (int i = 0; i < frame; i++)
		{
			yield return null;
		}
		if(!m_IsPausing){
			m_Rigidbody.WakeUp();
            m_Rigidbody.velocity = velocity;
		}
	}
    
	public void Pause()
	{
		m_IsPausing = true;
		m_Rigidbody.velocity = Vector2.zero;
		m_Rigidbody.Sleep();
	}
}
