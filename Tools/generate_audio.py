"""Generate original, deterministic mono PCM sound assets. No downloaded audio."""
import math, random, struct, wave
from pathlib import Path
random.seed(1979)
root = Path(__file__).resolve().parents[1] / 'Assets/Resources/Audio'
root.mkdir(parents=True, exist_ok=True)
rate = 22050
def write(name, seconds, fn):
    data = bytearray()
    n = int(seconds * rate)
    low = 0
    for i in range(n):
        t = i / rate
        noise = random.uniform(-1, 1)
        low = low * .96 + noise * .04
        sample = fn(t, noise, low) * min(1, i/150, (n-i)/300)
        data.extend(struct.pack('<h', int(max(-1, min(1, sample)) * 28000)))
    with wave.open(str(root / (name + '.wav')), 'wb') as wav:
        wav.setparams((1, 2, rate, n, 'NONE', 'not compressed'))
        wav.writeframes(data)
write('ambience', 12, lambda t,n,l: l*.55+n*.018+math.sin(t*2*math.pi*47)*.012)
write('drone', 12, lambda t,n,l: (math.sin(t*math.pi*2*49)+math.sin(t*math.pi*2*50.5)*.6+math.sin(t*math.pi*2*73.5)*.25)*.055)
write('buzz', 3, lambda t,n,l: math.sin(t*math.pi*2*100)*.04+math.sin(t*math.pi*2*200)*.012+n*.006)
write('step', .28, lambda t,n,l: (l*.9+n*.12)*math.exp(-t*23))
write('lighter', .45, lambda t,n,l: n*.32*math.exp(-t*30)+n*.08*(1 if .16<t<.27 else 0))
write('extinguish', .3, lambda t,n,l: n*.10*math.exp(-t*11))
write('door', 1.1, lambda t,n,l: math.sin(t*2*math.pi*(130-t*60))*.045*(1-t/1.1)+(l*2+n*.15)*math.exp(-abs(t-.65)*45))
write('scratch', 2.3, lambda t,n,l: (n*.07+l*.4)*(.3+max(0,math.sin(t*19)))+math.sin(t*math.pi*2*(870+100*math.sin(t*8)))*.012)
write('impact', 1.6, lambda t,n,l: (math.sin(t*2*math.pi*(80-t*25))*.3+n*.3)*math.exp(-t*5))
write('alarm', 3, lambda t,n,l: math.sin(t*math.pi*2*(620+100*math.sin(t*6)))*.08)
write('paper', .5, lambda t,n,l: n*.07*math.sin(t*10)**2)
print('Generated 11 original sound assets')
