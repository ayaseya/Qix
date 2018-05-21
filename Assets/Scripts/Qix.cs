using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Qix : MonoBehaviour
{
	private Vector2 m_LastVelocity;
	private Rigidbody2D m_Rigidbody;

	public UnityEvent OnQixTouched;

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
		if(collision.gameObject.CompareTag("Line")){
			OnQixTouched.Invoke();

		}
		if (collision.gameObject.CompareTag("Marker"))
        {
			OnQixTouched.Invoke();
        }


		Vector2 refrectVec = Vector2.Reflect(m_LastVelocity, collision.contacts[0].normal);
		m_Rigidbody.velocity = refrectVec;
	}

	private int GetRandamSign()
	{
		return Random.Range(0, 1) == 0 ? -1 : 1;
	}

	public void AddInitialForce(){
		m_Rigidbody.velocity = new Vector2(Random.Range(64, 96) * GetRandamSign(),
                                           Random.Range(64, 96) * GetRandamSign());
	}
    
	public void OnPause()
	{
		Vector2 velocity = m_Rigidbody.velocity; ;
		m_Rigidbody.Sleep();
		StartCoroutine(OnResume(velocity));
	}

	private IEnumerator OnResume(Vector2 velocity)
	{
		for (int i = 0; i < 15; i++)
		{
			yield return null;
		}

		m_Rigidbody.WakeUp();
		m_Rigidbody.velocity = velocity;
	}

	public void OnStart()
    {
		m_Rigidbody.WakeUp();
    }

	public void OnStop(){
		m_Rigidbody.Sleep();
	}
}
