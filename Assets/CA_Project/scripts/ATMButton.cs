using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ATMButton : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
    public enum ButtonType { Digit, Cancel, Enter, Reset, InsertCard, MenuOption }
    public ButtonType buttonType;

    // For digit buttons (0-9)
    public string digit;

    // For menu option buttons: 1 = Check Balance, 2 = Deposit, etc.
    public int menuOption;

    private ATMController atmController;

    protected override void Awake()
    {
        base.Awake();
        // Look for the ATMController in the scene.
        atmController = FindObjectOfType<ATMController>();
        if (atmController == null)
        {
            Debug.LogError("ATMController not found in the scene. Please ensure one is present.");
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (atmController == null)
            return;

        switch (buttonType)
        {
            case ButtonType.Digit:
                atmController.OnDigitPressed(digit);
                break;
            case ButtonType.Cancel:
                atmController.OnCancelPressed();
                break;
            case ButtonType.Enter:
                atmController.OnEnterPressed();
                break;
            case ButtonType.Reset:
                atmController.OnResetPressed();
                break;
            case ButtonType.InsertCard:
                atmController.OnInsertCardPressed();
                break;
            case ButtonType.MenuOption:
                atmController.OnMenuOptionSelected(menuOption);
                break;
            default:
                break;
        }
    }
}