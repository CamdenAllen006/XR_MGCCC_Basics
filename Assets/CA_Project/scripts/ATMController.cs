using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class ATMController : MonoBehaviour
{
    // Assign these in the Inspector.
    // This should be your 3D TextMeshPro text object.
    public TextMeshPro displayText;     
    public GameObject atmModel;         // Reference to the ATM model (if needed for animations/effects).
    public GameObject atmButtons;       // Reference to the button group (if needed).
    public GameObject insertCardButton; // Assign this in the Unity Inspector


    // ATM configuration
    private const int initialBalance = 1000;
    private const int maxPinAttempts = 3;
    private int balance;
    private int pin;
    private int pinAttempts;
    private string[] languages = new string[] { "1", "2" };
    private string currentLanguage;

    // For managing user input from buttons (like digits)
    private string currentInput = "";

    // A simple state machine to handle what the ATM is doing
    private enum ATMState
    {
        WaitingForCard,
        PINEntry,
        MainMenu,
        DepositEntry,
        WithdrawEntry,
        ChangePinEntry,
        LanguageEntry
    }
    private ATMState currentState;

    // Track card status.
    private bool cardInserted = false;

    void Start()
    {
        balance = initialBalance;
        pin = 1234;
        pinAttempts = 0;
        currentLanguage = "en";
        currentState = ATMState.WaitingForCard;
        UpdateDisplay("Please insert your card.");
    }

    // Update the TextMeshPro display.
    void UpdateDisplay(string message)
    {
        if (displayText != null)
            displayText.text = message;
        else
            Debug.Log(message);
    }

    // Called when the physical "Insert Card" button is clicked.
    public void OnInsertCardPressed()
    {
        if (cardInserted) return; // Prevent multiple presses

        cardInserted = true;
        currentState = ATMState.PINEntry;
        currentInput = "";
    
        // Hide the insert card button
        if (insertCardButton != null)
            insertCardButton.SetActive(false);

        UpdateDisplay("Card inserted. Please enter your PIN:");
    }

    // Called when a digit button is clicked.
    public void OnDigitPressed(string digit)
    {
        // Ignore if no card is inserted.
        if (!cardInserted)
            return;
        
        if (currentState == ATMState.MainMenu)
            return;

        currentInput += digit;
        UpdateDisplay(GetCurrentPrompt() + "\n" + currentInput);
    }

    // Called when the Cancel button is clicked.
    public void OnCancelPressed()
    {
        // If we are in the ExitATM state, do nothing
        if (currentState == ATMState.WaitingForCard)
            return;

        currentInput = "";
    
        if (currentState == ATMState.PINEntry)
        {
            cardInserted = false;
            currentState = ATMState.WaitingForCard;
            UpdateDisplay("Card ejected. Please insert your card.");

            // Show the insert card button again
            if (insertCardButton != null)
                insertCardButton.SetActive(true);
        }
        else if (currentState != ATMState.MainMenu)
        {
            currentState = ATMState.MainMenu;
            UpdateDisplay("Operation canceled.");
            ShowMainMenu();
        }
    }

    // Called when the Reset button is clicked.
    public void OnResetPressed()
    {
        // Ignore if no card is inserted.
        if (!cardInserted)
            return;

        // Do nothing if we're in the Main Menu.
        if (currentState == ATMState.MainMenu)
            return;

        currentInput = "";
        UpdateDisplay(GetCurrentPrompt());
    }

    // Called when the Enter button is clicked.
    public void OnEnterPressed()
    {
        // Ignore if no card is inserted.
        if (!cardInserted)
            return;

        switch (currentState)
        {
            case ATMState.PINEntry:
                ProcessPinEntry();
                break;
            case ATMState.DepositEntry:
                ProcessDeposit();
                break;
            case ATMState.WithdrawEntry:
                ProcessWithdraw();
                break;
            case ATMState.ChangePinEntry:
                ProcessChangePin();
                break;
            case ATMState.LanguageEntry:
                ProcessLanguageChange();
                break;
            default:
                break;
        }
    }

    // Called when one of the physical menu option buttons is clicked.
    // Mapping:
    // Option 1: Check Balance, Option 2: Deposit, Option 3: Withdraw,
    // Option 4: Change Language, Option 5: Change PIN, Option 6: Report Lost/Stolen Card, Option 7: Exit.
    public void OnMenuOptionSelected(int option)
    {
        // If we're in a mode (Deposit, Withdraw, etc.), ignore further mode selections.
        if (currentState != ATMState.MainMenu)
            return;

        currentInput = "";
        switch (option)
        {
            case 1:
                ShowBalance();
                break;
            case 2:
                currentState = ATMState.DepositEntry;
                UpdateDisplay("Enter deposit amount:");
                break;
            case 3:
                currentState = ATMState.WithdrawEntry;
                UpdateDisplay("Enter withdrawal amount:");
                break;
            case 4:
                currentState = ATMState.LanguageEntry;
                UpdateDisplay("Enter language code. 1(English) 2(español):");
                break;
            case 5:
                currentState = ATMState.ChangePinEntry;
                UpdateDisplay("Enter new PIN:");
                break;
            case 6:
                ReportLostStolenCard();
                break;
            case 7:
                ExitATM();
                break;
            default:
                UpdateDisplay("Invalid selection.");
                break;
        }
    }

    void ProcessPinEntry()
    {
        int enteredPin;
        if (int.TryParse(currentInput, out enteredPin))
        {
            if (enteredPin == pin)
            {
                pinAttempts = 0;
                currentState = ATMState.MainMenu;
                ShowMainMenu();
            }
            else
            {
                pinAttempts++;
                if (pinAttempts >= maxPinAttempts)
                {
                    UpdateDisplay("Too many incorrect attempts. Card retained. Exiting...");
                    // Close the application. If in the Unity Editor, stop play mode.
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
                else
                {
                    UpdateDisplay("Incorrect PIN. Please try again:");
                    currentInput = "";
                }
            }
        }
        else
        {
            UpdateDisplay("Invalid PIN entry. Please try again:");
            currentInput = "";
        }
    }

    void ProcessDeposit()
    {
        int depositAmount;
        if (int.TryParse(currentInput, out depositAmount))
        {
            balance += depositAmount;
            UpdateDisplay("Deposit successful. New balance: " + balance);
            currentState = ATMState.MainMenu;
            currentInput = "";
            StartCoroutine(WaitAndShowMenu(3.0f));
        }
        else
        {
            UpdateDisplay("Invalid amount. Please enter a valid number:");
            currentInput = "";
        }
    }

    void ProcessWithdraw()
    {
        int withdrawAmount;
        if (int.TryParse(currentInput, out withdrawAmount))
        {
            if (withdrawAmount <= balance)
            {
                balance -= withdrawAmount;
                UpdateDisplay("Please take your cash. New balance: " + balance);
            }
            else
            {
                UpdateDisplay("Insufficient funds.");
            }
            currentState = ATMState.MainMenu;
            currentInput = "";
            StartCoroutine(WaitAndShowMenu(3.0f));
        }
        else
        {
            UpdateDisplay("Invalid amount. Please enter a valid number:");
            currentInput = "";
        }
    }

    void ProcessChangePin()
    {
        int newPin;
        if (int.TryParse(currentInput, out newPin))
        {
            int oldPin = pin;
            pin = newPin;
            UpdateDisplay("PIN changed. Old PIN: " + oldPin + " New PIN: " + pin);
            currentState = ATMState.MainMenu;
            currentInput = "";
            StartCoroutine(WaitAndShowMenu(3.0f));
        }
        else
        {
            UpdateDisplay("Invalid PIN. Please enter a valid number:");
            currentInput = "";
        }
    }

    void ProcessLanguageChange()
    {
        string selectedLanguage = currentInput.ToLower();
        if (selectedLanguage == "1" || selectedLanguage == "2")
        {
            currentLanguage = selectedLanguage;
            UpdateDisplay("Language set to " + (selectedLanguage == "1" ? "English" : "español"));
            currentState = ATMState.MainMenu;
            currentInput = "";
            StartCoroutine(WaitAndShowMenu(3.0f));
        }
        else
        {
            UpdateDisplay("Invalid language selection. Enter 1 or 2:");
            currentInput = "";
        }
    }

    // Updated ShowBalance now displays the balance, waits a few seconds, then shows the main menu.
    void ShowBalance()
    {
        UpdateDisplay("Your balance is: " + balance);
        StartCoroutine(WaitAndShowMenu(3.0f));
    }

    IEnumerator WaitAndShowMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowMainMenu();
    }

    void ReportLostStolenCard()
    {
        UpdateDisplay("PIN and Card details reported stolen. Card is deactivated.");
        cardInserted = false;
        currentState = ATMState.WaitingForCard;
        currentInput = "";
        // Close the application. If in the Unity Editor, stop play mode.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ExitATM()
    {
        UpdateDisplay("Thank you for using our ATM. Please take your card.");
        cardInserted = false;
        currentState = ATMState.WaitingForCard;
        currentInput = "";

        // Show the insert card button again
        if (insertCardButton != null)
            insertCardButton.SetActive(true);
    }

    // Updated main menu text.
    void ShowMainMenu()
    {
        string menu = "Main Menu:\n" +
                      "L1: Check Balance\n" +
                      "L2: Deposit\n" +
                      "L3: Withdraw\n" +
                      "L4: Change Language\n" +
                      "R1: Change PIN\n" +
                      "R2: Report Stolen Details\n" +
                      "R3: Exit";
        UpdateDisplay(menu);
    }

    string GetCurrentPrompt()
    {
        switch (currentState)
        {
            case ATMState.PINEntry:
                return "Please enter your PIN:";
            case ATMState.DepositEntry:
                return "Enter deposit amount:";
            case ATMState.WithdrawEntry:
                return "Enter withdrawal amount:";
            case ATMState.ChangePinEntry:
                return "Enter new PIN:";
            case ATMState.LanguageEntry:
                return "Enter language code. 1(English) 2(español):";
            case ATMState.MainMenu:
                return "Main Menu:";
            default:
                return "";
        }
    }
}