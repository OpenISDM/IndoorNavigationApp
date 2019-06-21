using AVFoundation;
using IndoorNavigation.iOS;
using IndoorNavigation.Models;

[assembly: Xamarin.Forms.Dependency(typeof(TextToSpeechImplementation))]
namespace IndoorNavigation.iOS
{
    public class TextToSpeechImplementation : ITextToSpeech
    {
        public TextToSpeechImplementation() { }

        public void Speak(string text, string language)
        {
            var speechSynthesizer = new AVSpeechSynthesizer();
            var speechUtterance = new AVSpeechUtterance(text)
            {
                Rate = AVSpeechUtterance.MaximumSpeechRate / 2,
                Voice = AVSpeechSynthesisVoice.FromLanguage(language),
                Volume = 1f,
                PitchMultiplier = 1.0f
            };

            speechSynthesizer.SpeakUtterance(speechUtterance);
        }
    }
}
