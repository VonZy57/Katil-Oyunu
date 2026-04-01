using UnityEngine;

public class MotherCooking : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void BuildDialogTree()
    {
        DialogNode firstPart = DialogBuilder.CreateNode
        ("I sh*t on neighbor's bed, I sh*t on neighbor's bed, I,I thougt he loved me, I thought he loved me!",
        "Komşunun yatağına s*çtım, Komşunun yatağına s*çtım, Beni, beni seviyor sandım, Beni seviyor sandım!",
        "Anne");

        DialogNode secondPart = DialogBuilder.CreateNode
        ("I made beans in the pot, I made tzatziki to go with, I put pickles next to it, I left my son an orphan, I left my son an orphan!",
        "Ocağa fasulye attım attım, Yanına cacık yaptım, Yanına turşu koydum, Oğlumu yetim koydum, Oğlumu yetim koydum!",
        "Anne");

        DialogNode thirdPart = DialogBuilder.CreateNode
        ("I sh*t on my neighbor's bed, I put tzatziki next to it, My son is my darling, I won't let you get sl*tty woman!",
        "Komşunun yatağına sıçtım, yanına cacık koydum, Oğlum benim canım, Salmam sana sana k*şar kadın kadın",
        "Anne");

        DialogNode finalPart = DialogBuilder.CreateNode
        ("I left my son an orphan, I f*cked my neighbor's p*ssy",
        "Oğlumu yetim koydum, Komşumun a*ına koydum.",
        "Anne");
    }
}
