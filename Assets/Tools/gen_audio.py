"""Synthesises every sound and the music track for CS464 Assignment 04.
Run from the project root:  python Assets/Tools/gen_audio.py
Needs only numpy. Output: 16-bit mono WAV files in Assets/Audio (22.05 kHz).
All sounds are generated from sine, noise and simple envelopes, so there is nothing to license:
they are CC0 (public domain)."""
import os, wave
import numpy as np

SR = 22050
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Audio")
os.makedirs(OUT, exist_ok=True)
rng = np.random.default_rng(464)

def t(sec): return np.arange(int(SR * sec)) / SR
def note(m): return 440.0 * 2 ** ((m - 69) / 12)          # MIDI number -> Hz
def env(n, attack=0.005, decay=6.0):
    x = np.arange(n) / SR
    a = np.minimum(1.0, x / max(attack, 1e-4))
    return a * np.exp(-decay * x)
def sine(f, sec, **kw): return np.sin(2 * np.pi * f * t(sec)) * env(int(SR * sec), **kw)
def bell(f, sec, decay=5.0): return (sine(f, sec, decay=decay) + 0.4 * sine(2.0 * f, sec, decay=decay * 1.5) + 0.2 * sine(3.01 * f, sec, decay=decay * 2))
def sweep(f0, f1, sec, decay=4.0):
    x = t(sec); f = f0 + (f1 - f0) * (x / sec)
    return np.sin(2 * np.pi * np.cumsum(f) / SR) * env(len(x), decay=decay)
def lowpass(x, k=40): return np.convolve(x, np.ones(k) / k, mode="same")
def place(buf, sig, at):
    i = int(SR * at); buf[i:i + len(sig)] += sig[:len(buf) - i]
def norm(x, peak): return x / np.max(np.abs(x)) * peak
def save(name, x, peak=0.8):
    x = norm(x, peak)
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x * 32767).astype("<i2").tobytes())
    print(f"{name}.wav  {len(x)/SR:5.2f}s  first {x[0]:+.3f} last {x[-1]:+.3f}")

# ---------------------------------------------------------------- effects
save("sfx_pickup", np.concatenate([bell(note(88), 0.12, 9), bell(note(95), 0.22, 7)]))
save("sfx_hit", lowpass(rng.normal(size=int(SR * .3)), 12) * env(int(SR * .3), decay=14) + 1.2 * sweep(140, 55, .3, decay=9))
save("sfx_button", sine(880, 0.07, attack=0.001, decay=45) + 0.5 * sine(1320, 0.07, attack=0.001, decay=55))
save("sfx_jump", sweep(320, 640, 0.18, decay=14))
save("sfx_bounce", sweep(180, 760, 0.34, decay=7) * (1 + 0.25 * np.sin(2 * np.pi * 14 * t(0.34))))
save("sfx_checkpoint", np.concatenate([bell(note(79), 0.16, 8), bell(note(84), 0.4, 5)]))
b = np.zeros(int(SR * 0.6))
for i, m in enumerate((72, 76, 79, 84)): place(b, bell(note(m), 0.3, 9), i * 0.07)
save("sfx_powerup", b)
w = np.zeros(int(SR * 1.5))
for i, m in enumerate((72, 76, 79)): place(w, bell(note(m), 0.35, 6), i * 0.18)
place(w, bell(note(84), 0.95, 3.2) + bell(note(88), 0.95, 3.2) * .6, 0.54)
save("sfx_win", w)
l = np.zeros(int(SR * 1.6))
for i, m in enumerate((69, 65, 62, 57)):
    place(l, (sine(note(m), 0.5, decay=3.5) + 0.5 * sine(note(m) * 2, 0.5, decay=5)), i * 0.3)
save("sfx_lose", l)

# ---------------------------------------------------------------- world loops (whole cycles, so they loop with no click)
x = t(2.0)
hum = (np.sin(2 * np.pi * 110 * x) + 0.5 * np.sin(2 * np.pi * 165 * x)) * (0.85 + 0.15 * np.sin(2 * np.pi * 2 * x))   # two soft sines, gentle 2 Hz swell
save("loop_guard_hum", hum, 0.5)
x = t(3.0)
chime = (np.sin(2 * np.pi * 440 * x) + 0.7 * np.sin(2 * np.pi * 660 * x) + 0.3 * np.sin(2 * np.pi * 880 * x)) * (0.65 + 0.35 * np.sin(2 * np.pi * 1 * x))
save("loop_goal_chime", chime, 0.6)

# ---------------------------------------------------------------- music: Am | F | C | G, 80 bpm, 6 s per chord = 24 s loop
chords = [((57, 60, 64), 45), ((53, 57, 60), 41), ((48, 52, 55), 36), ((55, 59, 62), 43)]
beat = 60 / 80
bar = np.zeros(int(SR * 24.0))
for ci, (tones, root) in enumerate(chords):
    start = ci * 6.0
    for m in tones:                                          # soft pad: two slightly detuned sines per tone
        x = t(6.0)
        pad = (np.sin(2 * np.pi * note(m) * x) + np.sin(2 * np.pi * note(m) * 1.003 * x)) * 0.5
        shape = np.minimum(1, x / 0.8) * np.minimum(1, (6.0 - x) / 0.8)
        place(bar, pad * shape * 0.18, start)
    for k in range(8):                                       # bass on every beat
        place(bar, sine(note(root), 0.7, attack=0.01, decay=3.5) * 0.5, start + k * beat)
    for k in range(16):                                      # 8th-note arpeggio
        m = tones[[0, 1, 2, 1][k % 4]] + 12
        place(bar, bell(note(m), 0.4, 7) * 0.22, start + k * beat / 2)
bar[: int(SR * .01)] *= np.linspace(0, 1, int(SR * .01))
save("music_theme", bar, 0.55)
