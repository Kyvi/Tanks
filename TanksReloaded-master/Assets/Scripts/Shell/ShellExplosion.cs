using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
	public LayerMask m_TankMask; //Comme expliqué au début, les Tanks sont placés dans un calque dédié (Players). L'objectif étant de ne pas affecter que
	//les Tanks lorsque ces derniers sont touchés par un obus, il est necessaire de créer un LayerMask 
    public ParticleSystem m_ExplosionParticles; //réference au système de particules lorsque l'obus va exploser       
    public AudioSource m_ExplosionAudio; //réference à l'audio qui va être joué               
    public float m_MaxDamage = 100f; //le maximum de dégât qu'un obus peut causer                  
    public float m_ExplosionForce = 1000f; //La force de l'explosion. Si l'obus est trop proche du Tank ce dernier va s'éjecter encore plus que s'il est plus loin           
    public float m_MaxLifeTime = 2f; //la durée après laquelle, l'obus va être supprimé du jeu                   
    public float m_ExplosionRadius = 5f; //Le rayon dans lequel un Tank peut être affecté.             


    private void Start()
    {
        Destroy(gameObject, m_MaxLifeTime); //Détruit cet objet après deux secondes. 
    }


    private void OnTriggerEnter(Collider other) //Vérifier si l'obus est entré en collision avec n'importe quel autre objet
    {
        // Find all the tanks in an area around the shell and damage them.
		Collider[] colliders = Physics.OverlapSphere(transform.position,m_ExplosionRadius, m_TankMask); //on va créer une sorte de sphère virtuelle qui va enregistrer toutes les données 
			//de tous les objets qui vont entrer en collision avec elle.
		for (int i = 0; i < colliders.Length; i++) {
			Rigidbody targetRigidBody = colliders [i].GetComponent<Rigidbody> (); //On va vérifier si l'objet qu'on a touché contient un rigidbody 
			if (!targetRigidBody)
				continue; //S'il n y à pas on passe au suivant
			targetRigidBody.AddExplosionForce(m_ExplosionForce,transform.position,m_ExplosionRadius); //Cela va permettre de définir la force d'explosion
			//qui va être appliquée sur le Tank.
			TankHealth targetHealth = targetRigidBody.GetComponent<TankHealth>(); //puisque l'objet qui va être touché est un Tank on va chercher une instance
			//du script TankHealth qu'on a crée précédement. ET faire les modifications sur son attribut health.
			//Dans ce cas de figure rigidbody et gameObject réfèrent tous les deux au même composant.
			if (!targetHealth)
				continue; //Si ce composant ne contient pas targethealth script alors passer au suivant.
			float damage = CalculateDamage(targetRigidBody.position); //Pour calculer la quantité de dégâts que l'obus va infliger au Tank on va calculer
			//sa distance par rapport à ce dernier.
			targetHealth.TakeDamage(damage); //invoquer la fonction TakeDamage du Script TankHealth dans Tank
		}

		m_ExplosionParticles.transform.parent = null; //Pour s'assurer que les particules continuent à s'animer même si l'obus est detruit et supprimé en tant qu'objet.
    //Pour ce faire on enlève le lien de parenté entre les particules et l'obus
		m_ExplosionParticles.Play();
		m_ExplosionAudio.Play ();

		Destroy (m_ExplosionParticles.gameObject, m_ExplosionParticles.duration); //çà va peremttre de détruire les particules après la fin de leur animation.
		Destroy(gameObject); //détruire l'obus.
	}


    private float CalculateDamage(Vector3 targetPosition)
    {
        // Calculate the amount of damage a target should take based on it's position.
		Vector3 explosionToTarget = targetPosition - transform.position; //çà va permettre de calculer la distance entre l'obus et le(s) Tank(s)

		float explosionDistance = explosionToTarget.magnitude; //Retourne la longueur du vecteur distance = racine(X²+Y²+Z²). Valeur entre 0 et le Radius déjà défini 

		float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;//Calculer la valeur relative qui est comprise entre 0 et 1.

		float damage = relativeDistance * m_MaxDamage; //une valeur entre 0 et 1 qui va être multipliée par 100 çà va nous donner la quantité d'energie qu'on
		//va perdre.

		damage = Mathf.Max (0f, damage); //Vérifier que le dégât ne peut pas être négatif. 

		return damage;
    }
}