using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthManager : MonoBehaviour {
	private PlayerManager _playerManager;
	private Animator _animator;
	public float HealthPoints;
	private float _hp;
	public Image HealthBar;

	private int _healthPacks;
	public int InitialHealthPacks;
	
	public AudioSource UseHealthPackSound;
	public AudioSource NoHealthPackSound;
	public AudioSource DeathSound;
	public TextMeshProUGUI HealthPacksUI;
	private HealthManager _hManager;
	private bool dead;

	// Use this for initialization
	void Start () {
		_hp = HealthPoints;
		_animator = GetComponent<Animator>();
		_playerManager = GetComponent<PlayerManager>();
		_healthPacks = InitialHealthPacks;
		_hManager = GetComponent<HealthManager>();
	}
	
	// Update is called once per frame
	void Update () {
		HealthBar.fillAmount = _hManager._hp / HealthPoints;
	}
}
