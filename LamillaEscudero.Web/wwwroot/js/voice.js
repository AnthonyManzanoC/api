window.mensajesVoz = Array.isArray(window.mensajesVoz)
    ? window.mensajesVoz
    : [];

window.voiceChat = (() => {
    let recognition = null;
    let activeDotNetRef = null;
    let activeSpeechRequestId = 0;
    let activeSpeechTimeoutId = null;
    let cachedSpanishVoice = null;

    const defaultSpeechLanguage = "es-ES";
    const speechRestartDelayMs = 75;
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

    function cleanSpeechText(text) {
        return typeof text === "string"
            ? text.replace(/\s+/g, " ").trim()
            : "";
    }

    function getSpeechSynthesis() {
        return window.speechSynthesis || null;
    }

    function ensureSpeechReferences() {
        if (!Array.isArray(window.mensajesVoz)) {
            window.mensajesVoz = [];
        }

        return window.mensajesVoz;
    }

    function clearSpeechReferences() {
        ensureSpeechReferences().length = 0;
    }

    function releaseSpeechReference(utterance) {
        const references = ensureSpeechReferences();

        if (references.includes(utterance)) {
            references.length = 0;
        }
    }

    function keepSpeechReference(utterance) {
        ensureSpeechReferences().push(utterance);
    }

    function clearPendingSpeechTimeout() {
        if (activeSpeechTimeoutId !== null) {
            window.clearTimeout(activeSpeechTimeoutId);
            activeSpeechTimeoutId = null;
        }
    }

    function getAvailableVoices() {
        const synth = getSpeechSynthesis();

        if (!synth || typeof synth.getVoices !== "function") {
            return [];
        }

        return synth.getVoices();
    }

    function findSpanishVoice(language) {
        const voices = getAvailableVoices();

        if (!voices.length) {
            return null;
        }

        if (cachedSpanishVoice && voices.includes(cachedSpanishVoice)) {
            return cachedSpanishVoice;
        }

        const requestedLanguage = (language || defaultSpeechLanguage).toLowerCase();
        const normalizeLang = voice => (voice?.lang || "").toLowerCase();
        const exactLocalVoice = voices.find(voice =>
            normalizeLang(voice) === requestedLanguage && voice.localService);
        const exactVoice = voices.find(voice =>
            normalizeLang(voice) === requestedLanguage);
        const localSpanishVoice = voices.find(voice =>
            normalizeLang(voice).startsWith("es-") && voice.localService);
        const spanishVoice = voices.find(voice =>
            normalizeLang(voice).startsWith("es-"));
        const defaultVoice = voices.find(voice => voice.default);

        cachedSpanishVoice =
            exactLocalVoice ||
            exactVoice ||
            localSpanishVoice ||
            spanishVoice ||
            defaultVoice ||
            null;

        return cachedSpanishVoice;
    }

    function splitSpeechText(text) {
        const maxLength = 220;
        const sentences = text.match(/[^.!?;:]+[.!?;:]?/g) || [text];
        const chunks = [];
        let current = "";

        const pushCurrent = () => {
            const value = current.trim();

            if (value) {
                chunks.push(value);
            }

            current = "";
        };

        sentences.forEach(sentence => {
            const cleanSentence = sentence.trim();

            if (!cleanSentence) {
                return;
            }

            if (cleanSentence.length > maxLength) {
                pushCurrent();

                cleanSentence.split(/\s+/).forEach(word => {
                    if ((current + " " + word).trim().length > maxLength) {
                        pushCurrent();
                    }

                    current = `${current} ${word}`.trim();
                });

                pushCurrent();
                return;
            }

            if ((current + " " + cleanSentence).trim().length > maxLength) {
                pushCurrent();
            }

            current = `${current} ${cleanSentence}`.trim();
        });

        pushCurrent();

        return chunks.length ? chunks : [text];
    }

    function cancelNativeSpeech() {
        const synth = getSpeechSynthesis();
        activeSpeechRequestId++;
        clearPendingSpeechTimeout();
        clearSpeechReferences();

        if (synth && typeof synth.cancel === "function") {
            synth.cancel();
        }
    }

    function desbloquearAudio(language) {
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
            utterance.rate = 1;
            utterance.pitch = 1;

            if (typeof synth.resume === "function") {
                synth.resume();
            }

            synth.speak(utterance);
            return true;
        } catch {
            return false;
        }
    }

    function reproducirRespuesta(text, language) {
        const synth = getSpeechSynthesis();

        if (!synth
            || typeof synth.speak !== "function"
            || typeof synth.cancel !== "function"
            || typeof window.SpeechSynthesisUtterance !== "function") {
            return false;
        }

        const requestId = ++activeSpeechRequestId;
        const speechLanguage = language || defaultSpeechLanguage;

        clearPendingSpeechTimeout();
        clearSpeechReferences();
        synth.cancel();

        const cleanText = cleanSpeechText(text);

        if (!cleanText) {
            return false;
        }

        const voice = findSpanishVoice(speechLanguage);
        const chunks = splitSpeechText(cleanText);
        let chunkIndex = 0;

        const speakNextChunk = () => {
            if (requestId !== activeSpeechRequestId) {
                return;
            }

            const chunk = chunks[chunkIndex];

            if (!chunk) {
                return;
            }

            activeSpeechTimeoutId = window.setTimeout(() => {
                activeSpeechTimeoutId = null;

                if (requestId !== activeSpeechRequestId) {
                    return;
                }

                const utterance = new SpeechSynthesisUtterance(chunk);
                utterance.lang = voice?.lang || speechLanguage;
                utterance.rate = 0.95;
                utterance.pitch = 1;
                utterance.volume = 1;

                if (voice) {
                    utterance.voice = voice;
                }

                utterance.onend = () => {
                    releaseSpeechReference(utterance);

                    if (requestId !== activeSpeechRequestId) {
                        return;
                    }

                    chunkIndex++;
                    speakNextChunk();
                };

                utterance.onerror = event => {
                    releaseSpeechReference(utterance);

                    if (requestId !== activeSpeechRequestId
                        || event?.error === "canceled"
                        || event?.error === "interrupted") {
                        return;
                    }

                    console.error("voiceChat: fallo la sintesis de voz nativa.", event);
                };

                keepSpeechReference(utterance);
                synth.speak(utterance);

                if (typeof synth.resume === "function") {
                    synth.resume();
                }
            }, speechRestartDelayMs);
        };

        speakNextChunk();

        return true;
    }

    if (window.speechSynthesis) {
        window.speechSynthesis.onvoiceschanged = () => {
            cachedSpanishVoice = null;
        };
    }

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

        reproducirVoz(text, language) {
            return reproducirRespuesta(text, language);
        },

        cancelSpeech() {
            cancelNativeSpeech();
        },

        scrollToBottom(element) {
            if (!element) {
                return;
            }

            element.scrollTop = element.scrollHeight;
        }
    };
})();
