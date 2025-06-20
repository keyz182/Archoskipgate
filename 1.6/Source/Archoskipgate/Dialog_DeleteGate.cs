using UnityEngine;
using Verse;

namespace Archoskipgate;

public class Dialog_DeleteGate(GateAddress address, IWindowDrawing customWindowDrawing = null) : Window(customWindowDrawing)
{
    public override Vector2 InitialSize => new(200f, 150f);
    public GDFP_WorldComponent WorldComponent => Find.World.GetComponent<GDFP_WorldComponent>();

    public override void DoWindowContents(Rect inRect)
    {
        closeOnClickedOutside = true;
        RectDivider window = new(inRect, 2527234);

        RectDivider label = window.NewRow(48f);
        Widgets.Label(label, "GDFP_AreYouSure_Label".Translate(address.name));

        RectDivider buttonRow = window.NewRow(32f);
        RectDivider button1 = buttonRow.NewCol(60f);
        RectDivider spacer = buttonRow.NewCol(10f);
        RectDivider button2 = buttonRow.NewCol(60f);

        if (Widgets.ButtonText(button1.Rect, "Yes"))
        {
            WorldComponent.LearnedAddresses.Remove(address);
            Close();
        }
        if (Widgets.ButtonText(button2.Rect, "No"))
        {
            Close();
        }
    }
}
