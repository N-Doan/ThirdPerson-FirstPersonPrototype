using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class LeaperAnimController : MonoBehaviour
{
    [SerializeField]
    private List<Animator> animators;
    // Start is called before the first frame update

    void Start()
    {

    }

    public void activation()
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("Activated", true);
        }

    }

    public void jumping()
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("IsJumping", true);
        }

    }

    public void landing()
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("IsJumping", false);
        }

    }
}
