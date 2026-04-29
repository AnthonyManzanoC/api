window.voiceChat = (() => {
    let recognition = null;
    let activeDotNetRef = null;
    let activeAudio = null;
    let audioUnlocked = false;

    const SpeechRecognitionCtor =
        window.SpeechRecognition || window.webkitSpeechRecognition || null;
    const silentAudioDataUrl =
        "data:audio/wav;base64,UklGRiQAAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQAAAAA=";

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

    function registerAudioUnlock() {
        const unlockAudio = async () => {
            if (audioUnlocked) {
                return;
            }

            try {
                const probe = new Audio(silentAudioDataUrl);
                probe.volume = 0;

                const playPromise = probe.play();
                if (playPromise && typeof playPromise.then === "function") {
                    await playPromise;
                }

                probe.pause();
                probe.currentTime = 0;
                audioUnlocked = true;
            } catch (error) {
                console.error("voiceChat: no se pudo desbloquear el audio del navegador.", error);
            }
        };

        ["pointerdown", "touchstart", "keydown"].forEach(eventName => {
            document.addEventListener(eventName, unlockAudio, { capture: true, passive: true });
        });
    }

    function normalizeBase64Audio(base64Audio) {
        if (!base64Audio || typeof base64Audio !== "string") {
            return "";
        }

        return base64Audio
            .replace(/^data:audio\/[a-z0-9.+-]+;base64,/i, "")
            .replace(/\s+/g, "")
            .trim();
    }

    async function notify(methodName, value) {
        if (!activeDotNetRef) {
            return;
        }

        try {
            await activeDotNetRef.invokeMethodAsync(methodName, value);
        } catch {
            // El componente pudo haberse desmontado.
        }
    }

    function stopRecognition() {
        if (recognition) {
            recognition.onstart = null;
            recognition.onresult = null;
            recognition.onerror = null;
            recognition.onend = null;
            recognition.stop();
            recognition = null;
        }
    }

    registerAudioUnlock();

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

        stopRecognition() {
            stopRecognition();
            activeDotNetRef = null;
        },

        async playAudioFromBase64(base64Audio) {
            const normalizedBase64 = normalizeBase64Audio(base64Audio);
            if (!normalizedBase64) {
                console.error("voiceChat: el audio TTS llego vacio o mal formateado.");
                return false;
            }

            if (activeAudio) {
                activeAudio.pause();
                activeAudio.currentTime = 0;
            }

            try {
                activeAudio = new Audio(`data:audio/mpeg;base64,${normalizedBase64}`);
                activeAudio.preload = "auto";
                activeAudio.onerror = event => {
                    console.error("voiceChat: el navegador no pudo cargar el audio base64.", event);
                };

                const playPromise = activeAudio.play();
                if (playPromise && typeof playPromise.then === "function") {
                    await playPromise;
                }

                return true;
            } catch (error) {
                console.error("voiceChat: fallo la reproduccion del audio TTS.", {
                    error,
                    base64Length: normalizedBase64.length,
                    base64Preview: normalizedBase64.slice(0, 32)
                });
                activeAudio = null;
                return false;
            }
        },

        scrollToBottom(element) {
            if (!element) {
                return;
            }

            element.scrollTop = element.scrollHeight;
        }
    };
})();

window.playAudioBase64 = function (base64Audio) {
    return window.voiceChat.playAudioFromBase64(base64Audio);
};
