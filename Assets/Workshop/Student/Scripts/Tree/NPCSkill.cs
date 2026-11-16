using Solution;
using UnityEngine;

public class NPCSkill : Identity
{
    public GameObject skillUi;
    public bool canTalk = true;

    public override bool Hit()
    {
        // ตรวจสอบว่าผู้เล่นมีไอเท็มที่ต้องการหรือไม่
        if (canTalk)
        {
            Debug.Log("NPCSkill");

            skillUi.SetActive(true);
            return false;
        }
        else
        {
            Debug.Log("I not neet to talk to you");
            return false;
        }
    }
}
