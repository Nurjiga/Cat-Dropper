"""Generates every sound of Cat Dropper from scratch (no samples, fully original).
Run:  python3 generate_sounds.py <output_folder>     (needs numpy)
"""
import sys, wave, numpy as np
SR = 44100
OUT = sys.argv[1] if len(sys.argv) > 1 else "."
rng = np.random.default_rng(7)

def save(name, x, peak=0.89):
    x = x / (np.abs(x).max() + 1e-9) * peak
    with wave.open(f"{OUT}/{name}.wav", "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x * 32767).astype(np.int16).tobytes())
    print(f"{name}.wav  {len(x)/SR:.2f}s")

def T(d): return np.arange(int(SR * d)) / SR
def phase(f): return 2 * np.pi * np.cumsum(f) / SR
def interp(t, pts):
    xs, ys = zip(*pts); return np.interp(t / t[-1], xs, ys)
def fade(x, a=0.004, r=0.01):
    n = len(x); e = np.ones(n); na, nr = int(SR * a), int(SR * r)
    e[:na] = np.linspace(0, 1, na); e[-nr:] = np.linspace(1, 0, nr); return x * e
def lowpass(x, fc):
    X = np.fft.rfft(x); f = np.fft.rfftfreq(len(x), 1 / SR)
    return np.fft.irfft(X / (1 + (f / fc) ** 4), len(x))
def highpass(x, fc):
    X = np.fft.rfft(x); f = np.fft.rfftfreq(len(x), 1 / SR)
    return np.fft.irfft(X * (f / fc) ** 4 / (1 + (f / fc) ** 4), len(x))

# ---------- meow: harmonic stack shaped by moving vowel formants (m -> e -> a -> ow) ----------
def meow(dur, f_pts, breath=0.03, vib=0.012, seed=0):
    t = T(dur); u = t / t[-1]
    f0 = interp(t, f_pts) * (1 + vib * np.sin(2 * np.pi * 6.5 * t) * u)
    ph = phase(f0)
    F1 = np.interp(u, [0, .12, .45, 1], [350, 600, 1000, 480])
    F2 = np.interp(u, [0, .12, .45, 1], [1900, 2500, 1700, 950])
    F3 = 3400.0
    x = np.zeros_like(t)
    for k in range(1, 13):
        fk = f0 * k
        g = (np.exp(-((fk - F1) / 420) ** 2) + 0.7 * np.exp(-((fk - F2) / 520) ** 2)
             + 0.25 * np.exp(-((fk - F3) / 700) ** 2) + 0.10)
        x += g / k ** 0.6 * np.sin(k * ph + k) * (fk < SR / 2.2)
    env = np.interp(u, [0, .07, .3, .75, 1], [0, .75, 1, .55, 0]) ** 1.3
    n = highpass(np.random.default_rng(seed).normal(0, 1, len(t)), 2500) * breath * env
    return fade(x * env + n * np.abs(x).max())

save("meow_1", meow(0.36, [(0, 760), (.15, 930), (.5, 880), (1, 740)], seed=1))
save("meow_2", meow(0.42, [(0, 700), (.2, 880), (.6, 800), (1, 650)], seed=2))
save("meow_3", meow(0.30, [(0, 850), (.12, 1040), (.5, 980), (1, 860)], seed=3))
save("meow_4", meow(0.50, [(0, 620), (.25, 820), (.55, 840), (1, 600)], seed=4))
save("meow_sad", meow(1.05, [(0, 640), (.12, 700), (.45, 560), (1, 390)], breath=0.05, vib=0.03, seed=5), 0.8)
save("meow_happy", np.concatenate([meow(0.2, [(0, 800), (.5, 1000), (1, 1150)], seed=6), np.zeros(int(SR * .05)),
                                   meow(0.42, [(0, 900), (.2, 1250), (.6, 1150), (1, 900)], seed=7)]))

# ---------- small effects ----------
def sweep(dur, f_a, f_b, curve=1.0, harm=()):
    t = T(dur); u = (t / t[-1]) ** curve
    f = f_a * (f_b / f_a) ** u; ph = phase(f)
    x = np.sin(ph)
    for k, a in harm: x += a * np.sin(k * ph)
    return x, t

x, t = sweep(0.11, 330, 980, 0.7, [(2, .25)]);  save("pop", fade(x * np.exp(-t * 16), .002, .02))
x, t = sweep(0.16, 820, 300, 0.8, [(2, .15)]);  save("drop", fade(x * np.exp(-t * 11), .004, .03), 0.7)
x, t = sweep(0.14, 170, 62, 0.5)
thud = x * np.exp(-t * 28) + 0.25 * lowpass(rng.normal(0, 1, len(t)), 900) * np.exp(-t * 90)
save("land", fade(thud, .001, .03), 0.8)
t = T(0.055); click = (np.sin(2 * np.pi * 1250 * t) + .5 * np.sin(2 * np.pi * 1880 * t)) * np.exp(-t * 85)
save("click", fade(click, .0008, .01), 0.6)
t = T(0.09); x1 = np.sin(2 * np.pi * 660 * t) * np.exp(-t * 40); x2 = np.sin(2 * np.pi * 990 * t) * np.exp(-t * 40)
save("open", fade(np.concatenate([x1, x2]), .001, .02), 0.55)
save("close", fade(np.concatenate([x2, x1]), .001, .02), 0.55)

# ---------- pitched instruments ----------
def hz(m): return 440.0 * 2 ** ((m - 69) / 12)
def marimba(m, dur, vel=1.0):
    t = T(dur + 0.25); f = hz(m)
    x = np.sin(2 * np.pi * f * t) * np.exp(-t * 7) + .35 * np.sin(2 * np.pi * 4 * f * t) * np.exp(-t * 22) \
        + .12 * np.sin(2 * np.pi * 9.2 * f * t) * np.exp(-t * 45)
    return fade(x, .002, .05) * vel
def tuba(m, dur, vel=1.0):
    t = T(dur); f = hz(m); ph = 2 * np.pi * f * t
    x = sum(np.sin(k * ph) / k ** 1.4 for k in range(1, 9))
    env = np.minimum(1, t / 0.02) * np.exp(-t * 2.5)
    return fade(x * env, .005, .04) * vel
def pluck(m, dur, vel=1.0):
    t = T(dur + 0.1); f = hz(m); ph = 2 * np.pi * f * t
    x = sum(np.sin(k * ph) * np.exp(-t * (9 + 3 * k)) / k for k in range(1, 7))
    return fade(x, .002, .04) * vel
def whistle(m, dur, vel=1.0):
    t = T(dur); f = hz(m) * (1 + .006 * np.sin(2 * np.pi * 5.5 * t))
    x = np.sin(phase(f)) + .08 * np.sin(2 * phase(f))
    return fade(x, .03, .06) * vel
def shaker(dur=0.06, vel=1.0):
    t = T(dur); return highpass(rng.normal(0, 1, len(t)), 5000) * np.exp(-t * 60) * vel
def block(vel=1.0):
    t = T(0.06); return np.sin(2 * np.pi * 1700 * t) * np.exp(-t * 70) * vel

def brass(m_a, m_b, dur, wah=True):
    """sad-trombone note sliding from m_a to m_b"""
    t = T(dur); u = t / t[-1]
    f = hz(m_a) * (hz(m_b) / hz(m_a)) ** (u ** 2) * (1 + .012 * np.sin(2 * np.pi * 5 * t) * u)
    ph = phase(f); x = sum(np.sin(k * ph) / k for k in range(1, 15))
    env = np.minimum(1, t / 0.03) * np.interp(u, [0, .7, 1], [1, .85, 0])
    x = x * env
    if wah:  # closing mute: bright -> dull
        bright = lowpass(x, 2600); dull = lowpass(x, 500)
        x = bright * (1 - u) + dull * u
    return fade(x, .01, .05)

# ---------- game over: "wah wah wah waaaah" ----------
go = np.concatenate([brass(58, 57.6, .42), np.zeros(int(SR * .05)), brass(57, 56.6, .42), np.zeros(int(SR * .05)),
                     brass(56, 55.6, .42), np.zeros(int(SR * .05)), brass(55, 53.2, 1.5)])
save("gameover", go, 0.8)

# ---------- legendary fanfare ----------
def mix(events, total):
    buf = np.zeros(int(SR * total) + SR)
    for start, x in events:
        i = int(SR * start); buf[i:i + len(x)] += x
    return buf
ev = [(i * .09, marimba(n, .3)) for i, n in enumerate([72, 76, 79, 84, 88, 91, 96])]
ev += [(.63, marimba(n, 1.0, .6)) for n in (84, 88, 91)]
ev += [(.1 + i * .07, block(.15)) for i in range(8)]
save("legendary", fade(mix(ev, 1.9)[:int(SR * 1.9)], .001, .3))

# ---------- silly background loop: 16 bars, 112 BPM, seamless ----------
BPM = 112; B = 60 / BPM; BARS = 16; LEN = int(SR * B * 4 * BARS)
loop = np.zeros(LEN)
def put(beat, x, gain=1.0):
    i = int(SR * B * beat) % LEN
    idx = (i + np.arange(len(x))) % LEN          # wrap tails so the loop point is click-free
    np.add.at(loop, idx, x * gain)

mel = [  # (beat inside bar, midi, length in beats)
    [(0, 76, .5), (.5, 79, .5), (1, 84, .5), (1.5, 79, .5), (2, 76, .5), (2.5, 79, .5), (3, 72, 1)],
    [(0, 74, .5), (.5, 76, .5), (1, 75, .5), (1.5, 76, .5), (2, 79, 1)],
    [(0, 81, .5), (.5, 77, .5), (1, 72, .5), (1.5, 77, .5), (2, 81, .5), (2.5, 84, .5), (3, 81, 1)],
    [(0, 83, .5), (.5, 79, .5), (1, 74, .5), (1.5, 79, .5), (2, 77, .5), (2.5, 74, .5), (3, 71, 1)],
    [(0, 76, .5), (.5, 79, .5), (1, 84, .5), (1.5, 79, .5), (2, 76, .5), (2.5, 79, .5), (3, 72, 1)],
    [(0, 84, .5), (.5, 81, .5), (1, 76, .5), (1.5, 81, .5), (2, 84, 1), (3, 83, .5), (3.5, 81, .5)],
    [(0, 77, .5), (.5, 74, .5), (1, 81, 1), (2, 79, .5), (2.5, 77, .5), (3, 74, .5), (3.5, 71, .5)],
    [(0, 72, .5), (.5, 76, .5), (1, 79, .5), (1.5, 84, .5), (2.5, 84, .25), (3, 72, 1)],
]
chords = [(48, (64, 67, 72)), (48, (64, 67, 72)), (41, (65, 69, 72)), (43, (62, 67, 71)),
          (48, (64, 67, 72)), (45, (64, 69, 72)), (38, (62, 65, 69)), (48, (64, 67, 72))]
for bar in range(BARS):
    b0 = bar * 4; k = bar % 8; second = bar >= 8
    for beat, m, d in mel[k]:
        put(b0 + beat, marimba(m, d * B), .55)
        if second: put(b0 + beat, whistle(m + 12, d * B * .9), .10)
    root, ch = chords[k]
    if k == 6:   # Dm | G split bar
        put(b0, tuba(38, B * .8), .5); put(b0 + 2, tuba(43, B * .8), .5)
        for n in (62, 65, 69): put(b0 + 1, pluck(n, B * .4), .16)
        for n in (62, 67, 71): put(b0 + 3, pluck(n, B * .4), .16)
    else:
        put(b0, tuba(root, B * .8), .5); put(b0 + 2, tuba(root + 7 if root < 44 else root - 5, B * .8), .45)
        for beat in (1, 3):
            for n in ch: put(b0 + beat, pluck(n, B * .4), .16)
    for e in range(8):
        put(b0 + e * .5, shaker(vel=.9 if e % 2 else .5), .07)
    put(b0 + 1, block(), .10); put(b0 + 3, block(), .10)
    if k == 7: put(b0 + 3.5, block(), .12); put(b0 + 3.75, block(), .12)
save("music", loop, 0.7)
