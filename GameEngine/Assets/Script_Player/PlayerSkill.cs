using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerSkill : MonoBehaviour
{
    [SerializeField] GameObject DaggerPrefeb;
    float SkillTimer1;
    float SkillTimer2;
    [SerializeField] float SkillColldown1;
    [SerializeField] float SkillColldown2;
    [SerializeField] float BulletSpeed;
    [SerializeField] UIManager UI;

    [SerializeField] bool Penetration;

    [SerializeField] Transform Attackpos;
    Transform DaggerTrans;

    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        SkillTimer1 += Time.deltaTime;
        SkillTimer2 += Time.deltaTime;
        if (SkillTimer2 < SkillColldown2)
        {
            UI.SetSkillCooldown(2, (SkillTimer2 / SkillColldown2));
        }
    }
    public void Bandit_short(InputAction.CallbackContext obj)
    {
         
    }
    public void Throw(InputAction.CallbackContext obj)
    {
        if (SkillTimer2 < SkillColldown2)
        {
            return;
        }
        GameObject TDagger = Instantiate(DaggerPrefeb);
        TDagger.transform.position = new Vector2(Attackpos.position.x, Attackpos.position.y - 0.25f);

        Vector2 shotVec;
        if (GameObject.FindWithTag("Player").GetComponent<Player>().Fliper())
        {
            shotVec = Vector2.right * BulletSpeed;
            TDagger.GetComponent<Transform>().Rotate(new Vector3(0, 0, 0));
        }
        else
        {
            shotVec = Vector2.left * BulletSpeed;
            TDagger.GetComponent<Transform>().Rotate(new Vector3(0, 0, 180));
        }
        TDagger.GetComponent<Rigidbody2D>().linearVelocityX = shotVec.x * BulletSpeed;
        TDagger.GetComponent<Dagger>().setPen(Penetration);
        SkillTimer2 = 0;
        UI.SetSkillCooldown(2, 0);
        StopCoroutine("AttackCancel");
        StartCoroutine("AttackCancel");
    }
    IEnumerator AttackCancel()
    {
        animator.SetBool("Skill2", true);
        yield return new WaitForSeconds(0.15f);
        
        animator.SetBool("Skill2", false);
        yield break;
    }
}