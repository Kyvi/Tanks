using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float m_DampTime = 0.2f;  // Le temps approximatif durant lequel, la caméra va se déplacer vers la position cible. Au lieu qu'elle se déplace
	//instantanément, cette dernière va prendre un léger retard pour donner l'impression de suivre le tank 
    public float m_ScreenEdgeBuffer = 4f; // Une valeur qu'on va rajouter à la variable size pour s'assurer que le Tank est toujours à l'intérieur de l'écran           
    public float m_MinSize = 6.5f; // la valeur de rapprochement de la caméra au Tank. Il faut s'assurer qu'elle ne s'approche pas trop.                     
    [HideInInspector] public Transform[] m_Targets; //Un tableau de Transform qui contient tous les Transforms de tous les Tanks qui vont être rajotués
	//au jeu.


    private Camera m_Camera; // La réference de la caméra c'est à dire la maincamera                       
    private float m_ZoomSpeed; //Pour la fonction Damp et qui va renseigner sur la vitesse du zoom actuelle dans le Damp                      
	private Vector3 m_MoveVelocity; //Pour la fonction Damp et qui va renseigner sur la vitesse de déplacement actuelle dans le Damp                   
    private Vector3 m_DesiredPosition; //La position cible qui va être la moyenne de tous les Tanks. C'est à partir de cette position qu'on va appliquer              
	// les opération de zoom et déplacements.

    private void Awake()
    {
        m_Camera = GetComponentInChildren<Camera>(); //Mise en place de la réference caméra. InChildren cherhcher un composant à l'intérieur de la RigCamera
    }


    private void FixedUpdate()
    {
        Move(); //Les fonctions Move et Zoom sont appelés à l'intérieur de Fixed Update pour synchroniser avec le FixedUpdate du script du Tank
        Zoom();
    }


    private void Move()
    {
        FindAveragePosition(); // Trouver la position moyenne 

        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }


    private void FindAveragePosition()
    {
        Vector3 averagePos = new Vector3();
        int numTargets = 0;

        for (int i = 0; i < m_Targets.Length; i++)
        {
            if (!m_Targets[i].gameObject.activeSelf) //Vérifier si le GameObject est actif. Par exemple s'il est mort il est désactivé
                continue; //Passer à l'entrée suivante dans la boucle

            averagePos += m_Targets[i].position; 
            numTargets++;
        }

        if (numTargets > 0) 
            averagePos /= numTargets; 

        averagePos.y = transform.position.y; //On ne veut pas déplacer la caméra suivant y et on garde la valeur du transform CameraRig. Ce morceau est rajoué
		//par mesure de sécurité car on a déjà bloqué le déplacement des Tanks sur Y.

        m_DesiredPosition = averagePos;
    }


    private void Zoom()
    {
        float requiredSize = FindRequiredSize(); //Même logique pour le mouvement de la caméra
        m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }


    private float FindRequiredSize()
    {
        Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition); //On cherche la position désirée à partir de l'attribut m_DesiredPosition
		// et pas à partir de la position actuelle de la caméra. On cherche à transformer la position désirée dans l'espace local de la CameraRig

        float size = 0f; //La valeur de retour de cette fonction. a fur et à mesure qu'on boucle au travers des Tanks, on va cherche la plus grande valeur
		//de size et rajouter le m_ScreenEdgeBuffer pour pouvoir assurer le non dépassement des bords de la caméra.

        for (int i = 0; i < m_Targets.Length; i++)
        {
            if (!m_Targets[i].gameObject.activeSelf)
                continue; //même remarque.

            Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position); // projection de la position du Tank de l'espace worldspace vers
			//l'espace local de la cameraRig. 

            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos; //Calculer la distance entre la position du Tank et la position désirée dans l'espace
			//local du Tank.

            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.y)); //L'utilisation de la valeur absolue va permettre de localiser les Tanks qui sont 
			//dans le sens opposé de l'axe y de la CameraRig

            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.x) / m_Camera.aspect); //Pareil pour l'axe x. Notions théoriques expliquées dans le cours.
        }
        
        size += m_ScreenEdgeBuffer;

        size = Mathf.Max(size, m_MinSize); //S'assurer qu'on est pas entrain de trop zoomer et donc bloquer le zoom sur la valeur mentionnée au début.

        return size;
    }


    public void SetStartPositionAndSize() //C'est une fonction qui permet d'initialiser la position de la caméra. Elle va être appelée par le gameManager
    {
        FindAveragePosition();

        transform.position = m_DesiredPosition;

        m_Camera.orthographicSize = FindRequiredSize();
    }
}