using UnityEngine;

public class TankMovement : MonoBehaviour
{
	//m_ pour désigner un attribut de la classe sinon c'est une variable de la fonction  
   //public va permettre d'afficher les attributs qui peuvent être manipulés par l'utilisateur pour personnaliser et l'adapter au jeu 
  //private est utilisé pour les attributs qui vont juste servir aux différentes méthodes pour être invoquées

    public int m_PlayerNumber = 1; //Le numéro du joueur pour activer les inputs correspondants       
    public float m_Speed = 12f;  //  La vitesse de déplacement du Tank         
    public float m_TurnSpeed = 180f; // La vitesse de rotation du Tank      
    public AudioSource m_MovementAudio;  //L'Audio source qui a été rajouté pour le Tank en mode repos
    public AudioClip m_EngineIdling; // Les clips qui vont être joués suivant les contrôles du jeu      
    public AudioClip m_EngineDriving;      
    public float m_PitchRange = 0.2f;//Variation du pitch de l'audio original

    
    private string m_MovementAxisName; //Il faut le paramétrer dans l'onglet InputManager. Il doit porter le nom de l'axe qui va être utilisé    
    private string m_TurnAxisName; // Pareil que le précédent        
    private Rigidbody m_Rigidbody; // Paramétrer le RigidBody pour les déplacements et les rotations du Tank        
    private float m_MovementInputValue; // Les valeurs des différents axes de déplacement   
    private float m_TurnInputValue;  //Les valeurs des différents axes de rotation      
    private float m_OriginalPitch; // Valeur du Pitch original. Permet de varier le pitch à partir de cette valeur initiale.        


    private void Awake() //Cette fonction est appelée lorsque le jeu démarre. Indépendement si Le script a été lancé ou pas 
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    private void OnEnable () //Cette fonction est appelée lorsque le script est activé.
    {
        m_Rigidbody.isKinematic = false; //isKinmatic renseigne si oui ou non une force est appliquée sur le Tank
        m_MovementInputValue = 0f; //RAZ des vitesses
        m_TurnInputValue = 0f;
    }


    private void OnDisable () //Cette fonction est appelée lorsque le script est désactivé.
    {
        m_Rigidbody.isKinematic = true;
    }


    private void Start()
    {
        m_MovementAxisName = "Vertical" + m_PlayerNumber; //Mise en place des axes de déplacement et de rotation
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        m_OriginalPitch = m_MovementAudio.pitch; //Sauvegader le pitch intial du Tank
    }
   

    private void Update()
    {
        // Store the player's input and make sure the audio for the engine is playing.
		m_MovementInputValue = Input.GetAxis(m_MovementAxisName); //Lier entre les inputs paramétérés dans le menu et leurs valeurs
		m_TurnInputValue = Input.GetAxis (m_TurnAxisName);

		EngineAudio ();

    }


    private void EngineAudio()
    {
        // Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.

		//Si le Tank est entrain de bouger on joue l'audio de mouvement sinon on joue le son du repos. 

		if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f) {
			//La valeur des m_Turn ou m_Movements seront entre -1 et 1 suivant l'axe (horizontal ou vertical). Donc si la valeur est inférieure à 0.1f 
			//on considère que le Tank n'est pas entrain de bouger
			if(m_MovementAudio.clip == m_EngineDriving) //Vérifier l'Audio source courant et son clip. Si c'est le même on le garde sinon on le change
			{
				m_MovementAudio.clip = m_EngineIdling;
				m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);// çà va permettre de rejouer le son
				//avec un pitch différent pour le varier.
				m_MovementAudio.Play(); //rejouer le même morceau
			}
		} else {
			// refaire la même chose avec le son de mouvement 
			if(m_MovementAudio.clip == m_EngineIdling) //Vérifier l'Audio source courant et son clip. Si c'est le même on le garde sinon on le change
			{
				m_MovementAudio.clip = m_EngineDriving;
				m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);// çà va permettre de rejouer le son
				//avec un pitch différent pour le varier.
				m_MovementAudio.Play(); //rejouer le même morceau
			}
		}
	

    }


    private void FixedUpdate() //Pareil que Update mais c'est lorsque la partie physique a été rendue avec succès.
    {
        // Move and turn the tank.
		Move();
		Turn ();
    }


    private void Move()
    {
        // Adjust the position of the tank based on the player's input.
		//Calculer le vector mouvement 
		Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime; //Créer un Vector dans la direction avant du Tank multiplié
		//par la valeur du mouvement introduite par l'utilisateur (-1 et 1) multipliée par la vitesse de mouvement multiplié par le temps (pour la rendre
		//proportionnelle à une fraction de seconde)

		m_Rigidbody.MovePosition (m_Rigidbody.position + movement); //modifier la valeur du RigidBody suivant le vecteur mouvement
    }


    private void Turn()
    {
        // Adjust the rotation of the tank based on the player's input.
		float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;//combien de degrés on va tourner. 
		Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);//La rotation dans Unity est assurée au travers des quaternions et pas au travers des Vector3 comme pour les déplacements.
		m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
	}


}