using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider hpBar;
    [SerializeField] private Image skill1CooldownImage;
    [SerializeField] private Image skill2CooldownImage;

    public void SetHP(float current, float max)
    {
        hpBar.maxValue = max;
        hpBar.value = current;
    }

    public void SetSkillCooldown(int skillIndex, float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        switch (skillIndex)
        {
            case 1:
                skill1CooldownImage.fillAmount = ratio;
                break;
            case 2:
                skill2CooldownImage.fillAmount = ratio;
                break;
        }
    }
}
