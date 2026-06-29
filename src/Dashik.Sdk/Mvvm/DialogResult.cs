namespace Dashik.Sdk.Mvvm;

/// <summary>
/// Specifies identifiers to indicate the return value of a dialog box.
/// </summary>
public enum DialogResult
{
    /// <summary>
    /// No result or user dismissed dialog without selecting an option.
    /// May indicate dialog was closed by clicking outside or pressing Escape.
    /// </summary>
    None,

    /// <summary>
    /// User chose "Cancel" option or dismissed the dialog.
    /// Indicates operation should be aborted or no action taken.
    /// Often the result of Escape key, Cancel button, or dialog dismissal.
    /// </summary>
    Cancel,

    /// <summary>
    /// User chose "OK" option in an information or prompt dialog.
    /// Indicates acknowledgement or confirmation of presented information.
    /// Used in single-choice dialogs where user just acknowledges content.
    /// </summary>
    OK,

    /// <summary>
    /// The dialog box return value is Abort (usually sent from a button labeled Abort).
    /// </summary>
    Abort,

    /// <summary>
    /// The dialog box return value is Retry (usually sent from a button labeled Retry).
    /// </summary>
    Retry,

    /// <summary>
    /// The dialog box return value is Ignore (usually sent from a button labeled Ignore).
    /// </summary>
    Ignore,

    /// <summary>
    /// User chose "Yes" option in a confirmation dialog.
    /// Typically, indicates affirmative action should be taken.
    /// Commonly used with "Are you sure?" type prompts.
    /// </summary>
    Yes,

    /// <summary>
    /// User chose "No" option in a confirmation dialog.
    /// Typically, indicates negative response to a question or prompt.
    /// Commonly used with "Are you sure?" type prompts.
    /// </summary>
    No,

    /// <summary>
    /// The dialog box return value is Try Again (usually sent from a button labeled Try Again).
    /// </summary>
    TryAgain,

    /// <summary>
    /// The dialog box return value is Continue (usually sent from a button labeled Continue).
    /// </summary>
    Continue,
}
