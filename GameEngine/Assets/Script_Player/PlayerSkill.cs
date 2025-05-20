using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerSkill : MonoBehaviour
{
    [SerializeField] GameObject DaggerPrefeb;
    [SerializeField] float SkillColldown1;
    [SerializeField] float SkillColldown2;
    [SerializeField] float BulletSpeed;
    public void Bandit_short(InputAction.CallbackContext obj)
    {
         
    }
    public void Throw(InputAction.CallbackContext obj)
    {
        GameObject Dagger = Instantiate(DaggerPrefeb);
        Dagger.transform.position = this.transform.position;
        Vector2 shotVec;
        if (this.transform.localPosition.x == 0.25)
            shotVec = Vector2.right * BulletSpeed;
        else
            shotVec = Vector2.left * BulletSpeed;
        Dagger.GetComponent<Rigidbody2D>().AddForce(shotVec);
    }
}
