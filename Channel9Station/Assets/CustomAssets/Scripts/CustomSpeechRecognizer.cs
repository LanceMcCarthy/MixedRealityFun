using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class CustomSpeechRecognizer : MonoBehaviour
{
    //[Tooltip("A text area for the recognizer to display the recognized strings.")]
    public GameObject TextDisplay;
    private DictationRecognizer dictationRecognizer;
    private StringBuilder textSoFar;

    void Awake()
    {
        // Create a new DictationRecognizer and assign it to dictationRecognizer variable.
        dictationRecognizer = new DictationRecognizer();

        // Register for dictationRecognizer.DictationHypothesis and implement DictationHypothesis below
        // This event is fired while the user is talking. As the recognizer listens, it provides text of what it's heard so far.
        dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;

        // Register for dictationRecognizer.DictationResult and implement DictationResult below
        // This event is fired after the user pauses, typically at the end of a sentence. The full recognized string is returned here.
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;

        // Register for dictationRecognizer.DictationComplete and implement DictationComplete below
        // This event is fired when the recognizer stops, whether from Stop() being called, a timeout occurring, or some other error.
        dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;

        // Register for dictationRecognizer.DictationError and implement DictationError below
        // This event is fired when an error occurs.
        dictationRecognizer.DictationError += DictationRecognizer_DictationError;

        // Use this string to cache the text currently displayed in the text box.
        textSoFar = new StringBuilder();
    }

    void Update()
    {

    }

    /// <summary>
    /// Activate speech recognition only when the user looks straight object
    /// </summary>
    public void OnFocusEnter()
    {
        // Don't activate speech recognition if the recognizer is already running
        if (dictationRecognizer.Status != SpeechSystemStatus.Running)
        {
            StartRecording();
        }
    }


    public void OnFocusExit()
    {
        StopRecording();
    }

    /// <summary>
    /// Turns on the dictation recognizer and begins recording audio from the default microphone.
    /// </summary>
    /// <returns>The audio clip recorded from the microphone.</returns>
    public void StartRecording()
    {
        // Start dictationRecognizer
        dictationRecognizer.Start();
        
        TextDisplay.GetComponent<TextMesh>().text = "Dictation is starting. It may take time to display your text the first time, but begin speaking now...";

        Debug.Log("Dictation Recognizer is now " +
                  ((dictationRecognizer.Status == SpeechSystemStatus.Running) ? "on" : "off"));
    }

    /// <summary>
    /// Ends the recording session.
    /// </summary>
    public void StopRecording()
    {
        // Check if dictationRecognizer.Status is Running and stop it if so
        if (dictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            dictationRecognizer.Stop();
        }

        Debug.Log("Dictation Recognizer is now " +
                  ((dictationRecognizer.Status == SpeechSystemStatus.Running) ? "on" : "off"));
    }

    /// <summary>
    /// This event is fired while the user is talking. As the recognizer listens, it provides text of what it's heard so far.
    /// </summary>
    /// <param name="text">The currently hypothesized recognition.</param>
    private void DictationRecognizer_DictationHypothesis(string text)
    {
        // Set DictationDisplay text to be textSoFar and new hypothesized text
        // We don't want to append to textSoFar yet, because the hypothesis may have changed on the next event
        TextDisplay.GetComponent<TextMesh>().text = textSoFar.ToString() + " " + text + "...";
    }


    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        StopRecording();

        // Append textSoFar with latest text
        textSoFar.Append(text);

        // Set DictationDisplay text to be textSoFar
        TextDisplay.GetComponent<TextMesh>().text = textSoFar.ToString();
    }

    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        // If Timeout occurs, the user has been silent for too long.
        // With dictation, the default timeout after a recognition is 20 seconds.
        // The default timeout with initial silence is 5 seconds.
        if (cause == DictationCompletionCause.TimeoutExceeded)
        {
            TextDisplay.GetComponent<TextMesh>().text = "Dictation has timed out. Please press the record button again.";
        }
    }
    
    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        // Set DictationDisplay text to be the error string
        TextDisplay.GetComponent<TextMesh>().text = error + "\nHRESULT: " + hresult;
    }
    
}
