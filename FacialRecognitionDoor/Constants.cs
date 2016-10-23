namespace FacialRecognitionDoor
{
    /// <summary>
    /// General constant variables
    /// </summary>
    public static class GeneralConstants
    {
        // This variable should be set to false for devices, unlike the Raspberry Pi, that have GPU support
        public const bool DisableLiveCameraFeed = true; //False para habilitar Webcam en PC

        // Oxford Face API Primary should be entered here
        // You can obtain a subscription key for Face API by following the instructions here: https://www.microsoft.com/cognitive-services/en-us/sign-up
        public const string OxfordAPIKey = "983f484ba73f4c2e8cde0560e88f5881";
        
        // Name of the folder in which all Whitelist data is stored
        public const string WhiteListFolderName = "Facial Recognition Door Whitelist";

        //Slack URI with token
        public const string SlackURI = "https://hooks.slack.com/services/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";
        
    }

    /// <summary>
    /// Constant variables that hold messages to be read via the SpeechHelper class
    /// </summary>
    public static class SpeechContants
    {
        public const string InitialGreetingMessage = "Bienvenido, el reconocimiento facial de acceso a la sala del tribunal ha sido inicializado.";

        public const string VisitorNotRecognizedMessage = "Disculpa, no te reconozco, por lo que no podré abrir la puerta.";
        public const string NoCameraMessage = "Disculpa, parece que la cámara no está completamente inicializada.";

        public static string GeneralGreetigMessage(string visitorName)
        {
            return "¡Bienvenido a la sala del tribunal " + visitorName + "! Mucha suerte.";
        }
    }

    /// <summary>
    /// Constant variables that hold values used to interact with device Gpio
    /// </summary>
    public static class GpioConstants
    {
        // The GPIO pin that the doorbell button is attached to
        public const int ButtonPinID = 5;

        // The GPIO pin that the door lock is attached to
        public const int DoorLockPinID = 4;

        // The GPIO pin that the door lock is attached to
        public const int LockIntruderPinID = 26;

        // The amount of time in seconds that the door will remain unlocked for
        public const int DoorLockOpenDurationSeconds = 10;
    }
}
