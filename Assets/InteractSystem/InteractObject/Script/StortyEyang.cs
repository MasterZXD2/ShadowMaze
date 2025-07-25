using UnityEngine;
using System.Collections;
public class StortyEyang : MonoBehaviour
{
    public GameObject Text;
    public AudioSource Story;
    public bool Godear;
    void OnTriggerEnter(Collider other){
        
        if(other.gameObject.CompareTag("Player")){
            StartCoroutine(TextStory());
        }
    }
    IEnumerator TextStory(){
        Text.SetActive(true);
        Story.Play();
        Godear=true;
        yield return new WaitForSeconds(6.0f);
        Text.SetActive(false);
        Destroy(gameObject);
    }
}
