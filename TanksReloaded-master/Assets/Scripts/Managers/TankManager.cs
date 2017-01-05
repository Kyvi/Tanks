using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable] //Puisque cette classe n'hérite pas de MonoBehavior donc par défaut ses attributs ne sont pas visualisés dans l'inspecteur. L'option 
//Serializable permet de les afficher.
public class TankManager //Cette classe n'hérite pas de la classe MonoBehavior donc elle ne peut pas utiliser les fonctions standards comme Start, Update...
{
    public Color m_PlayerColor; //La couleur du joueur            
    public Transform m_SpawnPoint; //L'emplacement du Tank créée         
    [HideInInspector] public int m_PlayerNumber; //cet attribut va nous permettre de savoir quel Tank est entrain de bouger, tourner ou tirer              
    [HideInInspector] public string m_ColoredPlayerText; //Une façon qui ressemble à HTML pour pouvoir copier la couleur des Tanks dans les textes.
    [HideInInspector] public GameObject m_Instance; //reférence au Tank. Pour pouvoir activer ou désactiver des options.         
    [HideInInspector] public int m_Wins; //nombres de round gagnés. Pour savoir si le joueur a remporté la partie ou pas.           



	private TankHealth m_Health; // Pour pouvoir controller les bonus du tank correspondant
    private TankMovement m_Movement; //Pour pouvoir controller le mouvement du tank correspondant      
    private TankShooting m_Shooting; //Pour pouvoir controller la rotation du tank correspondant
	private GameObject m_CanvasGameObject; //Pour pouvoir controller le Canvas du Tank correspondant (Nom du joueur...)


    public void Setup()
    {
		m_Health = m_Instance.GetComponent<TankHealth> (); //récupération du script TankHealth de l'instance du Tank
        m_Movement = m_Instance.GetComponent<TankMovement>(); //récupération du script TankMovement de l'instance du Tank
        m_Shooting = m_Instance.GetComponent<TankShooting>();//récupération du script TankShooting de l'instance du Tank
        m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject; //Récupération du Canvas de l'énergie à l'intérieur des GameObject
		//fils du Tank
		m_Health.m_PlayerNumber = m_PlayerNumber; 
        m_Movement.m_PlayerNumber = m_PlayerNumber; //affecter la valeur m_PlayerNumber aux attributs m_PlayerNumber dans le scripts TankMovement et
        m_Shooting.m_PlayerNumber = m_PlayerNumber;//TankShooting. Cela va permettre de pouvoir récupérer Fire1 , Fire2... Horizontal1, Horizontal2...

        m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>"; 
		//Création d'un texte de type HTML avec un style CSS de la même couleur que le Tank en lui même. 

        MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>(); //Cela va permettre d'appliquer la couleur non seulement sur
		//l'objet parent mais aussi sur tous ses fils.

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = m_PlayerColor;
        }
    }


    public void DisableControl() //désactiver les scripts TankMovement et TankShooting ainsi que le Canvas.
    {
        m_Movement.enabled = false;
        m_Shooting.enabled = false;

        m_CanvasGameObject.SetActive(false);
    }


    public void EnableControl() //Activer les scripts TankMovement et TankShooting ainsi que le Canvas
    {
        m_Movement.enabled = true;
        m_Shooting.enabled = true;

        m_CanvasGameObject.SetActive(true);
    }


    public void Reset()
    {
        m_Instance.transform.position = m_SpawnPoint.position; //Repositionner le Tank à la position du Spawn correspondant
        m_Instance.transform.rotation = m_SpawnPoint.rotation; //Tourner le Tank dans la même direction que le Spawn correspondant

        m_Instance.SetActive(false); //Désactive et Réactive tous les Tanks.
        m_Instance.SetActive(true);
    }

	public TankHealth getTankHealth(){
		return m_Health;
	}
		
}
