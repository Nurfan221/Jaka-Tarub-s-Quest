using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpesialSkillWeapon : MonoBehaviour
{
    [SerializeField] Player_Action player_Action;
    public Coroutine currentUICooldownCoroutine;
    [SerializeField] PlayerUI playerUI;

    [System.Serializable]
    public class CooldownInfo
    {
        public Item weapon;
        public bool isOnCooldown = false;
    }

    public List<CooldownInfo> infoWeapon = new List<CooldownInfo>();
    private Dictionary<Item, float> cooldownStartTime = new Dictionary<Item, float>();


    public void UseWeaponSkill(Item weapon, bool useSkill)
    {
        // Cari apakah weapon sudah ada dalam list
        CooldownInfo existingInfo = infoWeapon.Find(x => x.weapon.itemName == weapon.itemName);

        if (existingInfo == null)
        {
            // Tambahkan jika belum ada
            existingInfo = new CooldownInfo { weapon = weapon, isOnCooldown = false };
            infoWeapon.Add(existingInfo);
        }

        // Jika skill digunakan
        if (useSkill)
        {
            if (!existingInfo.isOnCooldown)
            {
                existingInfo.isOnCooldown = true;
                player_Action.canSpecialAttack = false;
                // Jalankan cooldown UI
                StartCooldownUI(playerUI.specialAttackUI, weapon.SpecialAttackCD);

                // Mulai coroutine untuk reset cooldown
                StartCoroutine(ResetCooldown(existingInfo, weapon.SpecialAttackCD));
                Debug.Log("menjalankan cooldown bro sabar sikit ");
            }
            else
            {
                Debug.Log("Skill masih cooldown");
            }
        }
        else
        {
            // Kalau hanya ganti weapon
            if (existingInfo.isOnCooldown)
            {
                StartCooldownUI(playerUI.specialAttackUI, GetRemainingCooldown(weapon));
            }
            else
            {

                Debug.Log("mengganti weapon dan tidak ada cooldown");
                // STOP coroutine lama agar tidak override fillAmount
                if (currentUICooldownCoroutine != null)
                {
                    StopCoroutine(currentUICooldownCoroutine);
                    currentUICooldownCoroutine = null;
                    player_Action.canSpecialAttack = true;
                }
                playerUI.specialAttackUI.fillAmount = 1;
            }
        }

        Debug.Log("Menggunakan / mengganti weapon: " + weapon.itemName);
    }



    public void StartCooldownUI(Image image, float duration)
    {
        // Hentikan coroutine lama jika ada
        if (currentUICooldownCoroutine != null)
        {
            StopCoroutine(currentUICooldownCoroutine);
        }

        // Mulai cooldown baru
        currentUICooldownCoroutine = StartCoroutine(HandleUICD(image, duration));
    }


    IEnumerator HandleUICD(Image image, float cd)
    {
        float startTime = Time.time;
        while (Time.time < startTime + cd)
        {
            image.fillAmount = (Time.time - startTime) / cd;
            yield return null;
        }
        image.fillAmount = 1;
    }

    private IEnumerator ResetCooldown(CooldownInfo info, float duration)
    {
        cooldownStartTime[info.weapon] = Time.time;
        yield return new WaitForSeconds(duration);
        info.isOnCooldown = false;
        player_Action.canSpecialAttack = true;
    }

    private float GetRemainingCooldown(Item weapon)
    {
        if (!cooldownStartTime.ContainsKey(weapon)) return 0f;
        float elapsed = Time.time - cooldownStartTime[weapon];
        return Mathf.Max(0f, weapon.SpecialAttackCD - elapsed);
    }


}
