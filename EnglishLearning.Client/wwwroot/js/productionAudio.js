let active;

export async function record() {
    if (active) throw new Error("Ya hay una grabación.");
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    let recorder;
    try {
        recorder = new MediaRecorder(stream);
    } catch (error) {
        stream.getTracks().forEach(track => track.stop());
        throw error;
    }

    const chunks = [];
    const completed = new Promise((resolve, reject) => {
        recorder.addEventListener("dataavailable", event => chunks.push(event.data));
        recorder.addEventListener("error", reject, { once: true });
        recorder.addEventListener("stop", () => resolve(new Blob(chunks, { type: recorder.mimeType })), { once: true });
    });

    const finish = () => {
        if (recorder.state !== "inactive") recorder.stop();
        stream.getTracks().forEach(track => track.stop());
    };
    const timeout = setTimeout(finish, 30_000);
    active = { finish, timeout, completed };
    recorder.start();
}

export async function stop() {
    if (!active) return null;
    const recording = active;
    active = undefined;
    clearTimeout(recording.timeout);
    recording.finish();
    const original = await recording.completed;
    const decoder = new AudioContext();
    try {
        const decoded = await decoder.decodeAudioData(await original.arrayBuffer());
        const sampleCount = Math.min(480_000, Math.floor(decoded.duration * 16_000));
        if (sampleCount < 16_000) return null;
        const buffer = new OfflineAudioContext(1, sampleCount, 16_000);
        const input = buffer.createBufferSource();
        input.buffer = decoded;
        input.connect(buffer.destination);
        input.start();
        const samples = (await buffer.startRendering()).getChannelData(0);
        return wave(samples);
    } finally {
        await decoder.close();
    }
}

export function cancel() {
    if (!active) return;
    clearTimeout(active.timeout);
    active.finish();
    active = undefined;
}

function wave(samples) {
    const bytes = new Uint8Array(44 + samples.length * 2);
    const view = new DataView(bytes.buffer);
    for (const [offset, text] of [[0, "RIFF"], [8, "WAVE"], [12, "fmt "], [36, "data"]]) {
        bytes.set(new TextEncoder().encode(text), offset);
    }
    view.setUint32(4, bytes.length - 8, true);
    view.setUint32(16, 16, true);
    view.setUint16(20, 1, true);
    view.setUint16(22, 1, true);
    view.setUint32(24, 16_000, true);
    view.setUint32(28, 32_000, true);
    view.setUint16(32, 2, true);
    view.setUint16(34, 16, true);
    view.setUint32(40, samples.length * 2, true);
    for (let i = 0; i < samples.length; i++) {
        const sample = Math.max(-1, Math.min(1, samples[i]));
        view.setInt16(44 + i * 2, sample * (sample < 0 ? 32768 : 32767), true);
    }
    return bytes;
}
