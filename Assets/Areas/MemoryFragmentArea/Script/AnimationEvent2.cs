using UnityEngine;
using UnityEngine.InputSystem;  

public class AnimationEvent2 : MonoBehaviour
{
    public GameObject gamekak, gamekak1, gamekak2;

    public void forevent2()
    {
        gamekak.SetActive(false);
        gamekak1.SetActive(false);
        gamekak2.SetActive(false);
    }
}
