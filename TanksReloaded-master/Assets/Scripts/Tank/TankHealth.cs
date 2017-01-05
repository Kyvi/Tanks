using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
	public int m_PlayerNumber = 1; //Le numéro du joueur pour activer les inputs correspondants       

    public float m_StartingHealth = 100f;   //vie de début    
	public float m_StartingShield = 100f; // bouclier de début
	public float m_StartingXPGoal = 100f; // expérience à avoir pour monter de niveau
	public int nblevel = 1; // niveau du tank
    public Slider m_Slider;     //référence au Slider                   
    public Image m_FillImage;  // Nous cherchons à modifier la couleur de l'image qui doit être renseignée dans cet attribut                    
	public Color m_FullHealthColor = Color.green; //Couleur lorsque l'énergie est égale à 100   
    public Color m_ZeroHealthColor = Color.red;  //Couleur lorsque l'énergie est égale à 0   
    public GameObject m_ExplosionPrefab; //référence au Prefab d'explosion
	

	public AudioSource m_BonusAudio; // réference à l'audio qui va être joué lors du bonus
	public AudioClip m_BonusClip;// un audio clip pour le bonus    
    
	private GameManager gameManager; // référence au gameManager
	private MeshRenderer[] renderers; // référence au renderers
    private AudioSource m_ExplosionAudio; //reference à l'audio source du TankExplosion          
    private ParticleSystem m_ExplosionParticles; //reference au système de particule   

	//**** On récupère les sliders à partir du GameManager **** //
	private Slider health;
	private Text textHealth;

	private Slider Shield;
	private Text textShield;

	private Slider Experience;
	private Text textExperience;

	private Text level;
	/******************/

    private float m_CurrentHealth; //reference à l'état de vie actuel 
	private float m_CurrentShield; // reference à l'état de bouclier actuel
	private float m_CurrentXP; // reference à l'état actuel d'expérience
	private float m_ShieldRegen = 2; // reference à la vitesse de régénération du bouclier
    private bool m_Dead;   // Tank est mort ou pas ?     
	private bool isInvincible; // Tank est invincible ou pas ?
	private float timer; // référence au temps de capture du bonus
	private float timerZone; // référence au temps de damage pris précédemment dans la zone
	private float BonusTime = 10f; // temps du bonus
	private bool hasChanged = false; // Interrupteur
	private Color tankColor; // référence à la couleur du tank
	private bool isInsideZone = false; // le tank est-il dans la zone ?
	private float DamageTime = 0.1f; // temps entre chaque dommage dans la zone

    private void Awake()
    {
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>(); //Instantier le systeme de particule du prefab
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>(); //réference au composant audio du système de particules.
        m_ExplosionParticles.gameObject.SetActive(false); //Désactiver le GameObject pour le moment


		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>(); // On récupère le script global
		renderers = gameObject.GetComponentsInChildren<MeshRenderer>(); // On récupère les rendus visuels

    }

	private void Start(){
		tankColor = renderers [0].material.color; // On récupère la couleur du tank

		InvokeRepeating ("gainXPAndShield", 5f, 1f); // On régénère le bouclier toute les secondes et on ajoute de l'expérience

	}
		

	private void Update (){


		if (m_CurrentXP >= m_StartingXPGoal) { // On check le level Up
			levelUp();
		}
			
		if (isInvincible && hasChanged) { // On change la couleur du tank s'il est invincible une seule fois
			colorInvincible();
		}

		if (isInvincible && timer + BonusTime < Time.time) { // L'invincibilité disparait après le temps du bonus
			colorNormal ();
		}

		if (isInsideZone && timerZone + DamageTime < Time.time) {
			TakeDamage (1f);
			timerZone = Time.time;
		}
	}

	private void levelUp(){
		
		nblevel++; // monte de niveau

		level.text = "Tank" + m_PlayerNumber + " : Niveau " + nblevel; // Actualise le texte

		m_CurrentXP = 0f; // l'expérience de base est remise à 0
		m_StartingXPGoal = m_StartingXPGoal + 80f; // l'expérience objectif augmente
		SetExperienceUI(); // Actualisation barre d'expérience

		m_StartingHealth = m_StartingHealth + 20f; // la vie initiale augmente 
		m_CurrentHealth = m_StartingHealth; // la vie du tank actuelle est mise à la vie initiale
		SetHealthUI (); // Actualisation barre de vie

		m_StartingShield = m_StartingShield + 30f; // Le bouclier initial est plus puissant
		m_CurrentShield = m_StartingShield; // Le bouclier du tank est réinitialisée
		SetShieldUI (); // Actualisation barre de bouclier

		m_ShieldRegen = m_ShieldRegen + 1f; // le taux de régénération du tank augmente
	}

	private void colorInvincible(){
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers [i].material.color = new Color (255, 171, 22);
		}
		hasChanged = false;
	}

	private void colorNormal(){
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers [i].material.color = tankColor;
		}
		isInvincible = false;
	}


    private void OnEnable() //Lorsque le Tank est activé
    {
		Setup ();
        m_CurrentHealth = m_StartingHealth; //Initialiser le niveau de vie 
		SetHealthUI(); 

		m_CurrentShield = m_StartingShield; // Initialiser le niveau de bouclier
		SetShieldUI();

		m_CurrentXP = 0f; // Initialiser l'expérience
		SetExperienceUI ();

        m_Dead = false; //Le Tank n'est pas mort
    }
    

    public void TakeDamage(float amount) //L'argument amount peut être variable. Par exemple si l'obus est trop prêt alors le dégat est plus conséquent que
	//si l'obus est plus loin.
    {
		
		if (!isInvincible) { // On prend des dommages que si le vaisseau est invincible
			// Adjust the tank's current health, update the UI based on the new health and check whether or not the tank is dead.
			if (amount <= m_CurrentShield) {
				m_CurrentShield -= amount; // On met à jour le niveau de bouclier du tank
				SetShieldUI (); // On met à jour le dessin du Slider de bouclier
			} else {
				float trueAmount = amount - m_CurrentShield;
				m_CurrentShield = 0; // Le bouclier est cassé
				SetShieldUI (); // On met à jour le dessin du Slider de bouclier
				m_CurrentHealth -= trueAmount; //On met à jour la vie du Tank mais on est pas entrain de déssiner
				SetHealthUI (); //On met à jour le dessin du Slider de vie
			}
			if (m_CurrentHealth <= 0f && !m_Dead) { //Si la vie est inférieure ou égale à 0 et on n'est pas déjà mort
				OnDeath ();
			}
		}
    }


    private void SetHealthUI()
    {
        // Adjust the value and colour of the slider.
		m_Slider.value = (m_CurrentHealth / m_StartingHealth) * 100;
		m_FillImage.color = Color.Lerp (m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth); //le troisième argument est une sorte
		//de pourcentage qui reflète le pourcentage de niveau d'enérgie qu'on possède.

		health.value = (m_CurrentHealth / m_StartingHealth) * 100;
		textHealth.text = "Health : " + (int) m_CurrentHealth + " / " + m_StartingHealth;

    }

	private void SetShieldUI()
	{
		Shield.value = (m_CurrentShield / m_StartingShield) * 100;
		textShield.text = "Shield : " + (int) m_CurrentShield + " / " + m_StartingShield;
	}

	private void SetExperienceUI()
	{
		Experience.value = (m_CurrentXP / m_StartingXPGoal) * 100;
		textExperience.text = "Experience : " + m_CurrentXP + " / " + m_StartingXPGoal;
	}


    private void OnDeath()
    {
        // Play the effects for the death of the tank and deactivate it.
		m_Dead = true;
		m_ExplosionParticles.transform.position = transform.position; //Placer le GameObject des particles à la position du Tank
		m_ExplosionParticles.gameObject.SetActive (true);//Activer le gameObject parce qu'il a été désactivé au début

		m_ExplosionParticles.Play (); //jouer le mouvement des particules
		m_ExplosionAudio.Play (); //jouer le son de l'explosion

		gameObject.SetActive(false); //désactiver le Tank.
    }

	private void OnTriggerEnter(Collider other){ 


		if (other.tag == "HealingBonus") { // Vérifier si le tank est rentré en collision avec un Bonus
			m_CurrentXP += 50f;
			SetExperienceUI ();
			m_CurrentHealth = Mathf.Min (m_StartingHealth, m_CurrentHealth + 20); // Regagner des points de vie
			m_BonusAudio.clip = m_BonusClip; //jouer le son du bonus
			m_BonusAudio.Play ();
			SetHealthUI ();
			gameManager.addBonusHealthSpot (other.gameObject.transform.position); // On rajoute la position du spot en disponible
			Destroy (other.gameObject); // On détruit le bonus
		}

		if (other.tag == "InvincibleBonus") { // Vérifier si le tank est rentré en collision avec un Bonus
			m_CurrentXP += 50f;
			SetExperienceUI ();
			isInvincible = true; // On le met invincible
			hasChanged = true; // On met en place l'interrupteur pour que l'update ne fasse qu'une seule fois le changement
			timer = Time.time; // On récupère le temps de capture du bonus
			m_BonusAudio.clip = m_BonusClip; //jouer le son du bonus
			m_BonusAudio.Play ();
			gameManager.addBonusHealthSpot (other.gameObject.transform.position); // On rajoute la position du spot en disponible
			Destroy (other.gameObject); // On détruit le bonus
		}

		if (other.tag == "BadZone") { // Vérifier si le tank est rentré en collision avec la zone
			isInsideZone = true;
			timerZone = Time.time;
		}
	}

	private void OnTriggerExit(Collider other){
		if (other.tag == "BadZone") { // Vérifier si le tank est sorti de collision avec la zone
			isInsideZone = false;
		}
	}

	public void setisInsideFalse(){
		isInsideZone = false;
	}

	public void gainXPAndShield(){
		m_CurrentXP += 2f;
		SetExperienceUI ();

		m_CurrentShield = Mathf.Min(m_StartingShield, m_CurrentShield+m_ShieldRegen);
		SetShieldUI ();
	}

	private void Setup(){
		switch (m_PlayerNumber) {
		case 1:
			health = gameManager.health1;
			textHealth = gameManager.textHealth1;

			Shield = gameManager.Shield1;
			textShield = gameManager.textShield1;

			Experience = gameManager.Experience1;
			textExperience = gameManager.textExperience1;

			level = gameManager.level1;
			break;
		case 2:
			health = gameManager.health2;
			textHealth = gameManager.textHealth2;

			Shield = gameManager.Shield2;
			textShield = gameManager.textShield2;

			Experience = gameManager.Experience2;
			textExperience = gameManager.textExperience2;

			level = gameManager.level2;
			break;
		}
	}
}