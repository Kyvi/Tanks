using UnityEngine;

public class UIDirectionControl : MonoBehaviour
{
    public bool m_UseRelativeRotation = true;  //l'objectif de ce script est de permettre healthSlider de garder la même rotation que celle du Canvas qui 
	//le possède. Par conséquent même si le Tank est en rotation la barre d'énèrgie ne va pas tourner.


    private Quaternion m_RelativeRotation;     


    private void Start()
    {
        m_RelativeRotation = transform.parent.localRotation;
    }


    private void Update()
    {
        if (m_UseRelativeRotation)
            transform.rotation = m_RelativeRotation;
    }
}
