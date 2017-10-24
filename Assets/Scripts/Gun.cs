using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum GunFireMode
{
	Safe,
	Single,
	Auto,
	Burst,
	Load
}

public class Gun : MonoBehaviour {
	//Publicos:


	public GameObject Projectile;
	public float ProjectileSpeed = 10.0f;
	public Camera Cam;
	public bool LookAtMouse;
	public GunFireMode FireMode;
	public int BurstSize;
	public int Magazine;
	public int Ammo;
	public bool BulletCam;


	public int CurrentMagSize;
	//Privado:
	private SpriteRenderer GunSprite;
	private Vector3 MouseWorld;
	private float HandRotation;
	private Player PlayerSc;
	private Transform BSpawn;
	private float LoadState;

	void Awake(){
		GunSprite = GetComponent<SpriteRenderer> ();
		BSpawn = GetComponentInChildren<Transform> ();
	}

	void Start()
	{
		PlayerSc = GetComponentInParent<Player> ();
	}

	void Update () 
	{
		//LookAtMouse:
		if (LookAtMouse) {
			MouseWorld = Cam.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Cam.nearClipPlane));
			HandRotation = Mathf.Atan2 (MouseWorld.y - GunSprite.transform.position.y, MouseWorld.x - GunSprite.transform.position.x);
			HandRotation = HandRotation * (180.0f / Mathf.PI);
			GunSprite.transform.localRotation = Quaternion.Euler (0, 0, HandRotation);
		}
			
		Fire ();

		if (Input.GetKeyDown (KeyCode.R))
			Reload (true);

		if (Input.GetKeyDown(KeyCode.Q))
			SwitchFireMode ();
			
		if (PlayerSc.Left) {
			GunSprite.flipX = false;
			GunSprite.flipY = true;
			gameObject.transform.localPosition = new Vector3 (-0.045f, -0.095f, 0f);
		} else {
			GunSprite.flipX = false;
			GunSprite.flipY = false;
			gameObject.transform.localPosition = new Vector3 (0.045f, -0.095f, 0f);

		}
	}

	public void Fire ()
	{
		if (CurrentMagSize > 0) 
		{
			switch (FireMode) 
			{
			case GunFireMode.Safe:
				break;

			case GunFireMode.Single:
				if (Input.GetMouseButtonDown (0))
					CreateBullet (ProjectileSpeed);
				break;

			case GunFireMode.Auto:
				if (Input.GetMouseButton (0))
					CreateBullet (ProjectileSpeed);
				break;

			case GunFireMode.Burst:
				if (Input.GetMouseButtonDown (0))
					for (int i = 0; i < BurstSize; i++)
						CreateBullet (ProjectileSpeed);
				break;
		
			case GunFireMode.Load:
				if (Input.GetMouseButton (0))
					LoadState += (Time.deltaTime * 1);

				if (Input.GetMouseButtonUp (0)) {
					CreateBullet (LoadState*ProjectileSpeed);
					LoadState = 0;
				}
				break;
			}
		}
	}
		
	public void Reload(bool chamber)
	{
		if (Ammo < 0)
			return;

		Ammo -= (Magazine-CurrentMagSize);
		CurrentMagSize = Magazine;
		
	}

	void CreateBullet (float speed)
	{
		var bullet = Instantiate (Projectile,BSpawn.transform.position,transform.rotation);
		Projectile bulletSc = bullet.GetComponent<Projectile> ();

		bulletSc.Direction = (Vector2)(Quaternion.Euler(0,0,HandRotation)*Vector2.right);
		bulletSc.Speed = speed;
		bulletSc.name = "7.62x45mm";
		bulletSc.Launch ();
		CurrentMagSize -= 1;
	}

	void SwitchFireMode()
	{
		if (FireMode > GunFireMode.Load)
			FireMode = GunFireMode.Safe;
		else
			FireMode++;
	}
		
}
