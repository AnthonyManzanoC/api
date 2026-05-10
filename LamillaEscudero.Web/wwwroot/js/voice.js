if (window.speechSynthesis && typeof window.speechSynthesis.getVoices === "function") {
    window.speechSynthesis.getVoices();
}

window.voiceChat = (() => {
    let recognition = null;
    let activeDotNetRef = null;
    let audioDesbloqueado = false;

    const defaultSpeechLanguage = "es-ES";
    const SpeechRecognitionCtor =
        window.SpeechRecognition || window.webkitSpeechRecognition || null;

    function mapError(error) {
        switch (error) {
            case "not-allowed":
                return "No se concedio permiso para usar el microfono.";
            case "audio-capture":
                return "No se detecto un microfono disponible en este dispositivo.";
            case "network":
                return "El navegador no pudo completar el reconocimiento de voz.";
            default:
                return "No se pudo procesar el dictado por voz. Puedes escribir tu consulta manualmente.";
        }
    }

    async function notify(methodName, value) {
        if (!activeDotNetRef) {
            return;
        }

        try {
            await activeDotNetRef.invokeMethodAsync(methodName, value);
        } catch {
        }
    }

    function stopRecognition() {
        if (!recognition) {
            return;
        }

        const currentRecognition = recognition;
        recognition = null;
        currentRecognition.onstart = null;
        currentRecognition.onresult = null;
        currentRecognition.onerror = null;
        currentRecognition.onend = null;

        try {
            currentRecognition.stop();
        } catch {
        }
    }

    function getSpeechSynthesis() {
        return window.speechSynthesis || null;
    }

    function desbloquearAudio(language) {
        if (audioDesbloqueado) {
            return true;
        }

        const synth = getSpeechSynthesis();

        if (!synth
            || typeof synth.speak !== "function"
            || typeof window.SpeechSynthesisUtterance !== "function") {
            return false;
        }

        try {
            const utterance = new SpeechSynthesisUtterance(" ");
            utterance.lang = language || defaultSpeechLanguage;
            utterance.volume = 0;
            utterance.rate = 1.0;
            audioDesbloqueado = true;
            synth.speak(utterance);
            return true;
        } catch {
            return false;
        }
    }

    function desbloquearAudioDesdeInteraccion() {
        desbloquearAudio(defaultSpeechLanguage);
    }

    function registrarDesbloqueoAudio() {
        if (window.__voiceChatAudioUnlockRegistered
            || typeof document === "undefined"
            || typeof document.addEventListener !== "function") {
            return;
        }

        window.__voiceChatAudioUnlockRegistered = true;
        const unlockEventName = window.PointerEvent ? "pointerdown" : "click";
        document.addEventListener(
            unlockEventName,
            desbloquearAudioDesdeInteraccion,
            { capture: true, once: true });
    }

    function reproducirRespuesta(texto) {
        const synth = getSpeechSynthesis();

        if (!synth
            || typeof synth.speak !== "function"
            || typeof window.SpeechSynthesisUtterance !== "function"
            || typeof texto !== "string"
            || !texto.trim()) {
            return false;
        }

        try {
            const utterance = new SpeechSynthesisUtterance(texto);
            utterance.lang = defaultSpeechLanguage;
            utterance.rate = 1.0;
            synth.speak(utterance);
            return true;
        } catch {
            return false;
        }
    }

    registrarDesbloqueoAudio();

    return {
        async startRecognition(inputElement, dotNetRef, language) {
            if (!SpeechRecognitionCtor) {
                return false;
            }

            stopRecognition();
            activeDotNetRef = dotNetRef || null;

            recognition = new SpeechRecognitionCtor();
            recognition.lang = language || "es-EC";
            recognition.continuous = false;
            recognition.interimResults = false;
            recognition.maxAlternatives = 1;

            recognition.onstart = () => {
                notify("OnVoiceStateChanged", true);
            };

            recognition.onresult = async event => {
                const transcript = Array
                    .from(event.results || [])
                    .map(result => result?.[0]?.transcript || "")
                    .join(" ")
                    .trim();

                if (inputElement) {
                    inputElement.value = transcript;
                    inputElement.dispatchEvent(new Event("input", { bubbles: true }));
                    inputElement.focus();
                }

                await notify("OnSpeechRecognized", transcript);
            };

            recognition.onerror = async event => {
                await notify("OnVoiceStateChanged", false);

                if (event?.error !== "aborted" && event?.error !== "no-speech") {
                    await notify("OnVoiceError", mapError(event?.error));
                }
            };

            recognition.onend = () => {
                notify("OnVoiceStateChanged", false);
                recognition = null;
            };

            recognition.start();
            return true;
        },

        desbloquearAudio,

        reproducirRespuesta,

        stopRecognition() {
            stopRecognition();
            activeDotNetRef = null;
        },

        reproducirVoz(texto) {
            return reproducirRespuesta(texto);
        },

        scrollToBottom(element) {
            if (!element) {
                return;
            }

            element.scrollTop = element.scrollHeight;
        }
    };
})();
