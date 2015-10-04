using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PickUpScript : MonoBehaviour {


	public Text countText;
	public Text winText;
	private int count;
	
	void Start()
	{
		count = 0;
		SetCountText ();
		winText.text = " ";
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag ("PickUp")) 
		{
			other.gameObject.SetActive (false);
			count+=1;
			SetCountText ();
		}
	}
	
	void SetCountText()
	{
		countText.text = "Count: " + count.ToString ();
		if (count >= 12) {
			winText.text = "You win!!!";
		}
	}
}
