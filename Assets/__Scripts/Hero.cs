using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour {

	static public Hero S;

	public float gameRestartDelay = 2f;

	//these fields control the movement of the ship
	public float speed = 30;
	public float rollMult = -45;
	public float pitchMult = 30;

	//Ship status info
	[SerializeField]
	private float _shieldLevel = 1;
	public Weapon[] weapons;

	public bool _______________________;

	public Bounds bounds;
	public delegate void WeaponFireDelegate();
	public WeaponFireDelegate fireDelegate;

	void Awake(){
		S = this; // set the singleton
		bounds = Utils.CombineBoundsOfChildren(this.gameObject);
	}

	void Start(){
		ClearWeapons();
		weapons[0].SetType(WeaponType.blaster);
	}

	void Update () {
		//pull in information from the input class
		float xAxis = Input.GetAxis("Horizontal");
		float yAxis = Input.GetAxis("Vertical");

		//change transform.position based on the axes
		Vector3 pos = this.transform.position;

		pos.x += xAxis * speed * Time.deltaTime;
		pos.y += yAxis * speed * Time.deltaTime;
		this.transform.position = pos;

		bounds.center = this.transform.position;

		//keep the ship constrained to the screen bounds
		Vector3 off = Utils.ScreenBoundsCheck(bounds, BoundsTest.onScreen);
		if (off != Vector3.zero){
			pos -= off;
			transform.position = pos;
		}

		//Rotate the ship to make it feel more dynamic
		transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);

		if (Input.GetAxis("Jump") == 1 && fireDelegate != null){
			fireDelegate();
		}
	}

	//This variable holds a reference to the last triggering game object
	public GameObject lastTriggerGo = null;

	void OnTriggerEnter(Collider other){
		GameObject go = Utils.FindTaggedParent(other.gameObject);
		if (go != null){
			if (go == lastTriggerGo){
				return;
			}
			lastTriggerGo = go;
			if (go.tag == "Enemy"){
				shieldLevel--;
				Destroy(go);
			}
			else if (go.tag == "PowerUp"){
				AbsorbPowerUp(go);
			}
			else{
				print("Triggered: " + go.name);
			}
		}
		else{
			print("triggered: " + other.gameObject.name);
		}
	}

	public float shieldLevel {
		get {
			return _shieldLevel;
		}
		set {
			_shieldLevel = Mathf.Min(value, 4);
			if (value < 0){
				Destroy(this.gameObject);
				Main.S.DelayedRestart(gameRestartDelay);
			}
		}
	}

	public void AbsorbPowerUp(GameObject go){
		PowerUp pu = go.GetComponent<PowerUp>();
		switch (pu.type){
		case WeaponType.shield:
			shieldLevel++;
			break;
		default:
			if (pu.type == weapons[0].type){
				Weapon w = GetEmptyWeaponSlot();
				if (w != null){
					w.SetType(pu.type);
				}
			}
			else{
				ClearWeapons();
				weapons[0].SetType(pu.type);
			}
			break;
		}
		pu.AbsorbedBy(this.gameObject);
	}

	Weapon GetEmptyWeaponSlot(){
		for (int i = 0; i < weapons.Length; ++i){
			if (weapons[i].type == WeaponType.none){
				return weapons[i];
			}
		}
		return null;
	}

	void ClearWeapons(){
		foreach(Weapon w in weapons){
			w.SetType(WeaponType.none);
		}
	}
}