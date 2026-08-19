using UnityEngine;
public class ExplosionDeactivator : MonoBehaviour
{
    void OnEnable() 
    { 
        CancelInvoke("Deactivate");
        Invoke("Deactivate", 2f);       
    }
    void Deactivate() { gameObject.SetActive(false); }
}
