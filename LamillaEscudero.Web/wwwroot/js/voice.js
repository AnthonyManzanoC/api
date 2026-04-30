window.voiceChat = (() => {
    let recognition = null;
    let activeDotNetRef = null;
    let activeAudio = null;
    let activeAudioUrl = null;
    let activeSpeechRequestId = 0;
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

    function resetActiveAudio() {
        if (activeAudio) {
            activeAudio.pause();
            activeAudio.currentTime = 0;
            activeAudio = null;
        }

        if (activeAudioUrl) {
            URL.revokeObjectURL(activeAudioUrl);
            activeAudioUrl = null;
        }
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

        async generateAndPlayElevenLabs(text, apiKey, voiceId) {
            const cleanText = typeof text === "string" ? text.trim() : "";
            const cleanApiKey = typeof apiKey === "string" ? apiKey.trim() : "";
            const cleanVoiceId = typeof voiceId === "string" ? voiceId.trim() : "";

            if (!cleanText || !cleanApiKey || !cleanVoiceId) {
                return false;
            }

            const requestId = ++activeSpeechRequestId;

            try {
                const response = await fetch(
                    `https://api.elevenlabs.io/v1/text-to-speech/${encodeURIComponent(cleanVoiceId)}`,
                    {
                        method: "POST",
                        headers: {
                            "xi-api-key": cleanApiKey,
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify({
                            text: cleanText,
                            model_id: "eleven_multilingual_v2"
                        })
                    });

                if (!response.ok) {
                    const errorBody = await response.text();
                    console.error("voiceChat: ElevenLabs devolvio un error.", {
                        status: response.status,
                        body: errorBody
                    });
                    return false;
                }

                const sourceBlob = await response.blob();
                if (requestId !== activeSpeechRequestId) {
                    return false;
                }

                if (!sourceBlob || sourceBlob.size === 0) {
                    console.error("voiceChat: ElevenLabs devolvio un audio vacio.");
                    return false;
                }

                resetActiveAudio();

                const audioBlob = new Blob([sourceBlob], { type: "audio/mpeg" });
                const audioUrl = URL.createObjectURL(audioBlob);
                const audio = new Audio(audioUrl);
                audio.preload = "auto";

                audio.onended = () => {
                    if (activeAudio === audio) {
                        activeAudio = null;
                    }

                    if (activeAudioUrl === audioUrl) {
                        URL.revokeObjectURL(audioUrl);
                        activeAudioUrl = null;
                    }
                };

                audio.onerror = event => {
                    console.error("voiceChat: el navegador no pudo reproducir el audio de ElevenLabs.", event);

                    if (activeAudio === audio) {
                        activeAudio = null;
                    }

                    if (activeAudioUrl === audioUrl) {
                        URL.revokeObjectURL(audioUrl);
                        activeAudioUrl = null;
                    }
                };

                activeAudio = audio;
                activeAudioUrl = audioUrl;

                const playPromise = audio.play();
                if (playPromise && typeof playPromise.then === "function") {
                    await playPromise;
                }

                return true;
            } catch (error) {
                if (requestId === activeSpeechRequestId) {
                    resetActiveAudio();
                }

                console.error("voiceChat: fallo la llamada directa a ElevenLabs.", error);
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

window.generateAndPlayElevenLabs = function (text, apiKey, voiceId) {
    return window.voiceChat.generateAndPlayElevenLabs(text, apiKey, voiceId);
};
