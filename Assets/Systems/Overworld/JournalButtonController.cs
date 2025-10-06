using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Animator))]
public class JournalButtonController : MonoBehaviour
{
    Animator anim;
    bool isOpen;

    void Awake()
    {
        anim = GetComponent<Animator>();
        GetComponent<Button>().onClick.AddListener(Toggle);
        anim.SetBool("Open", false); // start closed
    }

    void Toggle()
    {
        Debug.Log("journal");
        isOpen = !isOpen;
        anim.SetBool("Open", isOpen);
    }
}
