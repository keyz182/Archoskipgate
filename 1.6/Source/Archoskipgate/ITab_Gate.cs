using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Archoskipgate;


[StaticConstructorOnStartup]
public class ITab_Gate: ITab
{

    public static readonly Texture2D TrashIcon = ContentFinder<Texture2D>.Get("UI/GDFP_Trash");
    public static readonly Texture2D RenameIcon = ContentFinder<Texture2D>.Get("UI/GDFP_Rename");
    public ITab_Gate()
    {
        size = new Vector2(300f, 480f);
        labelKey = "GDFP_GateTab";
    }

    public Building_SkipGate Parent => SelThing as Building_SkipGate;
    public static GDFP_WorldComponent worldComponent => Find.World.GetComponent<GDFP_WorldComponent>();

    protected Vector2 ScrollPosition = Vector2.zero;
    public float scrollHeight = 0;

    protected const float LineHeight = 60f;

    protected QuickSearchWidget QuickSearchWidget = new();

    public bool FindSearchMatch(GateAddress address)
    {
        if (address == null) return false;
        if (QuickSearchWidget.filter.Text == "") return true;
        return address.name.ToLower().Contains(QuickSearchWidget.filter.Text.ToLower()) || address.description.ToLower().Contains(QuickSearchWidget.filter.Text.ToLower());
    }

    protected override void FillTab()
    {
        Rect tabRect = new Rect(0.0f, 0.0f, size.x, size.y).ContractedBy(10f);
        Widgets.BeginGroup(tabRect);

        Rect menuRect = new Rect(0.0f, 20f, tabRect.width, tabRect.height - 20f);
        Widgets.DrawMenuSection(menuRect);

        Rect searchWidgetRect = new(menuRect.x + 3f, menuRect.yMin + 26f, (float)(tabRect.width - 16.0 - 6.0), 24f);
        QuickSearchWidget.OnGUI(searchWidgetRect);

        Rect viewRect = new(0.0f, 0.0f, menuRect.width - 20f, scrollHeight);

        menuRect.yMin += 52f;
        menuRect.yMax -= 6f;
        menuRect.xMax -= 4f;

        Widgets.BeginScrollView(menuRect, ref ScrollPosition, viewRect);

        scrollHeight = 0;

        try
        {
            foreach (GateAddress address in worldComponent.LearnedAddresses.Where(FindSearchMatch))
            {
                Rect row = new(5, scrollHeight, viewRect.width - 26f, LineHeight);
                Rect name = new(5, scrollHeight, viewRect.width - 26f, LineHeight / 3);
                Rect decription = new(5, scrollHeight + (LineHeight / 3), viewRect.width - 26f, LineHeight / 3);
                Rect visited = new(5, scrollHeight + 2*(LineHeight / 3), viewRect.width - 26f, LineHeight / 3);

                Widgets.DrawHighlightIfMouseover(row);

                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = Color.white;

                Widgets.Label(name, address.Name.Truncate(name.width));
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;

                Text.Font = GameFont.Tiny;
                Widgets.LabelEllipses(decription, address.Description);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;

                Text.Font = GameFont.Tiny;
                Widgets.LabelEllipses(visited, address.Visited ? "Visited" : "Unvisited");;
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;

                TooltipHandler.TipRegion(row, address.Description);

                bool checkOn = Parent.selectedAddress == address;
                bool flag = checkOn;

                Widgets.Checkbox(new Vector2(name.xMax, scrollHeight), ref flag, 20, paintable: true, disabled: Parent.IsOpen);
                if (!Parent.IsOpen && checkOn != flag)
                    Parent.selectedAddress = address;
                TooltipHandler.TipRegion(new Rect(name.xMax, scrollHeight, 20, 20), "Select Address");

                Rect renameRect = new(row.xMax, scrollHeight + 20, 20f, 20f);
                if (Widgets.ButtonImage(renameRect, RenameIcon))
                {
                    Find.WindowStack.Add(new Dialog_RenameGate(address));
                }
                TooltipHandler.TipRegion(renameRect, "Rename Address");

                Rect buttonRect = new(row.xMax, scrollHeight + 40, 20f, 20f);
                if (Widgets.ButtonImage(buttonRect, TrashIcon) && Parent.selectedAddress != address && !Parent.IsOpen)
                {
                    Find.WindowStack.Add(new Dialog_DeleteGate(address));
                }
                TooltipHandler.TipRegion(buttonRect, "Delete Address");

                scrollHeight += LineHeight + 2f;
            }
        }
        catch (Exception e)
        {
            ModLog.Error("Error drawing Gate tab", e);
        }
        finally
        {
            Widgets.EndScrollView();

            Widgets.EndGroup();
        }
    }
}
