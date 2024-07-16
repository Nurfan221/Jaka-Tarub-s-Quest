using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventSystem : MonoBehaviour
{
    public static GameEventSystem Instance;

    [SerializeField] GameObject playerNameInputUI;
    [SerializeField] Dialogues afterFirstBandit;
    [SerializeField] Dialogues afterFinishDialogue;

    public bool DoneFirstNarration;
    public bool DoneDialogue_TamashiiGiveName;
    public bool DoneDialogue_DanauPertamaKeDesa;
    public bool DoneDialogue_FirstDesaWarga;
    public bool DoneDialogue_FirstKakRen;
    public bool DoneDialogue_FirstBandit;
    public bool DoneDialogue_FirstBanditDone;
    public bool DoneDialogue_FinshDialogue;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void DoAfterDialogue(string prompt)
    {
        if (prompt == "playerName")
        {
            playerNameInputUI.SetActive(true);
        }
        else if (prompt == "firstNarration")
        {
            DoneFirstNarration = true;

            Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem("Ranting Pohon"));
            Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem("Batu", 5));
            Player_Inventory.Instance.EquipItem(Player_Inventory.Instance.FindItemInInventory("Ranting Pohon"), 0);
            Player_Inventory.Instance.EquipItem(Player_Inventory.Instance.FindItemInInventory("Batu"), 1);

            Player_Direction.Instance.Target = ForestController.Instance.FirstSpawner.transform;
            Player_Quest.Instance.SetQuest("Bunuh semua serigala");
            TamashiiFollow.Instance.SetFollowing(true);
        }
        else if (prompt == "1Dialogue")
        {
            DoneDialogue_TamashiiGiveName = true;
            Player_Direction.Instance.Target = ForestController.Instance.DanauKetenangan;
            Player_Quest.Instance.SetQuest("Pergi ke danau ketenangan");
        }
        else if (prompt == "2Dialogue")
        {
            DoneDialogue_DanauPertamaKeDesa = true;
            Player_Direction.Instance.Target = ForestController.Instance.VillagePortal;
            Player_Quest.Instance.SetQuest("Kembali ke desa");
        }
        else if (prompt == "3Dialogue")
        {
            DoneDialogue_FirstDesaWarga = true;
            Player_Direction.Instance.Target = VillageController.Instance.KakRenTransform;
            Player_Quest.Instance.SetQuest("Bertemu Kak Ren");
        }
        else if (prompt == "4Dialogue")
        {
            DoneDialogue_FirstKakRen = true;
            Player_Direction.Instance.Target = VillageController.Instance.TanamanCabaiTransform;

            Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem("Penyiram Tanaman"));
            ItemDisplay.Instance.ShowItemDisplay(ItemPool.Instance.GetItem("Penyiram Tanaman"));
            ItemDisplay.Instance.AddButtonListener(null);

            Player_Quest.Instance.SetQuest("" +
                "- Gunakan penyiram tanaman di Inventory\n" +
                "- Berkebun bersama Tamashii");
        }
        else if (prompt == "givePedang")
        {
            Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem("Pedang Ren"));
            ItemDisplay.Instance.ShowItemDisplay(ItemPool.Instance.GetItem("Pedang Ren"));
            ItemDisplay.Instance.AddButtonListener(ItemDisplay.Instance.DialoguePedangPart2);
        }
        else if (prompt == "5Dialogue")
        {
            DoneDialogue_FirstBandit = true;
            RaidSystem.Instance.StartRaid(1, 500, () => DialogueSystem.Instance.StartDialogue(afterFirstBandit));

            Player_Direction.Instance.Target = null;
            Player_Quest.Instance.SetQuest("" +
                "- Gunakan pedang Kak Ren di Inventory\n" +
                "- Bunuh semua bandit");
        }
        else if (prompt == "6Dialogue")
        {
            DoneDialogue_FirstBanditDone = true;
            Player_Direction.Instance.Target = null;
            Player_Quest.Instance.SetQuest("");
        }
        else if (prompt == "FinishDialogue")
        {
            DoneDialogue_FinshDialogue = true;
            RaidSystem.Instance.StartRaid(true, () => DialogueSystem.Instance.StartDialogue(afterFinishDialogue));
        }
        else
        {
            print("no valid prompt");
        }
    }
}
