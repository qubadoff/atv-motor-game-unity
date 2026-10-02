using UnityEngine;
using UnityEngine.UI;

/// <summary>Ekran ustu gaz/fren pedali: basiliyken siyaha doner. Giris AtvController'dan okunur.</summary>
public class PedalButton : MonoBehaviour
{
    public AtvController atv;
    public bool isGas;
    public Image fill;
    public Image icon;
    public Text label;

    static readonly Color IdleFill = new Color(1f, 1f, 1f, 0.75f);

    void Update()
    {
        bool pressed = atv != null && atv.ControlsEnabled && (isGas ? atv.Throttle > 0.01f : atv.Throttle < -0.01f);
        if (fill != null) fill.color = pressed ? Color.black : IdleFill;
        if (icon != null) icon.color = pressed ? Color.white : Color.black;
        if (label != null) label.color = pressed ? Color.white : Color.black;
    }
}
